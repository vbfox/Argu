namespace Argu.PerformanceTests

module PaketCommands =
    open System

    open Argu

    type AddArgs =
        | [<ExactlyOnce;MainCommand>] NuGet of package_ID:string
        | [<Hidden;ExactlyOnce;CustomCommandLine("nuget")>] NuGet_Legacy of package_ID:string

        | [<Unique;AltCommandLine("-V")>] Version of version_constraint:string
        | [<Hidden;Unique;CustomCommandLine("version")>] Version_Legacy of version_constraint:string

        | [<Unique;AltCommandLine("-p")>] Project of path:string
        | [<Hidden;Unique;CustomCommandLine("project")>] Project_Legacy of path:string

        | [<Unique;AltCommandLine("-g")>] Group of name:string
        | [<Hidden;Unique;CustomCommandLine("group")>] Group_Legacy of name:string

        | [<Unique>] Create_New_Binding_Files
        | [<Hidden;Unique;CustomCommandLine("--createnewbindingfiles")>] Create_New_Binding_Files_Legacy

        | [<Unique;AltCommandLine("-f")>] Force
        | [<Unique;AltCommandLine("-i")>] Interactive
        | [<Unique>] Redirects
        | [<Unique>] Clean_Redirects
        | [<Unique>] No_Install
        | [<Unique>] No_Resolve
        | [<Unique>] Keep_Major
        | [<Unique>] Keep_Minor
        | [<Unique>] Keep_Patch
        | [<Unique>] Touch_Affected_Refs
        | [<Unique;AltCommandLine("-t")>] Type of packageType:AddArgsDependencyType
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | NuGet(_) -> "NuGet package ID"
                | NuGet_Legacy(_) -> "[obsolete]"

                | Group(_) -> "add the dependency to a group (default: Main group)"
                | Group_Legacy(_) -> "[obsolete]"

                | Version(_) -> "dependency version constraint"
                | Version_Legacy(_) -> "[obsolete]"

                | Project(_) -> "add the dependency to a single project only"
                | Project_Legacy(_) -> "[obsolete]"

                | Create_New_Binding_Files -> "create binding redirect files if needed"
                | Create_New_Binding_Files_Legacy -> "[obsolete]"

                | Force -> "force download and reinstallation of all dependencies"
                | Interactive -> "ask for every project whether to add the dependency"
                | Redirects -> "create binding redirects"
                | Clean_Redirects -> "remove binding redirects that were not created by Paket"
                | No_Resolve -> "do not resolve"
                | No_Install -> "do not modify projects"
                | Keep_Major -> "only allow updates that preserve the major version"
                | Keep_Minor -> "only allow updates that preserve the minor version"
                | Keep_Patch -> "only allow updates that preserve the patch version"
                | Touch_Affected_Refs -> "touch project files referencing affected dependencies to help incremental build tools detecting the change"
                | Type _ -> "the type of dependency: nuget|clitool (default: nuget)"
    and [<RequireQualifiedAccess>] AddArgsDependencyType =
        | Nuget
        | Clitool

    type ConfigArgs =
        | [<Unique;CustomCommandLine("add-credentials")>] AddCredentials of key_or_URL:string
        | [<Unique;CustomCommandLine("add-token")>] AddToken of key_or_URL:string * token:string
        | [<Unique>] Username of username:string
        | [<Unique>] Password of password:string
        | [<Unique>] AuthType of authType:string
        | [<Unique;AltCommandLine>] Verify
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | AddCredentials(_) -> "add credentials for URL or credential key"
                | AddToken(_) -> "add token for URL or credential key"
                | Username(_) -> "provide username"
                | Password(_) -> "provide password"
                | AuthType (_) -> "specify authentication type: basic|ntlm (default: basic)"
                | Verify (_) -> "specify in case you want to verify the credentials"

    type ConvertFromNugetArgs =
        | [<Unique;AltCommandLine("-f")>] Force
        | [<Unique>] No_Install
        | [<Unique>] No_Auto_Restore

        | [<Unique>] Migrate_Credentials of mode:string
        | [<Hidden;Unique;CustomCommandLine("--creds-migrations")>] Migrate_Credentials_Legacy of mode:string
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Force -> "force the conversion even if paket.dependencies or paket.references files are present"
                | No_Install -> "do not modify projects"
                | No_Auto_Restore -> "do not enable automatic package restore"

                | Migrate_Credentials(_) -> "specify mode for NuGet source credential migration: encrypt|plaintext|selective (default: encrypt)"
                | Migrate_Credentials_Legacy(_) -> "[obsolete]"

    type FindRefsArgs =
        | [<ExactlyOnce;MainCommand>] NuGets of package_ID:string list
        | [<Hidden;ExactlyOnce;CustomCommandLine("nuget")>] NuGets_Legacy of package_ID:string list

        | [<Unique;AltCommandLine("-g")>] Group of name:string
        | [<Hidden;Unique;CustomCommandLine("group")>] Group_Legacy of name:string
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | NuGets(_) -> "list of NuGet package IDs"
                | NuGets_Legacy(_) -> "[obsolete]"

                | Group(_) -> "specify dependency group (default: Main group)"
                | Group_Legacy(_) -> "[obsolete]"

    type InitArgs =
        | [<Hidden;NoCommandLine>] NoArgs
    with
        interface IArgParserTemplate with
            member __.Usage = ""

    type AutoRestoreFlags = On | Off

    type AutoRestoreArgs =
        | [<MainCommand;ExactlyOnce>] Flags of AutoRestoreFlags
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Flags(_) -> "enable or disable automatic package restore"

    type LanguageFlags = Csx | Fsx

    type InstallArgs =
        | [<Unique;AltCommandLine("-f")>] Force
        | [<Unique>] Redirects

        | [<Unique>] Create_New_Binding_Files
        | [<Hidden;Unique;CustomCommandLine("--createnewbindingfiles")>] Create_New_Binding_Files_Legacy

        | [<Unique>] Clean_Redirects
        | [<Unique>] Keep_Major
        | [<Unique>] Keep_Minor
        | [<Unique>] Keep_Patch
        | [<Unique;CustomCommandLine("--only-referenced")>] Install_Only_Referenced
        | [<Unique>] Touch_Affected_Refs
        | [<Hidden;Unique;CustomCommandLine("project-root")>] Project_Root of path:string

        | [<Unique>] Generate_Load_Scripts
        | Load_Script_Framework of framework:string
        | [<Hidden;CustomCommandLine("load-script-framework")>] Load_Script_Framework_Legacy of framework:string

        | Load_Script_Type of LanguageFlags
        | [<Hidden;CustomCommandLine("load-script-type")>] Load_Script_Type_Legacy of LanguageFlags
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Force -> "force download and reinstallation of all dependencies"
                | Redirects -> "create binding redirects"

                | Create_New_Binding_Files -> "create binding redirect files if needed"
                | Create_New_Binding_Files_Legacy -> "[obsolete]"

                | Clean_Redirects -> "remove binding redirects that were not created by Paket"
                | Install_Only_Referenced -> "only install dependencies listed in paket.references files, instead of all packages in paket.dependencies"
                | Keep_Major -> "only allow updates that preserve the major version"
                | Keep_Minor -> "only allow updates that preserve the minor version"
                | Keep_Patch -> "only allow updates that preserve the patch version"
                | Touch_Affected_Refs -> "touch project files referencing affected dependencies to help incremental build tools detecting the change"
                | Project_Root(_) -> "alternative project root (only used for tooling)"

                | Generate_Load_Scripts -> "generate F# and C# include scripts that reference installed packages in a interactive environment like F# Interactive or ScriptCS"
                | Load_Script_Framework(_) -> "framework identifier to generate scripts for, such as net45 or netstandard1.6; may be repeated"
                | Load_Script_Framework_Legacy(_) -> "[obsolete]"

                | Load_Script_Type(_) -> "language to generate scripts for; may be repeated; may be repeated"
                | Load_Script_Type_Legacy(_) -> "[obsolete]"

    type OutdatedArgs =
        | [<Unique;AltCommandLine("-f")>] Force
        | [<Unique>] Ignore_Constraints

        | [<Unique;AltCommandLine("-g")>] Group of name:string
        | [<Hidden;Unique;CustomCommandLine("group")>] Group_Legacy of name:string

        | [<Unique;AltCommandLine("--pre")>] Include_Prereleases
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Force -> "force download and reinstallation of all dependencies"
                | Ignore_Constraints -> "ignore version constraints in the paket.dependencies file"

                | Group(_) -> "specify dependency group (default: all groups)"
                | Group_Legacy(_) -> "[obsolete]"

                | Include_Prereleases -> "consider prerelease versions as updates"

    type RemoveArgs =
        | [<ExactlyOnce;MainCommand>] NuGet of package_ID:string
        | [<Hidden;ExactlyOnce;CustomCommandLine("nuget")>] NuGet_Legacy of package_ID:string

        | [<Unique;AltCommandLine("-p")>] Project of path:string
        | [<Hidden;Unique;CustomCommandLine("project")>] Project_Legacy of path:string

        | [<Unique;AltCommandLine("-g")>] Group of name:string
        | [<Hidden;Unique;CustomCommandLine("group")>] Group_Legacy of name:string

        | [<Unique;AltCommandLine("-f")>] Force
        | [<Unique;AltCommandLine("-i")>] Interactive
        | [<Unique>] No_Install
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | NuGet(_) -> "NuGet package ID"
                | NuGet_Legacy(_) -> "[obsolete]"

                | Group(_) -> "remove the dependency from a group (default: Main group)"
                | Group_Legacy(_) -> "[obsolete]"

                | Project(_) -> "remove the dependency from a single project only"
                | Project_Legacy(_) -> "[obsolete]"

                | Force -> "force download and reinstallation of all dependencies"
                | Interactive -> "ask for every project whether to remove the dependency"
                | No_Install -> "do not modify projects"

    type ClearCacheArgs =
        | [<Unique;AltCommandLine("--clear-local")>] ClearLocal
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | ClearLocal -> "Clears local packages folder and paket-files."

    type RestoreArgs =
        | [<Unique;AltCommandLine("-f")>] Force
        | [<Unique;CustomCommandLine("--only-referenced")>] Install_Only_Referenced
        | [<Unique>] Touch_Affected_Refs
        | [<Unique>] Ignore_Checks
        | [<Unique>] Fail_On_Checks

        | [<Unique;AltCommandLine("-g")>] Group of name:string
        | [<Hidden;Unique;CustomCommandLine("group")>] Group_Legacy of name:string

        | [<Unique;AltCommandLine("-p")>] Project of path:string
        | [<Hidden;Unique;CustomCommandLine("project")>] Project_Legacy of path:string

        | References_File of path:string
        | [<Hidden;CustomCommandLine("--references-files")>] References_File_Legacy of path:string list

        | [<Unique>] Target_Framework of framework:string
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Force -> "force download and reinstallation of all dependencies"

                | Group(_) -> "restore dependencies of a single group"
                | Group_Legacy(_) -> "[obsolete]"

                | Install_Only_Referenced -> "only restore packages that are referenced by paket.references files"
                | Touch_Affected_Refs -> "touch project files referencing affected dependencies to help incremental build tools detecting the change"
                | Ignore_Checks -> "do not check if paket.dependencies and paket.lock are in sync"
                | Fail_On_Checks -> "abort if any checks fail"

                | Project(_) -> "restore dependencies of a single project"
                | Project_Legacy(_) -> "[obsolete]"

                | References_File(_) -> "restore packages from a paket.references file; may be repeated"
                | References_File_Legacy(_) -> "[obsolete]"

                | Target_Framework(_) -> "restore only for the specified target framework"

    type SimplifyArgs =
        | [<Unique;AltCommandLine("-i")>] Interactive
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Interactive -> "confirm deletion of every transitive dependency"

    type UpdateArgs =
        | [<ExactlyOnce;MainCommand>] NuGet of package_id:string
        | [<Hidden;ExactlyOnce;CustomCommandLine("nuget")>] NuGet_Legacy of package_id:string

        | [<Unique;AltCommandLine("-V")>] Version of version_constraint:string
        | [<Hidden;Unique;CustomCommandLine("version")>] Version_Legacy of version_constraint:string

        | [<Unique;AltCommandLine("-g")>] Group of name:string
        | [<Hidden;Unique;CustomCommandLine("group")>] Group_Legacy of name:string

        | [<Unique>] Create_New_Binding_Files
        | [<Hidden;Unique;CustomCommandLine("--createnewbindingfiles")>] Create_New_Binding_Files_Legacy

        | [<Unique;AltCommandLine("-f")>] Force
        | [<Unique>] Redirects
        | [<Unique>] Clean_Redirects
        | [<Unique>] No_Install
        | [<Unique>] Keep_Major
        | [<Unique>] Keep_Minor
        | [<Unique>] Keep_Patch
        | [<Unique>] Filter
        | [<Unique>] Touch_Affected_Refs
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | NuGet(_) -> "NuGet package ID"
                | NuGet_Legacy(_) -> "[obsolete]"

                | Group(_) -> "specify dependency group to update (default: all groups)"
                | Group_Legacy(_) -> "[obsolete]"

                | Version(_) -> "dependency version constraint"
                | Version_Legacy(_) -> "[obsolete]"

                | Create_New_Binding_Files -> "create binding redirect files if needed"
                | Create_New_Binding_Files_Legacy -> "[obsolete]"

                | Force -> "force download and reinstallation of all dependencies"
                | Redirects -> "create binding redirects"
                | Clean_Redirects -> "remove binding redirects that were not created by Paket"
                | No_Install -> "do not modify projects"
                | Keep_Major -> "only allow updates that preserve the major version"
                | Keep_Minor -> "only allow updates that preserve the minor version"
                | Keep_Patch -> "only allow updates that preserve the patch version"
                | Touch_Affected_Refs -> "touch project files referencing affected dependencies to help incremental build tools detecting the change"
                | Filter -> "treat the NuGet package ID as a regex to filter packages"

    type FindPackagesArgs =
        | [<ExactlyOnce;MainCommand>] Search of package_ID:string
        | [<Hidden;ExactlyOnce;CustomCommandLine("searchtext")>] Search_Legacy of package_ID:string

        | [<Unique>] Source of source_URL:string
        | [<Hidden;Unique;CustomCommandLine("source")>] Source_Legacy of source_URL:string

        | [<Unique;CustomCommandLine("--max")>] Max_Results of int
        | [<Hidden;Unique;CustomCommandLine("max")>] Max_Results_Legacy of int
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Search(_) -> "search for NuGet package ID"
                | Search_Legacy(_) -> "[obsolete]"

                | Source(_) -> "specify source URL"
                | Source_Legacy(_) -> "[obsolete]"

                | Max_Results(_) -> "limit maximum number of results"
                | Max_Results_Legacy(_) -> "[obsolete]"

    type FixNuspecArgs =
        | [<ExactlyOnce;CustomCommandLine("file")>] File of text:string
        | [<ExactlyOnce;CustomCommandLine("references-file")>] ReferencesFile of text:string
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | File _ -> ".nuspec file to fix transitive dependencies within"
                | ReferencesFile _ -> "paket.references to use"

    type FixNuspecsArgs =
        | [<ExactlyOnce;CustomCommandLine("files")>] Files of nuspecPaths:string list
        | [<CustomCommandLine("references-file")>] ReferencesFile of referencePath:string
        | [<CustomCommandLine("project-file")>] ProjectFile of referencePath:string
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Files _ -> ".nuspec files to fix transitive dependencies within"
                | ReferencesFile _ -> "paket.references to use"
                | ProjectFile _ -> "the proejct file to use"

    type GenerateNuspecArgs =
        | [<ExactlyOnce;CustomCommandLine "project">] Project of project:string
        | [<ExactlyOnce;CustomCommandLine "dependencies">] DependenciesFile of dependenciesPath:string
        | [<ExactlyOnce;CustomCommandLine "output">] Output of output:string
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Project _ -> "generate .nuspec for project"
                | DependenciesFile _ -> "paket.dependencies file used to populate .nuspec file"
                | Output _ -> "output directory of the .nuspec file"

    type ShowInstalledPackagesArgs =
        | [<Unique;AltCommandLine("-a")>] All

        | [<Unique;AltCommandLine("-p")>] Project of path:string
        | [<Hidden;Unique;CustomCommandLine("project")>] Project_Legacy of path:string
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | All -> "include transitive dependencies"

                | Project(_) -> "specify project to show dependencies for"
                | Project_Legacy(_) -> "[obsolete]"

    type ShowGroupsArgs =
        | [<Hidden;NoCommandLine>] NoArgs
    with
        interface IArgParserTemplate with
            member __.Usage = ""

    type FindPackageVersionsArgs =
        | [<ExactlyOnce;MainCommand>] NuGet of package_ID:string
        | [<Hidden;ExactlyOnce;CustomCommandLine("nuget", "name")>] NuGet_Legacy of package_ID:string

        | [<Unique>] Source of source_URL:string
        | [<Hidden;Unique;CustomCommandLine("source")>] Source_Legacy of source_URL:string

        | [<Unique;CustomCommandLine("--max")>] Max_Results of int
        | [<Hidden;Unique;CustomCommandLine("max")>] Max_Results_Legacy of int
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | NuGet(_) -> "NuGet package ID"
                | NuGet_Legacy(_) -> "[obsolete]"

                | Source(_) -> "specify source URL"
                | Source_Legacy(_) -> "[obsolete]"

                | Max_Results(_) -> "limit maximum number of results"
                | Max_Results_Legacy(_) -> "[obsolete]"

    type InfoArgs =
        | [<Unique>] Paket_Dependencies_Dir
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Paket_Dependencies_Dir -> "absolute path of paket.dependencies directory, if exists"

    type PackArgs =
        | [<ExactlyOnce;MainCommand>] Output of path:string
        | [<Hidden;ExactlyOnce;CustomCommandLine("output")>] Output_Legacy of path:string

        | [<Unique>] Build_Config of configuration:string
        | [<Hidden;Unique;CustomCommandLine("buildconfig")>] Build_Config_Legacy of configuration:string

        | [<Unique>] Build_Platform of platform:string
        | [<Hidden;Unique;CustomCommandLine("buildplatform")>] Build_Platform_Legacy of platform:string

        | [<Unique>] Version of version:string
        | [<Hidden;Unique;CustomCommandLine("version")>] Version_Legacy of version:string

        | [<Unique;CustomCommandLine("--template")>] Template_File of path:string
        | [<Hidden;Unique;CustomCommandLine("templatefile")>] Template_File_Legacy of path:string

        | [<CustomCommandLine("--exclude")>] Exclude_Template of package_ID:string
        | [<Hidden;CustomCommandLine("exclude")>] Exclude_Template_Legacy of package_ID:string

        |  Specific_Version of package_ID:string * version:string
        | [<Hidden;CustomCommandLine("specific-version")>] Specific_Version_Legacy of package_ID:string * version:string

        | [<Unique>] Release_Notes of text:string
        | [<Hidden;Unique;CustomCommandLine("releaseNotes")>] Release_Notes_Legacy of text:string

        | [<Unique>] Lock_Dependencies
        | [<Hidden;Unique;CustomCommandLine("lock-dependencies")>] Lock_Dependencies_Legacy

        | [<Unique;CustomCommandLine("--minimum-from-lock-file")>] Lock_Dependencies_To_Minimum
        | [<Hidden;Unique;CustomCommandLine("minimum-from-lock-file")>] Lock_Dependencies_To_Minimum_Legacy

        | [<Unique>] Pin_Project_References
        | [<Hidden;Unique;CustomCommandLine("pin-project-references")>] Pin_Project_References_Legacy

        | [<Unique>] Symbols
        | [<Hidden;Unique;CustomCommandLine("symbols")>] Symbols_Legacy

        | [<Unique>] Include_Referenced_Projects
        | [<Hidden;Unique;CustomCommandLine("include-referenced-projects")>] Include_Referenced_Projects_Legacy

        | [<Unique>] Project_Url of URL:string
        | [<Hidden;Unique;CustomCommandLine("project-url")>] Project_Url_Legacy of URL:string
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Output(_) -> "output directory for .nupkg files"
                | Output_Legacy(_) -> "[obsolete]"

                | Build_Config(_) -> "build configuration that should be packaged (default: Release)"
                | Build_Config_Legacy(_) -> "[obsolete]"

                | Build_Platform(_) -> "build platform that should be packaged (default: check all known platform targets)"
                | Build_Platform_Legacy(_) -> "[obsolete]"

                | Version(_) -> "version of the package"
                | Version_Legacy(_) -> "[obsolete]"

                | Template_File(_) -> "pack a single paket.template file"
                | Template_File_Legacy(_) -> "[obsolete]"

                | Exclude_Template(_) -> "exclude paket.template file by package ID; may be repeated"
                | Exclude_Template_Legacy(_) -> "[obsolete]"

                | Specific_Version(_) -> "version number to use for package ID; may be repeated"
                | Specific_Version_Legacy(_) -> "[obsolete]"

                | Release_Notes(_) -> "release notes"
                | Release_Notes_Legacy(_) -> "[obsolete]"

                | Lock_Dependencies -> "use version constraints from paket.lock instead of paket.dependencies"
                | Lock_Dependencies_Legacy(_) -> "[obsolete]"

                | Lock_Dependencies_To_Minimum -> "use version constraints from paket.lock instead of paket.dependencies and add them as a minimum version; --lock-dependencies overrides this option"
                | Lock_Dependencies_To_Minimum_Legacy(_) -> "[obsolete]"

                | Pin_Project_References -> "pin dependencies generated from project references to exact versions (=) instead of using minimum versions (>=); with --lock-dependencies project references will be pinned even if this option is not specified"
                | Pin_Project_References_Legacy(_) -> "[obsolete]"

                | Symbols -> "create symbol and source packages in addition to library and content packages"
                | Symbols_Legacy(_) -> "[obsolete]"

                | Include_Referenced_Projects -> "include symbols and source from referenced projects"
                | Include_Referenced_Projects_Legacy(_) -> "[obsolete]"

                | Project_Url(_) -> "homepage URL for the package"
                | Project_Url_Legacy(_) -> "[obsolete]"

    type PushArgs =
        | [<ExactlyOnce;MainCommand>] Package of path:string
        | [<Hidden;ExactlyOnce;CustomCommandLine("file")>] Package_Legacy of path:string

        | [<Unique>] Url of url:string
        | [<Hidden;Unique;CustomCommandLine("url")>] Url_Legacy of url:string

        | [<Unique>] Api_Key of key:string
        | [<Hidden;Unique;CustomCommandLine("apikey")>] Api_Key_Legacy of key:string

        | [<Unique>] Endpoint of path:string
        | [<Hidden;Unique;CustomCommandLine("endpoint")>] Endpoint_Legacy of path:string
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Package(_) -> "path to the .nupkg file"
                | Package_Legacy(_) -> "[obsolete]"

                | Url(_) -> "URL of the NuGet feed"
                | Url_Legacy(_) -> "[obsolete]"

                | Api_Key(_) -> "API key for the URL (default: value of the NUGET_KEY environment variable)"
                | Api_Key_Legacy(_) -> "[obsolete]"

                | Endpoint(_) -> "API endpoint to push to (default: /api/v2/package)"
                | Endpoint_Legacy(_) -> "[obsolete]"

    type GenerateLoadScriptsArgs =
        | [<AltCommandLine("-g")>] Group of name:string
        | [<Hidden;CustomCommandLine("groups")>] Group_Legacy of name:string list

        | [<AltCommandLine("-f")>] Framework of framework:string
        | [<Hidden;CustomCommandLine("framework")>] Framework_Legacy of framework:string

        | [<AltCommandLine("-t")>] Type of LanguageFlags
        | [<Hidden;CustomCommandLine("type")>] Type_Legacy of LanguageFlags
    with
      interface IArgParserTemplate with
          member this.Usage =
            match this with
            | Group(_) -> "groups to generate scripts for (default: all groups); may be repeated"
            | Group_Legacy(_) -> "[obsolete]"

            | Framework(_) -> "framework identifier to generate scripts for, such as net45 or netstandard1.6; may be repeated"
            | Framework_Legacy(_) -> "[obsolete]"

            | Type(_) -> "language to generate scripts for; may be repeated"
            | Type_Legacy(_) -> "[obsolete]"

    type WhyArgs =
        | [<ExactlyOnce;MainCommand>] NuGet of package_ID:string
        | [<Hidden;ExactlyOnce;CustomCommandLine("nuget")>] NuGet_Legacy of package_ID:string

        | [<Unique;AltCommandLine("-g")>] Group of name:string
        | [<Hidden;Unique;CustomCommandLine("group")>] Group_Legacy of name:string

        | [<Unique>] Details
    with
      interface IArgParserTemplate with
          member this.Usage =
            match this with
            | NuGet(_) -> "NuGet package ID"
            | NuGet_Legacy(_) -> "[obsolete]"

            | Group(_) -> "specify dependency group (default: Main group)"
            | Group_Legacy(_) -> "[obsolete]"

            | Details -> "display detailed information with all paths, versions and framework restrictions"

    type Command =
        // global options
        |                                                   Version
        | [<AltCommandLine("-s");Inherit>]                  Silent
        | [<AltCommandLine("-v");Inherit>]                  Verbose
        | [<Inherit>]                                       Log_File of path:string
        | [<Hidden;Inherit>]                                From_Bootstrapper
        // subcommands
        | [<CustomCommandLine("add")>]                      Add of ParseResults<AddArgs>
        | [<CustomCommandLine("clear-cache")>]              ClearCache of ParseResults<ClearCacheArgs>
        | [<CustomCommandLine("config")>]                   Config of ParseResults<ConfigArgs>
        | [<CustomCommandLine("convert-from-nuget")>]       ConvertFromNuget of ParseResults<ConvertFromNugetArgs>
        | [<CustomCommandLine("find-refs")>]                FindRefs of ParseResults<FindRefsArgs>
        | [<CustomCommandLine("init")>]                     Init of ParseResults<InitArgs>
        | [<CustomCommandLine("auto-restore")>]             AutoRestore of ParseResults<AutoRestoreArgs>
        | [<CustomCommandLine("install")>]                  Install of ParseResults<InstallArgs>
        | [<CustomCommandLine("outdated")>]                 Outdated of ParseResults<OutdatedArgs>
        | [<CustomCommandLine("remove")>]                   Remove of ParseResults<RemoveArgs>
        | [<CustomCommandLine("restore")>]                  Restore of ParseResults<RestoreArgs>
        | [<CustomCommandLine("simplify")>]                 Simplify of ParseResults<SimplifyArgs>
        | [<CustomCommandLine("update")>]                   Update of ParseResults<UpdateArgs>
        | [<CustomCommandLine("find-packages")>]            FindPackages of ParseResults<FindPackagesArgs>
        | [<CustomCommandLine("find-package-versions")>]    FindPackageVersions of ParseResults<FindPackageVersionsArgs>
        | [<Hidden;CustomCommandLine("fix-nuspec")>]        FixNuspec of ParseResults<FixNuspecArgs>
        | [<CustomCommandLine("fix-nuspecs")>]              FixNuspecs of ParseResults<FixNuspecsArgs>
        | [<CustomCommandLine("generate-nuspec")>]          GenerateNuspec of ParseResults<GenerateNuspecArgs>
        | [<CustomCommandLine("show-installed-packages")>]  ShowInstalledPackages of ParseResults<ShowInstalledPackagesArgs>
        | [<CustomCommandLine("show-groups")>]              ShowGroups of ParseResults<ShowGroupsArgs>
        | [<CustomCommandLine("pack")>]                     Pack of ParseResults<PackArgs>
        | [<CustomCommandLine("push")>]                     Push of ParseResults<PushArgs>
        | [<Hidden;CustomCommandLine("generate-include-scripts")>] GenerateIncludeScripts of ParseResults<GenerateLoadScriptsArgs>
        | [<CustomCommandLine("generate-load-scripts")>]    GenerateLoadScripts of ParseResults<GenerateLoadScriptsArgs>
        | [<CustomCommandLine("why")>]                      Why of ParseResults<WhyArgs>
        | [<CustomCommandLine("info")>]                     Info of ParseResults<InfoArgs>
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Add _ -> "add a new dependency"
                | ClearCache _ -> "clear the NuGet and git cache directories"
                | Config _ -> "store global configuration values like NuGet credentials"
                | ConvertFromNuget _ -> "convert projects from NuGet to Paket"
                | FindRefs _ -> "find all project files that have a dependency installed"
                | Init _ -> "create an empty paket.dependencies file in the current working directory"
                | AutoRestore _ -> "manage automatic package restore during the build process inside Visual Studio"
                | Install _ -> "compute dependency graph, download dependencies and update projects"
                | Outdated _ -> "find dependencies that have newer versions available"
                | Remove _ -> "remove a dependency"
                | Restore _ -> "download the computed dependency graph"
                | Simplify _ -> "simplify declared dependencies by removing transitive dependencies"
                | Update _ -> "update dependencies to their latest version"
                | FindPackages _ -> "search for NuGet packages"
                | FindPackageVersions _ -> "search for dependency versions"
                | FixNuspec _ -> "[obsolete]"
                | FixNuspecs _ -> "patch a list of .nuspec files to correct transitive dependencies"
                | GenerateNuspec _ -> "generate a default nuspec for a project including its direct dependencies"
                | ShowInstalledPackages _ -> "show installed top-level packages"
                | ShowGroups _ -> "show groups"
                | Pack _ -> "create NuGet packages from paket.template files"
                | Push _ -> "push a NuGet package"
                | GenerateIncludeScripts _ -> "[obsolete]"
                | GenerateLoadScripts _ -> "generate F# and C# include scripts that reference installed packages in a interactive environment like F# Interactive or ScriptCS"
                | Why _ -> "determine why a dependency is required"
                | Info _ -> "info"
                | Log_File _ -> "print output to a file"
                | Silent -> "suppress console output"
                | Verbose -> "print detailed information to the console"
                | Version -> "show Paket version"
                | From_Bootstrapper -> "call coming from the '--run' feature of the bootstrapper"

    let commandParser bypassDependencyGraphChecks =
        ArgumentParser.Create<Command>(
            programName = "paket",
            errorHandler = new ExceptionExiter(),
            bypassDependencyGraphChecks = bypassDependencyGraphChecks)

open BenchmarkDotNet.Running
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Attributes.Jobs
open BenchmarkDotNet.Engines
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Jobs
open BenchmarkDotNet.Environments
open System.Runtime

type BenchConfig() as this =
    inherit ManualConfig()

    let iterations = 100

    do
        //this.Add(Job.Core.With(RunStrategy.ColdStart).WithLaunchCount(iterations).WithWarmupCount(0).WithTargetCount(1))
        this.Add(Job.Clr.With(RunStrategy.ColdStart).WithLaunchCount(iterations).WithWarmupCount(0).WithTargetCount(1))

[<Config(typeof<BenchConfig>)>]
type PerfTest() =
    [<Params("restore", "install", "add nuget Foo.Bar")>]
    [<DefaultValue>]
    val mutable Args: string

    [<Params(true, false)>]
    [<DefaultValue>]
    val mutable BypassDependencyGraphChecks: bool

    let mutable args = [||]

    [<GlobalSetup>]
    member this.GlobalSetup() =
        args <- this.Args.Split(' ')

    [<Benchmark>]
    member this.Parse() =
        let parser = PaketCommands.commandParser this.BypassDependencyGraphChecks

        let results = parser.ParseCommandLine(args)

        results.GetSubCommand()

[<Config(typeof<BenchConfig>)>]
type SerializedPerfTest() =
    [<Params("restore", "install", "add nuget Foo.Bar")>]
    [<DefaultValue>]
    val mutable Args: string

    let mutable args = [||]

    let serializedParser: string = "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"

    [<GlobalSetup>]
    member this.GlobalSetup() =
#if NETCOREAPP2_0
#else
        //ProfileOptimization.SetProfileRoot(@"C:\temp\")
        //ProfileOptimization.StartProfile("Profile_Benchmark")
#endif
        args <- this.Args.Split(' ')
    [<Benchmark>]
    member this.Parse() =
        let parser =
            Argu.ArgumentParser.Create<PaketCommands.Command>(
                programName = "paket",
                errorHandler = new Argu.ExceptionExiter(),
                serializedParser = serializedParser)

        let results = parser.ParseCommandLine(args)

        results.GetSubCommand()