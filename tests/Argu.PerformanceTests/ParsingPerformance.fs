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