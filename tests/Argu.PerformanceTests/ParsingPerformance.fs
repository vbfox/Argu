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

    let iterations = 10

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
        ProfileOptimization.SetProfileRoot(@"C:\temp\")
        ProfileOptimization.StartProfile("Profile_Benchmark")
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