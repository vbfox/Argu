module internal Argu.Serialize

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Text

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
    writer.Write(fieldParserInfo.Name)
    writeOptString writer fieldParserInfo.Label
    writer.Write(fieldParserInfo.Type.AssemblyQualifiedName)
    // Parser
    // UnParser
    ()

let rec private writeParameterInfo (writer: BinaryWriter) (parameterInfo: ParameterInfo) =
    match parameterInfo with
    | Primitives fieldParserInfos ->
        writer.Write(1uy)
        writeSeq writer fieldParserInfos writeFieldParserInfo
    | OptionalParam (existential, fieldParserInfo) ->
        writer.Write(2uy)
        writer.Write(existential.Type.AssemblyQualifiedName)
        writeFieldParserInfo writer fieldParserInfo
    | ListParam (existential, fieldParserInfo) ->
        writer.Write(3uy)
        writer.Write(existential.Type.AssemblyQualifiedName)
        writeFieldParserInfo writer fieldParserInfo
    | SubCommand (shape, argInfo, label) ->
        writer.Write(4uy)
        writer.Write(shape.Type.AssemblyQualifiedName)
        writeUnionArgInfo writer argInfo
        writeOptString writer label

and private writeCase (writer: BinaryWriter) (case: UnionCaseArgInfo) =
    writer.Write(case.Name.Value)
    writer.Write(case.Depth)
    writer.Write(case.Arity)
    writer.Write(case.UnionCaseInfo.DeclaringType.AssemblyQualifiedName)
    writer.Write(case.UnionCaseInfo.Tag)
    writeParameterInfo writer case.ParameterInfo.Value
    // GetParent
    // CaseCtor
    // FieldCtor
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
    writer.Write(info.Type.AssemblyQualifiedName)
    writer.Write(info.Depth)
    // TryGetParent
    writeSeq writer info.Cases.Value writeCase
    writeHelpParam writer info.HelpParam
    writer.Write(info.ContainsSubcommands.Value)
    writer.Write(info.IsRequiredSubcommand.Value)
    // TagReader
    writeSeq writer info.InheritedParams.Value writeCase
    // GroupedSwitchExtractor
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

let private readArray<'a> (reader: BinaryReader) (elementReader: BinaryReader -> 'a): 'a[] =
    let length = reader.ReadInt32()
    let result = Array.zeroCreate<'a> length
    for i = 0 to length-1 do
        result.[i] <- elementReader reader
    result

let private readSeq<'a> (reader: BinaryReader) (elementReader: BinaryReader -> 'a): 'a seq =
    readArray reader elementReader :> seq<_>

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

let private readShapeArgumentTemplate (reader: BinaryReader): ShapeArgumentTemplate =
    let typeName = reader.ReadString()
    let type' = System.Type.GetType(typeName)
    ShapeArgumentTemplate.FromType(type')

let private readExistential (reader: BinaryReader): Existential =
    let typeName = reader.ReadString()
    let type' = System.Type.GetType(typeName)
    Existential.FromType(type')

let private readFieldParserInfo (reader: BinaryReader): FieldParserInfo =
    let name = reader.ReadString()
    let label = readOptString reader
    let typeName = reader.ReadString()
    let type' = System.Type.GetType(typeName)
    // Parser
    // UnParser
    failwith "boom"

let private readHelpParam (reader: BinaryReader): HelpParam =
    let flags = readArray reader readString |> List.ofArray
    let description = readString reader
    {
        Flags = flags
        Description = description
    }

let rec private readParameterInfo (reader: BinaryReader): ParameterInfo =
    // TODO: Reading data and materializing types should be 2 different steps
    // Most times data need ot be read but not materialized
    // Guess it depend on how much we lose doing GetType & friend
    let case = reader.ReadByte()
    match case with
    | 1uy ->
        let infos = readArray reader readFieldParserInfo
        Primitives infos
    | 2uy ->
        let existential = readExistential reader
        let info = readFieldParserInfo reader
        OptionalParam (existential, info)
    | 3uy ->
        let existential = readExistential reader
        let info = readFieldParserInfo reader
        ListParam (existential, info)
    | 4uy ->
        let shape = readShapeArgumentTemplate reader
        let argInfo = readUnionArgInfo reader
        let label = readOptString reader
        SubCommand (shape, argInfo, label)
    |_ -> failwith "Not supported"

and private readCase (reader: BinaryReader): UnionCaseArgInfo =
    let name = reader.ReadString()
    let depth = reader.ReadInt32()
    let arity = reader.ReadInt32()
    let unionCaseType = reader.ReadString()
    let unionCaseTag = reader.ReadInt32()
    let parameterInfo = readParameterInfo reader
    // GetParent
    // CaseCtor
    // FieldCtor
    // FieldReader
    let commandLineNames = readSeq reader readString
    let appSettingsName = readOptString reader
    let description = readString reader
    let appSettingsSeparators = readSeq reader readString
    let appSettingsSplitOptions = LanguagePrimitives.EnumOfValue<_, StringSplitOptions> (reader.ReadInt32())
    let customAssignmentSeparator = readOpt reader readString
    // AssignmentParser
    let cliPosition = LanguagePrimitives.EnumOfValue<_, CliPosition> (reader.ReadInt32())
    let MainCommandName = readOptString reader
    let isRest = reader.ReadBoolean()
    let AppSettingsCSV = reader.ReadBoolean()
    let IsMandatory = reader.ReadBoolean()
    let IsInherited = reader.ReadBoolean()
    let IsUnique = reader.ReadBoolean()
    let IsHidden = reader.ReadBoolean()
    let IsGatherUnrecognized = reader.ReadBoolean()
    let GatherAllSources = reader.ReadBoolean()

    failwith "boom"

and private readUnionArgInfo (reader: BinaryReader): UnionArgInfo =
    let typeName = reader.ReadString()
    let depth = reader.ReadInt32()
    // TryGetParent
    let cases = readSeq reader readCase
    let helpParam = readHelpParam reader
    let containsSubcommands = reader.ReadBoolean()
    let isRequiredSubcommand = reader.ReadBoolean()
    // TagReader
    let inheritedParams = readSeq reader readCase
    // GroupedSwitchExtractor
    let appSettingsParamIndex = readSeq reader (fun _ ->
        let key = reader.ReadString()
        let value = readCase reader
        KeyValuePair<_, _>(key, value))

    let cliParamIndex = readSeq reader (fun _ ->
        let key = reader.ReadString()
        let value = readCase reader
        key, value)
    let unrecognizedGatherParam = readOpt reader readCase
    let mainCommandParam = readOpt reader readCase
    {
        Type = failwith "boom"
        Depth = failwith "boom"
        TryGetParent = failwith "boom"
        Cases = failwith "boom"
        HelpParam = failwith "boom"
        ContainsSubcommands = failwith "boom"
        IsRequiredSubcommand = failwith "boom"
        TagReader = failwith "boom"
        InheritedParams = failwith "boom"
        GroupedSwitchExtractor = failwith "boom"
        AppSettingsParamIndex = failwith "boom"
        CliParamIndex = failwith "boom"
        UnrecognizedGatherParam = failwith "boom"
        MainCommandParam = failwith "boom"
    }

let private deserialize (data: byte[]): UnionArgInfo =
    use stream = new MemoryStream(data)
    use gzip = new GZipStream(stream, CompressionMode.Decompress)
    use reader = new BinaryReader(gzip, Encoding.UTF8)

    readUnionArgInfo reader

let load (data: string): UnionArgInfo =
    let bytes = Convert.FromBase64String(data)
    deserialize bytes