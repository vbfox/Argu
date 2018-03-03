﻿[<AutoOpen>]
module internal Argu.UnionArgInfo

open System
open System.IO
open System.Collections.Generic
open System.Reflection

open FSharp.Reflection
open FSharp.Quotations
open FSharp.Quotations.Patterns
open System.IO.Compression

type IParseResult =
    abstract GetAllResults : unit -> seq<obj>

[<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
type Assignment =
    | NoAssignment
    | Assignment of parameter:string * separator:string * value:string

/// Union Case Field info
[<NoEquality; NoComparison>]
type FieldParserInfo =
    {
        /// Type name
        Name : string
        /// field label
        Label : string option
        /// field type
        Type : Type
        /// string to field value parser
        Parser : string -> obj
        /// field value to string unparser
        UnParser : obj -> string
    }
with
    member inline p.Description =
        match p.Label with
        | None -> p.Name
        | Some l -> l

/// Help Param description
type HelpParam =
    {
        Flags : string list
        Description : string
    }
with
    member inline hp.IsHelpFlag(flag : string) =
        let rec aux = function
            | [] -> false
            | h :: tl' -> if h = flag then true else aux tl'

        aux hp.Flags

/// Represents a parsing schema for a single parameter
[<NoEquality; NoComparison>]
type UnionCaseArgInfo =
    {
        /// Human readable name identifier
        Name : Lazy<string>
        /// Contextual depth of current argument w.r.t subcommands
        Depth : int
        /// Numbers of parameters in the given union case
        Arity : int
        /// UCI identifier
        UnionCaseInfo : UnionCaseInfo
        /// Field parser definitions or nested union argument
        ParameterInfo : Lazy<ParameterInfo>

        /// Gets the parent record for union case
        GetParent : unit -> UnionArgInfo

        /// Builds a union case out of its field parameters
        CaseCtor : Lazy<obj [] -> obj>
        /// Composes case fields into a parametric tuple, if not nullary
        FieldCtor : Lazy<obj [] -> obj>
        /// Decomposes a case instance into an array of fields
        FieldReader : Lazy<obj -> obj[]>

        /// head element denotes primary command line arg
        CommandLineNames : Lazy<string list>
        /// name used in AppSettings
        AppSettingsName : Lazy<string option>

        /// Description of the parameter
        Description : Lazy<string>

        /// Configuration parsing parameter separator
        AppSettingsSeparators : string []
        /// Configuration parsing split options
        AppSettingsSplitOptions : StringSplitOptions

        /// Separator token used for EqualsAssignment syntax; e.g. '=' forces '--param=arg' syntax
        CustomAssignmentSeparator : Lazy<string option>
        /// Reads assignment for that specific value
        AssignmentParser : Lazy<string -> Assignment>

        /// Mandated Cli position for the argument
        CliPosition : Lazy<CliPosition>
        /// Specifies that this argument is the main CLI command
        MainCommandName : Lazy<string option>
        /// If specified, should consume remaining tokens from the CLI
        IsRest : Lazy<bool>
        /// If specified, multiple parameters can be added in Configuration in CSV form.
        AppSettingsCSV : Lazy<bool>
        /// Fails if no argument of this type is specified
        IsMandatory : Lazy<bool>
        /// Indicates that argument should be inherited in the scope of any sibling subcommands.
        IsInherited : Lazy<bool>
        /// Specifies that argument should be specified at most once in CLI
        IsUnique : Lazy<bool>
        /// Hide from Usage
        IsHidden : Lazy<bool>
        /// Declares that the parameter should gather any unrecognized CLI params
        IsGatherUnrecognized : Lazy<bool>
        /// Combine AppSettings with CLI inputs
        GatherAllSources : Lazy<bool>
    }
with
    member inline __.Tag = __.UnionCaseInfo.Tag
    member inline __.IsMainCommand = Option.isSome __.MainCommandName.Value
    member inline __.IsCommandLineArg = match __.CommandLineNames.Value with [] -> __.IsMainCommand | _ -> true
    member inline __.Type = __.ParameterInfo.Value.Type
    member inline __.IsCustomAssignment = Option.isSome __.CustomAssignmentSeparator.Value


and ParameterInfo =
    | Primitives of FieldParserInfo []
    | OptionalParam of Existential * FieldParserInfo
    | ListParam of Existential * FieldParserInfo
    | SubCommand of ShapeArgumentTemplate * argInfo:UnionArgInfo * label:string option
with
    member pI.Type =
        match pI with
        | Primitives _ -> ArgumentType.Primitive
        | OptionalParam _ -> ArgumentType.Optional
        | ListParam _ -> ArgumentType.List
        | SubCommand _ -> ArgumentType.SubCommand

and [<NoEquality; NoComparison>]
  UnionArgInfo =
    {
        /// Union Case Argument Info
        Type : Type
        /// Contextual depth of current argument w.r.t subcommands
        Depth : int
        /// If subcommand, attempt to retrieve the parent record
        TryGetParent : unit -> UnionCaseArgInfo option
        /// Union cases
        Cases : Lazy<UnionCaseArgInfo []>
        /// Help flags specified by the library
        HelpParam : HelpParam
        /// Denotes that the current argument contains subcommands
        ContainsSubcommands : Lazy<bool>
        /// Specifies that CLI parse results require a subcommand
        IsRequiredSubcommand : Lazy<bool>
        /// Precomputed union tag reader
        TagReader : Lazy<obj -> int>
        /// Arguments inherited by parent commands
        InheritedParams : Lazy<UnionCaseArgInfo []>
        /// Single character switches
        GroupedSwitchExtractor : Lazy<string -> string []>
        /// Union cases indexed by appsettings parameter names
        AppSettingsParamIndex : Lazy<IDictionary<string, UnionCaseArgInfo>>
        /// Union cases indexed by cli parameter names
        CliParamIndex : Lazy<PrefixDictionary<UnionCaseArgInfo>>
        /// Union case parameter used to gather unrecognized CLI params
        UnrecognizedGatherParam : Lazy<UnionCaseArgInfo option>
        /// Main command parameter used by the CLI syntax
        MainCommandParam : Lazy<UnionCaseArgInfo option>
    }
with
    member inline uai.UsesHelpParam = List.isEmpty uai.HelpParam.Flags |> not
    member inline uai.ContainsMainCommand = Option.isSome uai.MainCommandParam.Value

module BinaryUnionArgInfoSerializer =
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

[<NoEquality; NoComparison>]
type UnionCaseParseResult =
    {
        /// Parsed field parameters
        Fields : obj[]
        /// Index denoting order of parse result
        Index : int
        /// ArgInfo used to parse parameter
        CaseInfo : UnionCaseArgInfo
        /// metadata provided by the parser
        ParseContext : string
        /// parse source
        Source : ParseSource
    }
with
    member inline __.Tag = __.CaseInfo.Tag
    member inline __.Value = __.CaseInfo.CaseCtor.Value __.Fields
    member inline __.FieldContents = __.CaseInfo.FieldCtor.Value __.Fields

[<NoEquality; NoComparison>]
type UnionParseResults =
    {
        /// Results by union case
        Cases : UnionCaseParseResult[][]
        /// CLI tokens not recognized by the parser
        UnrecognizedCliParams : string list
        /// CLI parse objects not belonging to the current parser context
        UnrecognizedCliParseResults : obj list
        /// Usage string requested by the caller
        IsUsageRequested : bool
    }

type UnionCaseArgInfo with
    member inline ucai.IsFirst = ucai.CliPosition.Value = CliPosition.First
    member inline ucai.IsLast = ucai.CliPosition.Value = CliPosition.Last

    member ucai.ToArgumentCaseInfo() : ArgumentCaseInfo =
        {
            Name = ucai.Name
            ArgumentType = ucai.Type
            UnionCaseInfo = ucai.UnionCaseInfo
            CommandLineNames = ucai.CommandLineNames
            AppSettingsName = ucai.AppSettingsName
            Description = ucai.Description
            AppSettingsSeparators = Array.toList ucai.AppSettingsSeparators
            AppSettingsSplitOptions = ucai.AppSettingsSplitOptions
            IsMainCommand = ucai.IsMainCommand
            IsRest = ucai.IsRest
            CliPosition = ucai.CliPosition
            CustomAssignmentSeparator = ucai.CustomAssignmentSeparator
            AppSettingsCSV = ucai.AppSettingsCSV
            IsMandatory = ucai.IsMandatory
            IsUnique = ucai.IsUnique
            IsHidden = ucai.IsHidden
            IsGatherUnrecognized = ucai.IsGatherUnrecognized
            GatherAllSources = ucai.GatherAllSources
        }