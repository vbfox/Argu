module internal Argu.Serialize

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Text
open Microsoft.FSharp.Reflection

let private writeOpt (writer: BinaryWriter) (value: 'a option) (valueWriter: BinaryWriter -> 'a -> unit) =
    match value with
    | Some value ->
        writer.Write(true)
        valueWriter writer value
    | None ->
        writer.Write(false)

let private writeSeq (writer: BinaryWriter) (value: 'a seq) (valueWriter: BinaryWriter -> 'a -> unit) =
    let collection =
        if value :? ICollection<'a> then
            value :?> ICollection<'a>
        else
            value |> Array.ofSeq :> ICollection<'a>

    writer.Write(collection.Count)
    for item in collection do
        valueWriter writer item

let inline private writeString (writer: BinaryWriter) (s: string) =
    writer.Write(s)

let inline private writeOptString (writer: BinaryWriter) (s: string option) =
    writeOpt writer s writeString

let private writeHelpParam (writer: BinaryWriter) (helpParam: HelpParam) =
    writeSeq writer helpParam.Flags writeString
    writeString writer helpParam.Description
    ()

let private writeFieldParserInfo (writer: BinaryWriter) (fieldParserInfo: FieldParserInfo) =
    writeOptString writer fieldParserInfo.Label
    writer.Write(fieldParserInfo.Type.AssemblyQualifiedName)
    ()

let private beginBlock (writer: BinaryWriter) =
    let stream = new MemoryStream()
    let newWriter = new BinaryWriter(stream, Encoding.UTF8)
    let disposable =
        { new IDisposable with
            member __.Dispose() =
                newWriter.Flush()
                stream.Position <- 0L
                writer.Write(stream.Length)
                writer.Flush()
                stream.CopyTo(writer.BaseStream)
                newWriter.Dispose()
                stream.Dispose() }

    newWriter, disposable

let rec private writeParameterInfo (writer: BinaryWriter) (parameterInfo: ParameterInfo) =
    writer.Write(byte parameterInfo.Type)

    let writer, disposable = beginBlock writer
    use __ = disposable

    match parameterInfo with
    | Primitives fieldParserInfos ->
        writeSeq writer fieldParserInfos writeFieldParserInfo
    | OptionalParam (existential, fieldParserInfo) ->
        writer.Write(existential.Type.AssemblyQualifiedName)
        writeOpt writer fieldParserInfo.Label writeString
    | ListParam (existential, fieldParserInfo) ->
        writer.Write(existential.Type.AssemblyQualifiedName)
        writeOpt writer fieldParserInfo.Label writeString
    | SubCommand (shape, argInfo, label) ->
        writer.Write(shape.Type.AssemblyQualifiedName)
        writeUnionArgInfo writer argInfo
        writeOptString writer label

and private writeCase (writer: BinaryWriter) (case: UnionCaseArgInfo) =
    writer.Write(case.Name.Value)
    writer.Write(case.Depth)
    writer.Write(case.Arity)
    writer.Write(case.UnionCaseInfo.Value.DeclaringType.AssemblyQualifiedName)
    writer.Write(case.UnionCaseInfo.Value.Tag)
    writeParameterInfo writer case.ParameterInfo.Value
    // GetParent
    // CaseCtor
    let caseCtorInfo = FSharpValue.PreComputeUnionConstructorInfo case.UnionCaseInfo.Value
    writer.Write(caseCtorInfo.Name)
    // FieldCtor
    (*let fieldTypes = case.UnionCaseInfo.Value.GetFields() |> Array.map (fun f -> f.PropertyType)
    let fieldCtorInfoName =
        match fieldTypes.Length with
        | 0 -> None
        | _ ->
            let tupleType = FSharpType.MakeTupleType fieldTypes
            let typeInfo, ctorInfo = FSharpValue.PreComputeTupleConstructorInfo tupleType
            Some (typeInfo.Name)*)
    // FieldReader
    writeSeq writer case.CommandLineNames.Value writeString
    writeOptString writer case.AppSettingsName.Value
    writeString writer case.Description.Value
    writeSeq writer case.AppSettingsSeparators writeString
    writer.Write(int case.AppSettingsSplitOptions)
    writeOpt writer case.CustomAssignmentSeparator.Value writeString
    // AssignmentParser
    writer.Write(int case.CliPosition.Value)
    writeOptString writer case.MainCommandName.Value
    writer.Write(case.IsRest.Value)
    writer.Write(case.AppSettingsCSV.Value)
    writer.Write(case.IsMandatory.Value)
    writer.Write(case.IsInherited.Value)
    writer.Write(case.IsUnique.Value)
    writer.Write(case.IsHidden.Value)
    writer.Write(case.IsGatherUnrecognized.Value)
    writer.Write(case.GatherAllSources.Value)
    ()

and private writeUnionArgInfo (writer: BinaryWriter) (info: UnionArgInfo) =
    writer.Write(info.Type.Value.AssemblyQualifiedName)
    writer.Write(info.Depth)
    writeSeq writer info.Cases.Value writeCase
    writeHelpParam writer info.HelpParam
    writer.Write(info.ContainsSubcommands.Value)
    writer.Write(info.IsRequiredSubcommand.Value)
    writeSeq writer info.InheritedParams.Value writeCase
    writeOptString writer info.GroupedSwitchRegex.Value
    writeSeq writer info.AppSettingsParamIndex.Value (fun _ pair ->
        writer.Write(pair.Key)
        writeCase writer pair.Value)

    writeSeq writer info.CliParamIndex.Value (fun _ (key,value) ->
        writer.Write(key)
        writeCase writer value)
    writeOpt writer info.UnrecognizedGatherParam.Value writeCase
    writeOpt writer info.MainCommandParam.Value writeCase

let private serialize (info: UnionArgInfo) =
    use stream = new MemoryStream()
    use gzip = new GZipStream(stream, CompressionMode.Compress)
    use writer = new BinaryWriter(gzip, Encoding.UTF8)

    writeUnionArgInfo writer info
    writer.Dispose()
    gzip.Dispose()
    stream.ToArray()

let save (info: UnionArgInfo) =
    Convert.ToBase64String(serialize info)

open System.Runtime.CompilerServices


let private  readArray<'a> (reader: BinaryReader) (elementReader: BinaryReader -> 'a): 'a[] =
    let length = reader.ReadInt32()
    let result = Array.zeroCreate<'a> length
    for i = 0 to length-1 do
        result.[i] <- elementReader reader
    result

let private readOpt<'a> (reader: BinaryReader) (elementReader: BinaryReader -> 'a): 'a option =
    let isSome = reader.ReadBoolean()
    if isSome then
        let value = elementReader reader
        Some value
    else
        None

let inline private readString (reader: BinaryReader) =
    reader.ReadString()

let inline private readOptString (reader: BinaryReader) =
    readOpt reader readString

let private readShapeArgumentTemplate (reader: BinaryReader): Lazy<ShapeArgumentTemplate> =
    let typeName = reader.ReadString()
    lazy(
        let type' = System.Type.GetType(typeName)
        ShapeArgumentTemplate.FromType(type'))

let private readExistential (reader: BinaryReader): Lazy<Existential> =
    let typeName = reader.ReadString()
    lazy(
        let type' = System.Type.GetType(typeName)
        Existential.FromType(type'))

let private readFieldParserInfo (reader: BinaryReader): Lazy<FieldParserInfo> =
    let label = readOptString reader
    let typeName = reader.ReadString()
    lazy(
        let type' = System.Type.GetType(typeName)
        getPrimitiveParserByType label type')

let private readHelpParam (reader: BinaryReader): HelpParam =
    let flags = readArray reader readString |> List.ofArray
    let description = readString reader
    {
        Flags = flags
        Description = description
    }

let inline private lazyConst x =
    Lazy<_>(Func<_>(fun () -> x), false)

let blockAsLazy (reader: BinaryReader) (inner: BinaryReader -> 'a): Lazy<'a> =
    let blockSize = reader.ReadInt64()
    let start = reader.BaseStream.Position
    reader.BaseStream.Seek(blockSize, SeekOrigin.Current) |> ignore
    lazy(
        reader.BaseStream.Position <- start
        inner reader
    )

let rec private readParameterInfo (tryGetCurrent : unit -> UnionCaseArgInfo option) (reader: BinaryReader): ArgumentType * Lazy<ParameterInfo> =
    let case = LanguagePrimitives.EnumOfValue<_, ArgumentType> (int (reader.ReadByte()))
    let parameterInfo = blockAsLazy reader (fun (reader: BinaryReader) ->
        match case with
        | ArgumentType.Primitive ->
            let infos = readArray reader readFieldParserInfo
            let infos = infos |> Array.map (fun l -> l.Value)
            Primitives infos
        | ArgumentType.Optional ->
            let existential = readExistential reader
            let label = readOpt reader readString
            OptionalParam (existential.Value, getPrimitiveParserByType label existential.Value.Type)
        | ArgumentType.List ->
            let existential = readExistential reader
            let label = readOpt reader readString
            ListParam (existential.Value, getPrimitiveParserByType label existential.Value.Type)
        | ArgumentType.SubCommand ->
            let shape = readShapeArgumentTemplate reader
            let argInfo = readUnionArgInfo tryGetCurrent reader
            let label = readOptString reader
            SubCommand (shape.Value, argInfo, label)
        |_ -> failwith "Not supported")
    case, parameterInfo

and private readCase (getParent: unit -> UnionArgInfo) (reader: BinaryReader): UnionCaseArgInfo =
    let current = ref None
    let tryGetCurrent = fun () -> !current

    let name = reader.ReadString()
    let depth = reader.ReadInt32()
    let arity = reader.ReadInt32()
    let unionCaseType = reader.ReadString()
    let unionCaseTag = reader.ReadInt32()

    let argType, parameterInfo = readParameterInfo tryGetCurrent reader
    let caseConstructorName = reader.ReadString()
    //let caseCtorInfo = FSharpValue.PreComputeUnionConstructorInfo case.UnionCaseInfo.Value
    //writer.Write(caseCtorInfo.Name)
    let commandLineNames = readArray reader readString
    let appSettingsName = readOptString reader
    let description = readString reader
    let appSettingsSeparators = readArray reader readString
    let appSettingsSplitOptions = LanguagePrimitives.EnumOfValue<_, StringSplitOptions> (reader.ReadInt32())
    let customAssignmentSeparator = lazyConst (readOpt reader readString)
    let cliPosition = LanguagePrimitives.EnumOfValue<_, CliPosition> (reader.ReadInt32())
    let mainCommandName = readOptString reader
    let isRest = reader.ReadBoolean()
    let appSettingsCSV = reader.ReadBoolean()
    let isMandatory = reader.ReadBoolean()
    let isInherited = reader.ReadBoolean()
    let isUnique = reader.ReadBoolean()
    let isHidden = reader.ReadBoolean()
    let isGatherUnrecognized = reader.ReadBoolean()
    let gatherAllSources = reader.ReadBoolean()

    // --
    let unionType = lazy(Type.GetType(unionCaseType))
    let uci = lazy(FSharpType.GetUnionCases(unionType.Value).[unionCaseTag])
    let fields = lazy(uci.Value.GetFields())
    let types = lazy(fields.Value |> Array.map (fun f -> f.PropertyType))

    let caseCtor = lazy(
        let meth = unionType.Value.GetMethod(caseConstructorName, System.Reflection.BindingFlags.Static ||| System.Reflection.BindingFlags.Public )
        (fun args ->
            meth.Invoke(null, args))
    )
    let fieldReader = lazy((Helpers.fieldReader uci.Value).Value)
    let fieldCtor = lazy((Helpers.tupleConstructor types.Value).Value)
    let assignParser = Helpers.assignParser customAssignmentSeparator

    let uai = {
        Name = lazyConst name
        Depth = depth
        Arity = arity
        Tag = unionCaseTag
        UnionCaseInfo = uci
        ParameterInfo = parameterInfo
        ArgumentType = argType
        GetParent = getParent
        CaseCtor = caseCtor
        FieldCtor = fieldCtor
        FieldReader = fieldReader
        CommandLineNames = lazy(List.ofArray commandLineNames)
        AppSettingsName = lazyConst appSettingsName
        Description = lazyConst description
        AppSettingsSeparators = appSettingsSeparators
        AppSettingsSplitOptions = appSettingsSplitOptions
        CustomAssignmentSeparator = customAssignmentSeparator
        AssignmentParser = assignParser
        CliPosition = lazyConst cliPosition
        MainCommandName = lazyConst mainCommandName
        IsRest = lazyConst isRest
        AppSettingsCSV = lazyConst appSettingsCSV
        IsMandatory = lazyConst isMandatory
        IsInherited = lazyConst isInherited
        IsUnique = lazyConst isUnique
        IsHidden = lazyConst isHidden
        IsGatherUnrecognized = lazyConst isGatherUnrecognized
        GatherAllSources = lazyConst gatherAllSources
    }

    current := Some uai // assign result to children
    uai

and private readUnionArgInfo (tryGetParent : unit -> UnionCaseArgInfo option) (reader: BinaryReader): UnionArgInfo =
    let current = ref Unchecked.defaultof<_>
    let readCase = readCase (fun () -> !current)

    let typeName = reader.ReadString()
    let depth = reader.ReadInt32()
    // TryGetParent
    let caseInfo = lazyConst (readArray reader readCase)
    let helpParam = readHelpParam reader
    let containsSubcommands = reader.ReadBoolean()
    let isRequiredSubcommand = reader.ReadBoolean()
    // TagReader
    let inheritedParams = lazyConst (readArray reader readCase)
    let groupedSwitchRegex = lazyConst (readOptString reader)
    let appSettingsParamIndex = readArray reader (fun _ ->
        let key = reader.ReadString()
        let value = readCase reader
        key, value)

    let cliParamIndex = readArray reader (fun _ ->
        let key = reader.ReadString()
        let value = readCase reader
        key, value)
    let unrecognizedGatherParam = readOpt reader readCase
    let mainCommandParam = readOpt reader readCase

    // ----
    let t = lazy(Type.GetType(typeName))

    let result =
        {
            Type = t
            Depth = depth
            TryGetParent = tryGetParent
            Cases = caseInfo
            HelpParam = helpParam
            ContainsSubcommands = lazyConst containsSubcommands
            IsRequiredSubcommand = lazyConst isRequiredSubcommand
            TagReader = lazy(FSharpValue.PreComputeUnionTagReader(t.Value, allBindings))
            InheritedParams = inheritedParams
            GroupedSwitchRegex = groupedSwitchRegex
            GroupedSwitchExtractor = Helpers.groupedSwitchExtractor groupedSwitchRegex
            AppSettingsParamIndex =
                lazy(
                    let dict = Dictionary<_,_>(appSettingsParamIndex.Length)
                    for (key,value) in appSettingsParamIndex do
                        dict.Add(key, value)
                    dict :> IDictionary<_,_>)
            CliParamIndex = lazy(PrefixDictionary(cliParamIndex))
            UnrecognizedGatherParam = lazyConst unrecognizedGatherParam
            MainCommandParam = lazyConst mainCommandParam
        }

    current := result
    result

let private deserialize (data: byte[]): UnionArgInfo =
    use stream = new MemoryStream(data)
    use gzip = new GZipStream(stream, CompressionMode.Decompress)

    let finalStream = new MemoryStream(data.Length)
    gzip.CopyTo(finalStream)
    finalStream.Position <- 0L
    let reader = new BinaryReader(finalStream, Encoding.UTF8)

    readUnionArgInfo (fun () -> None) reader

let tryLoad (data: string): UnionArgInfo option =
    if (isNull data) || (data.Length = 0) then
        None
    else
        let bytes = Convert.FromBase64String(data)
        Some (deserialize bytes)