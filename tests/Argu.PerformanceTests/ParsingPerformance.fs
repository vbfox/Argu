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

type BenchConfig() as this =
    inherit ManualConfig()

    let iterations = 5

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

    [<Params(true, false)>]
    [<DefaultValue>]
    val mutable CmdLineOnly: bool

    let mutable args = [||]

    [<GlobalSetup>]
    member this.GlobalSetup() =
        args <- this.Args.Split(' ')

    [<Benchmark>]
    member this.Parse() =
        let parser = PaketCommands.commandParser this.BypassDependencyGraphChecks
        if this.CmdLineOnly then
            parser.ParseCommandLine(args)
        else
            parser.Parse(args)

[<Config(typeof<BenchConfig>)>]
type SerializedPerfTest() =
    [<Params("restore", "install", "add nuget Foo.Bar")>]
    [<DefaultValue>]
    val mutable Args: string

    [<Params(true, false)>]
    [<DefaultValue>]
    val mutable CmdLineOnly: bool

    let mutable args = [||]

    let serializedParser: string = "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"

    [<GlobalSetup>]
    member this.GlobalSetup() =
        args <- this.Args.Split(' ')

    [<Benchmark>]
    member this.Parse() =
        let parser =
            Argu.ArgumentParser.Create<PaketCommands.Command>(
                programName = "paket",
                errorHandler = new Argu.ExceptionExiter(),
                serializedParser = serializedParser)

        if this.CmdLineOnly then
            parser.ParseCommandLine(args)
        else
            parser.Parse(args)