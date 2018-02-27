[<AutoOpen>]
module internal Argu.PreCompute

#nowarn "44"

open System
open System.Reflection
open System.Collections.Generic
open System.Text.RegularExpressions

open FSharp.Reflection

let defaultHelpParam = "help"
let defaultHelpDescription = "display this list of options."

let getDefaultHelpParam (t : Type) =
    let prefixString =
        match t.TryGetAttribute<CliPrefixAttribute>() with
        | None -> CliPrefix.DoubleDash
        | Some pf -> pf.Prefix

    prefixString + defaultHelpParam

/// construct a CLI param from UCI name
let generateOptionName (uci : UnionCaseInfo) =
    let prefixString =
        match uci.TryGetAttribute<CliPrefixAttribute>(true) with
        | None -> CliPrefix.DoubleDash
        | Some pf -> pf.Prefix

    prefixString + uci.Name.ToLowerInvariant().Replace('_','-')

/// Generate a CLI Param for enumeration cases
let generateEnumName (name : string) = name.ToLowerInvariant().Replace('_','-')

/// construct an App.Config param from UCI name
let generateAppSettingsName (uci : UnionCaseInfo) =
    uci.Name.ToLowerInvariant().Replace('_',' ')

/// construct a command identifier from UCI name
let generateCommandName (uci : UnionCaseInfo) =
    uci.Name.ToUpperInvariant().Replace('_', ' ')

let private defaultLabelRegex = new Regex(@"^Item[0-9]*$", RegexOptions.Compiled)
/// Generates an argument label name from given PropertyInfo
let tryExtractUnionParameterLabel (p : PropertyInfo) =
    if defaultLabelRegex.IsMatch p.Name then None
    else Some(p.Name.Replace('_',' '))

let (|NestedParseResults|Optional|List|Other|) (t : Type) =
    if t.IsGenericType then
        let gt = t.GetGenericTypeDefinition()
        if typeof<IParseResult>.IsAssignableFrom t then NestedParseResults(t.GetGenericArguments().[0])
        elif gt = typedefof<_ option> then Optional(t.GetGenericArguments().[0])
        elif gt = typedefof<_ list> then List(t.GetGenericArguments().[0])
        else Other
    else Other

/// Creates a primitive field parser from given parser/unparser lambdas
let mkPrimitiveParser (name : string) (parser : string -> 'T) (unparser : 'T -> string) (label : string option) =
    {
        Name = name
        Label = label
        Type = typeof<'T>
        Parser = fun x -> parser x :> obj
        UnParser = fun o -> unparser (o :?> 'T)
    }

let primitiveParsers = lazy(
    let mkParser name (pars : string -> 'a) unpars = typeof<'a>, mkPrimitiveParser name pars unpars in
    dict [|
        mkParser "bool" Boolean.Parse (fun b -> if b then "true" else "false")
        mkParser "byte" Byte.Parse string
        mkParser "sbyte" SByte.Parse string

        mkParser "int16" Int16.Parse string
        mkParser "int" Int32.Parse string
        mkParser "int64" Int64.Parse string
        mkParser "uint16" UInt16.Parse string
        mkParser "uint" UInt32.Parse string
        mkParser "uint64" UInt64.Parse string

        mkParser "char" Char.Parse string
        mkParser "string" id id

        mkParser "float" Single.Parse string
        mkParser "double" Double.Parse string
        mkParser "decimal" Decimal.Parse string
        mkParser "bigint" System.Numerics.BigInteger.Parse string
        mkParser "guid" Guid string
        mkParser "base64" Convert.FromBase64String Convert.ToBase64String
    |])

/// Creates a primitive parser from an enumeration
let tryGetEnumerationParser label (t : Type) =
    if not t.IsEnum then None else
    let names = Enum.GetNames(t) |> Seq.map generateEnumName
    let values = Enum.GetValues(t) |> Seq.cast<obj>
    let index = Seq.zip names values |> Seq.toArray
    let name = names |> String.concat "|"

    let parser (text : string) =
        let text = text.Trim()
        let _,value = index |> Array.find (fun (id,_) -> text = id)
        value

    let unparser (value : obj) =
        match Enum.GetName(t, value) with
        | null -> failwith "invalid enum value!"
        | name -> generateEnumName name

    Some {
        Name = name
        Label = label
        Type = t
        Parser = parser
        UnParser = unparser
    }

/// Creates a primitive parser from an F# DU enumeration
/// (i.e. one with no parameters in any of its union cases)
let tryGetDuEnumerationParser label (t : Type) =
    if not <| FSharpType.IsUnion(t, allBindings) then None else

    let ucis = FSharpType.GetUnionCases(t, allBindings)
    if ucis |> Array.exists (fun uci -> uci.GetFields().Length > 0) then None else

    let tagReader = lazy(FSharpValue.PreComputeUnionTagReader(t, allBindings))
    let extractUciInfo (uci : UnionCaseInfo) =
        let name =
            match uci.TryGetAttribute<CustomCommandLineAttribute>() with
            | None -> generateEnumName uci.Name
            | Some attr -> attr.Name

        let value = FSharpValue.MakeUnion(uci, [||], allBindings)
        name, value

    let index = ucis |> Array.map extractUciInfo
    let name = index |> Seq.map fst |> String.concat "|"

    let parser (text : string) =
        let text = text.Trim()
        let _,value = index |> Array.find (fun (id,_) -> text = id)
        value

    let unparser (value : obj) =
        let tag = tagReader.Value value
        let id,_ = index.[tag]
        id

    Some {
        Name = name
        Label = label
        Type = t
        Parser = parser
        UnParser = unparser
    }

let getPrimitiveParserByType label (t : Type) =
    let ok, f = primitiveParsers.Value.TryGetValue t
    if ok then f label
    else
        match tryGetEnumerationParser label t with
        | Some p -> p
        | None ->

        match tryGetDuEnumerationParser label t with
        | Some p -> p
        | None ->


        // refine error messaging depending on the input time
        match t with
        | NestedParseResults _ -> arguExn "Nested ParseResult<'T> parameters can only occur as standalone parameters in union constructors."
        | Optional _ -> arguExn "F# Option parameters can only occur as standalone parameters in union constructors."
        | List _ -> arguExn "F# List parameters can only occur as standalone parameters in union constructors."
        | _ -> arguExn "template contains unsupported field of type '%O'." t

let private validCliParamRegex = new Regex(@"\S+", RegexOptions.Compiled)
let validateCliParam (name : string) =
    if name = null || not <| validCliParamRegex.IsMatch name then
        arguExn "CLI parameter '%s' contains invalid characters." name

let validSeparatorChars = [|'=' ; ':' ; '.' ; '#' ; '+' ; '^' ; '&' ; '?' ; '%' ; '$' ; '~' ; '@'|]
let private validSeparatorRegex =
    let escapedChars = new String(validSeparatorChars) |> Regex.Escape
    new Regex(sprintf @"[%s]+" escapedChars, RegexOptions.Compiled)

let validateSeparator (uci : UnionCaseInfo) (sep : string) =
    if sep = null || not <| validSeparatorRegex.IsMatch sep then
        let allowedchars = validSeparatorChars |> Seq.map (fun c -> sprintf "'%c'" c) |> String.concat ", "
        arguExn "parameter '%O' specifies invalid separator '%s' in CustomAssignment attribute.%sAllowed characters: %s"
            uci sep Environment.NewLine allowedchars

/// extracts the subcommand argument hierarchy for given UnionArgInfo
let getHierarchy (uai : UnionArgInfo) =
    let rec aux acc (uai : UnionArgInfo) =
        match uai.TryGetParent () with
        | None -> acc
        | Some ucai -> aux (ucai :: acc) (ucai.GetParent())

    aux [] uai

/// generate argument parsing schema from given UnionCaseInfo
let rec private preComputeUnionCaseArgInfo (stack : Type list) (helpParam : HelpParam option)
                                            (getParent : unit -> UnionArgInfo)
                                            (uci : UnionCaseInfo) : UnionCaseArgInfo =

    let fields = uci.GetFields()
    let types = fields |> Array.map (fun f -> f.PropertyType)

    let caseCtor = FSharpValue.PreComputeUnionConstructor(uci, allBindings)

    // create a dummy instance for given union case
    let dummyFields = types |> Array.map Unchecked.UntypedDefaultOf
    let dummy = caseCtor dummyFields :?> IArgParserTemplate

    // use ref cell for late binding of parent argInfo
    let current = ref None
    let tryGetCurrent = fun () -> !current

    let isNoCommandLine = lazy(uci.ContainsAttribute<NoCommandLineAttribute> (true))
    let isAppSettingsCSV = lazy(uci.ContainsAttribute<ParseCSVAttribute> ())
    let isExactlyOnce = lazy(uci.ContainsAttribute<ExactlyOnceAttribute> (true))
    let isMandatory = lazy(isExactlyOnce.Value || uci.ContainsAttribute<MandatoryAttribute> (true))
    let isUnique = lazy(isExactlyOnce.Value || uci.ContainsAttribute<UniqueAttribute> (true))
    let isInherited = lazy(uci.ContainsAttribute<InheritAttribute> ())
    let isGatherAll = lazy(uci.ContainsAttribute<GatherAllSourcesAttribute> ())
    let isRest = lazy(uci.ContainsAttribute<RestAttribute> ())
    let isHidden = lazy(uci.ContainsAttribute<HiddenAttribute> ())

    let mainCommandName = lazy(
        match uci.TryGetAttribute<MainCommandAttribute> () with
        | None -> None
        | Some _ when isNoCommandLine.Value -> arguExn "parameter '%O' contains conflicting attributes 'MainCommand' and 'NoCommandLine'." uci
        | Some _ when types.Length = 0 -> arguExn "parameter '%O' contains MainCommand attribute but has unsupported arity 0." uci
        | Some attr ->
            match attr.ArgumentName with
            | null -> generateCommandName uci
            | name -> name
            |> Some)

    let isMainCommand = lazy(Option.isSome mainCommandName.Value)

    let cliPosition = lazy(
        match uci.TryGetAttribute<CliPositionAttribute> () with
        | Some attr ->
            match attr.Position with
            | CliPosition.Unspecified
            | CliPosition.First
            | CliPosition.Last as p -> p
            | _ -> arguExn "Invalid CliPosition setting '%O' for parameter '%O'" attr.Position uci
        | None -> CliPosition.Unspecified)

    let customAssignmentSeparator = lazy(
        match uci.TryGetAttribute<CustomAssignmentAttribute> (true) with
        | Some attr ->
            if isMainCommand.Value && types.Length = 1 then
                arguExn "parameter '%O' of arity 1 contains incompatible attributes 'CustomAssignment' and 'MainCommand'." uci
            if types.Length <> 1 && types.Length <> 2 then
                arguExn "parameter '%O' has CustomAssignment attribute but specifies %d parameters. Should be 1 or 2." uci types.Length
            elif isRest.Value then
                arguExn "parameter '%O' contains incompatible attributes 'CustomAssignment' and 'Rest'." uci

            validateSeparator uci attr.Separator
            Some attr.Separator

        | None -> None)

    let isGatherUnrecognized = lazy(
        if uci.ContainsAttribute<GatherUnrecognizedAttribute>() then
            match types with
            | _ when isMainCommand.Value -> arguExn "parameter '%O' contains incompatible combination of attributes 'MainCommand' and 'GatherUnrecognized'." uci
            | [|t|] when t = typeof<string> -> true
            | _ -> arguExn "parameter '%O' has GatherUnrecognized attribute but specifies invalid parameters. Must contain single parameter of type string." uci
        else
            false)

    let appSettingsSeparators, appSettingsSplitOptions =
        match uci.TryGetAttribute<AppSettingsSeparatorAttribute> (true) with
        | None -> [|","|], StringSplitOptions.None
        | Some attr when attr.Separators.Length = 0 ->
            arguExn "parameter '%O' specifies a null or empty AppSettings separator." uci

        | Some attr ->
            for sep in attr.Separators do
                if String.IsNullOrEmpty sep then
                    arguExn "parameter '%O' specifies a null or empty AppSettings separator." uci

            attr.Separators, attr.SplitOptions

    let parsers = lazy(
        match types with
        | [|NestedParseResults prt|] ->
            if Option.isSome customAssignmentSeparator.Value then
                arguExn "CustomAssignment in '%O' not supported in subcommands." uci
            if isRest.Value then
                arguExn "Rest attribute in '%O' not supported in subcommands." uci
            if isMandatory.Value then
                arguExn "Mandatory attribute in '%O' not supported in subcommands." uci
            if isMainCommand.Value then
                arguExn "MainCommand attribute in '%O' not supported in subcommands." uci
            if isInherited.Value then
                arguExn "Inherit attribute in '%O' not supported in subcommands." uci

            let argInfo = preComputeUnionArgInfoInner stack helpParam tryGetCurrent prt
            let shape = ShapeArgumentTemplate.FromType prt
            SubCommand(shape, argInfo, tryExtractUnionParameterLabel fields.[0])

        | [|Optional t|] ->
            if isRest.Value then
                arguExn "Rest attribute in '%O' not supported in optional parameters." uci

            if isMainCommand.Value then
                arguExn "MainCommand attribute in '%O' not supported in optional parameters." uci

            let label = tryExtractUnionParameterLabel fields.[0]

            OptionalParam(Existential.FromType t, getPrimitiveParserByType label t)

        | [|List t|] ->
            if Option.isSome customAssignmentSeparator.Value then
                arguExn "CustomAssignment in '%O' not supported for list parameters." uci

            if isRest.Value then
                arguExn "Rest attribute in '%O' not supported for list parameters." uci

            let label = tryExtractUnionParameterLabel fields.[0]

            ListParam(Existential.FromType t, getPrimitiveParserByType label t)

        | _ ->
            let getParser (p : PropertyInfo) =
                let label = tryExtractUnionParameterLabel p
                getPrimitiveParserByType label p.PropertyType

            Array.map getParser fields |> Primitives)

    let commandLineArgs = lazy(
        if isMainCommand.Value || isNoCommandLine.Value then []
        else
            let cliNames = [
                match uci.TryGetAttribute<CustomCommandLineAttribute> () with
                | None -> yield generateOptionName uci
                | Some attr -> yield attr.Name ; yield! attr.AltNames

                yield!
                    uci.GetAttributes<AltCommandLineAttribute>()
                    |> Seq.collect (fun attr -> attr.Names)
            ]

            for name in cliNames do validateCliParam name

            cliNames)

    let appSettingsName = lazy(
        if uci.ContainsAttribute<NoAppSettingsAttribute> (true) then None
        else
            match uci.TryGetAttribute<CustomAppSettingsAttribute> () with
            | None -> Some <| generateAppSettingsName uci
            | Some _ when parsers.Value.Type = ArgumentType.SubCommand -> arguExn "CustomAppSettings in %O not supported in subcommands." uci
            | Some attr when not <| String.IsNullOrWhiteSpace attr.Name -> Some attr.Name
            | Some attr -> arguExn "AppSettings parameter '%s' contains invalid characters." attr.Name)

    /// gets the default name of the argument
    let defaultName = lazy(
        match commandLineArgs.Value with
        | h :: _ -> h
        | [] when isMainCommand.Value ->
            match parsers.Value with
            | Primitives ps ->
                let name = ps |> Seq.map (fun p -> sprintf "<%s>" p.Description) |> String.concat " "
                if isRest.Value then name + "..." else name
            | ListParam(_,p) -> sprintf "<%s>..." p.Description
            | _ -> arguExn "internal error in argu parser representation %O." uci
        | _ when Option.isSome appSettingsName.Value -> appSettingsName.Value.Value
        | _ -> arguExn "parameter '%O' needs to have at least one parse source." uci)

    let fieldCtor = lazy(
        match types.Length with
        | 0 -> fun _ -> arguExn "internal error: attempting to call tuple constructor on nullary case."
        | 1 -> fun (o:obj[]) -> o.[0]
        | _ ->
            let tupleType = FSharpType.MakeTupleType types
            FSharpValue.PreComputeTupleConstructor tupleType)

    let fieldReader = lazy(FSharpValue.PreComputeUnionReader(uci, allBindings))

    let assignParser = lazy(
        match customAssignmentSeparator.Value with
        | None -> arguExn "internal error: attempting to call assign parser on invalid parameter."
        | Some sep ->
            let pattern = sprintf @"^(.+)%s(.+)$" (Regex.Escape sep)
            let regex = new Regex(pattern, RegexOptions.RightToLeft ||| RegexOptions.Compiled)
            fun token ->
                let m = regex.Match token
                if m.Success then Assignment(m.Groups.[1].Value, sep, m.Groups.[2].Value)
                else NoAssignment)

    if isAppSettingsCSV.Value && fields.Length <> 1 then
        arguExn "CSV attribute is only compatible with branches of unary fields."

    // extract the description string for given union case
    let description =
        try dummy.Usage
        with _ -> arguExn "Error generating usage string from IArgParserTemplate for case %O." uci

    let uai = {
        UnionCaseInfo = uci
        Arity = fields.Length
        Depth = List.length stack - 1
        CaseCtor = caseCtor
        FieldReader = fieldReader
        FieldCtor = fieldCtor
        Name = defaultName
        GetParent = getParent
        CommandLineNames = commandLineArgs
        AppSettingsName = appSettingsName
        AppSettingsSeparators = appSettingsSeparators
        AppSettingsSplitOptions = appSettingsSplitOptions
        Description = description
        ParameterInfo = parsers
        AppSettingsCSV = isAppSettingsCSV
        MainCommandName = mainCommandName
        IsMandatory = isMandatory
        IsUnique = isUnique
        IsInherited = isInherited
        GatherAllSources = isGatherAll
        IsRest = isRest
        CliPosition = cliPosition
        CustomAssignmentSeparator = customAssignmentSeparator
        AssignmentParser = assignParser
        IsGatherUnrecognized = isGatherUnrecognized
        IsHidden = isHidden
    }

    current := Some uai // assign result to children
    uai

and private preComputeUnionArgInfoInner (stack : Type list) (helpParam : HelpParam option) (tryGetParent : unit -> UnionCaseArgInfo option) (t : Type) : UnionArgInfo =
    if not <| FSharpType.IsUnion(t, allBindings) then
        arguExn "template type '%O' is not an F# union." t
    elif stack |> List.exists ((=) t) then
        arguExn "template type '%O' implements unsupported recursive pattern." t
    elif t.IsGenericType then
        arguExn "template type '%O' is generic; this is not supported." t

    let helpParam =
        match helpParam with
        | Some hp -> hp // always inherit help schema from parent union
        | None ->
            let helpSwitches =
                match t.TryGetAttribute<HelpFlagsAttribute>() with
                | None -> [getDefaultHelpParam t]
                | Some hf ->
                    for f in hf.Names do validateCliParam f
                    Array.toList hf.Names

            let description =
                match t.TryGetAttribute<HelpDescriptionAttribute> () with
                | None -> defaultHelpDescription
                | Some attr -> attr.Description

            { Flags = helpSwitches ; Description = description }

    // use ref cell for late binding of parent argInfo
    let current = ref Unchecked.defaultof<_>
    let getCurrent = fun () -> !current

    let caseInfo = lazy(
        FSharpType.GetUnionCases(t, allBindings)
        |> Seq.map (preComputeUnionCaseArgInfo (t :: stack) (Some helpParam) getCurrent)
        |> Seq.sortBy (fun a -> a.Tag)
        |> Seq.toArray)

    let containsSubcommands = lazy(caseInfo.Value |> Array.exists (fun c -> c.Type = ArgumentType.SubCommand))
    let isRequiredSubcommand = lazy(t.ContainsAttribute<RequireSubcommandAttribute>() && containsSubcommands.Value)

    // need to delay this computation since it depends
    // on completed process of any potential parents
    let inheritedParams = lazy(
        match tryGetParent() with
        | None -> [||]
        | Some parent ->
            let pInfo = parent.GetParent()
            pInfo.Cases.Value
            |> Seq.filter (fun cI -> cI.IsInherited.Value)
            |> Seq.append pInfo.InheritedParams.Value
            |> Seq.toArray)

    // recognizes and extracts grouped switches
    // e.g. -efx --> -e -f -x
    let groupedSwitchExtractor = lazy(
        let chars =
            caseInfo.Value
            |> Seq.append inheritedParams.Value
            |> Seq.collect (fun c -> c.CommandLineNames.Value)
            |> Seq.append helpParam.Flags
            |> Seq.filter (fun name -> name.Length = 2 && name.[0] = '-' && Char.IsLetterOrDigit name.[1])
            |> Seq.map (fun name -> name.[1])
            |> Seq.distinct
            |> Seq.toArray
            |> String

        if chars.Length = 0 then (fun _ -> [||]) else

        let regex = new Regex(sprintf "^-[%s]+$" chars, RegexOptions.Compiled)

        fun (arg : string) ->
            if not <| regex.IsMatch arg then [||]
            else Array.init (arg.Length - 1) (fun i -> sprintf "-%c" arg.[i + 1]))

    let tagReader = lazy(FSharpValue.PreComputeUnionTagReader(t, allBindings))

    let cliIndex = lazy(
        caseInfo.Value
        |> Seq.append inheritedParams.Value
        |> Seq.collect (fun cs -> cs.CommandLineNames.Value |> Seq.map (fun name -> name, cs))
        |> PrefixDictionary)

    let appSettingsIndex = lazy(
        caseInfo.Value
        |> Seq.choose (fun cs -> match cs.AppSettingsName.Value with Some name -> Some(name, cs) | None -> None)
        |> dict)

    let unrecognizedParam = lazy(
        match caseInfo.Value |> Array.filter (fun cI -> cI.IsGatherUnrecognized.Value) with
        | [||] -> None
        | [|ur|] -> Some ur
        | _ -> arguExn "template type '%O' has specified the GatherUnrecognized attribute in more than one union cases." t)

    let mainCommandParam = lazy(
        match caseInfo.Value |> Array.filter (fun cI -> cI.IsMainCommand) with
        | [||] -> None
        | [|mcp|] -> Some mcp
        | _ -> arguExn "template type '%O' has specified the MainCommand attribute in more than one union cases." t)

    let result = {
        Type = t
        Depth = List.length stack
        TryGetParent = tryGetParent
        Cases = caseInfo
        TagReader = tagReader
        HelpParam = helpParam
        ContainsSubcommands = containsSubcommands
        IsRequiredSubcommand = isRequiredSubcommand
        GroupedSwitchExtractor = groupedSwitchExtractor
        AppSettingsParamIndex = appSettingsIndex
        InheritedParams = inheritedParams
        CliParamIndex = cliIndex
        UnrecognizedGatherParam = unrecognizedParam
        MainCommandParam = mainCommandParam
    }

    current := result // assign result to children
    result

and preComputeUnionArgInfo<'Template when 'Template :> IArgParserTemplate> () =
    preComputeUnionArgInfoInner [] None (fun () -> None) typeof<'Template>

// used for performing additional checks on the completed dependency graph
let checkUnionArgInfo (result: UnionArgInfo) =
    let rec postProcess (argInfo : UnionArgInfo) =
        // check for conflicting CLI identifiers
        argInfo.Cases.Value
        |> Seq.append argInfo.InheritedParams.Value // this will only have been populated post-construction
        |> Seq.collect (fun arg -> arg.CommandLineNames.Value |> Seq.map (fun cliName -> cliName, arg))
        |> Seq.map (fun ((name, arg) as t) ->
            if argInfo.HelpParam.IsHelpFlag name then
                arguExn "parameter '%O' using CLI identifier '%s' which is reserved for help parameters." arg.UnionCaseInfo name
            t)
        |> Seq.groupBy fst
        |> Seq.tryFind (fun (_,args) -> Seq.length args > 1)
        |> Option.iter (fun (name,args) ->
            let conflicts = args |> Seq.map snd |> Seq.toArray
            arguExn "parameters '%O' and '%O' using conflicting CLI identifier '%s'."
                conflicts.[0].UnionCaseInfo conflicts.[1].UnionCaseInfo name)

        // check for conflicting AppSettings identifiers
        if argInfo.Depth = 0 then
            argInfo.Cases.Value
            |> Seq.choose(fun arg -> arg.AppSettingsName.Value |> Option.map (fun name -> name, arg))
            |> Seq.groupBy fst
            |> Seq.tryFind(fun (_,args) -> Seq.length args > 1)
            |> Option.iter (fun (name,args) ->
                let conflicts = args |> Seq.map snd |> Seq.toArray
                arguExn "parameters '%O' and '%O' using conflicting AppSettings identifier '%s'."
                    conflicts.[0].UnionCaseInfo conflicts.[1].UnionCaseInfo name)

        // Evaluate every lazy property to ensure that their checks run
        for case in argInfo.Cases.Value do
            case.Name.Value |> ignore
            case.CommandLineNames.Value |> ignore
            case.AppSettingsName.Value |> ignore
            case.ParameterInfo.Value |> ignore
            case.AppSettingsCSV.Value |> ignore
            case.MainCommandName.Value |> ignore
            case.IsMandatory.Value |> ignore
            case.IsUnique.Value |> ignore
            case.IsInherited.Value |> ignore
            case.GatherAllSources.Value |> ignore
            case.IsRest.Value |> ignore
            case.CliPosition.Value |> ignore
            case.CustomAssignmentSeparator.Value |> ignore
            case.IsGatherUnrecognized.Value |> ignore
            case.IsHidden.Value |> ignore

        // iterate through the child nodes
        for case in argInfo.Cases.Value do
            match case.ParameterInfo.Value with
            | SubCommand(_, aI, _) -> postProcess aI
            | _ -> ()

    postProcess result
    result