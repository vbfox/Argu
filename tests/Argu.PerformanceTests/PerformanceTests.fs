module Argu.PerformanceTests.Program

open BenchmarkDotNet.Running
open System.Runtime

let load (saved: string) =
    let parser =
        Argu.ArgumentParser.Create<PaketCommands.Command>(
            programName = "paket",
            errorHandler = new Argu.ExceptionExiter(),
            serializedParser = saved)

    let results = parser.ParseCommandLine([|"add";"nuget";"Foo.Bar"|])
    let subCommand = results.GetSubCommand()
    subCommand

[<EntryPoint>]
let main (args: string[]) =
    if args.Length = 1 && args.[0] = "single" then
#if NETCOREAPP2_0
#else
        ProfileOptimization.SetProfileRoot(@"C:\temp\")
        ProfileOptimization.StartProfile("Profile_Benchmark")
#endif
        let instance = PerfTest()
        instance.Args <- "add nuget Foo.Bar"
        instance.BypassDependencyGraphChecks <- true
        instance.GlobalSetup()
        let subCommand = instance.Parse()
        printfn "%A" subCommand
    if args.Length = 1 && args.[0] = "serialized" then
#if NETCOREAPP2_0
#else
        ProfileOptimization.SetProfileRoot(@"C:\temp\")
        ProfileOptimization.StartProfile("Profile_Benchmark")
#endif
        let instance = SerializedPerfTest()
        instance.Args <- "add nuget Foo.Bar"
        instance.GlobalSetup()
        let subCommand = instance.Parse()
        printfn "%A" subCommand
    else if args.Length = 1 && args.[0] = "save" then
#if NETCOREAPP2_0
#else
        ProfileOptimization.SetProfileRoot(@"C:\temp\")
        ProfileOptimization.StartProfile("Profile_Benchmark")
#endif
        let parser = PaketCommands.commandParser true
        printfn "%s" (parser.Save())
        System.IO.File.WriteAllText("parser.txt", parser.Save())
        System.IO.File.WriteAllBytes("parser.bin", System.Convert.FromBase64String(parser.Save()))
    else if args.Length >= 1 && args.[0] = "load" then
        let saved = System.IO.File.ReadAllText("parser.txt")
        let subCommand = load saved
        printfn "%A" subCommand

    else
        let _summary = BenchmarkRunner.Run<PerfTest>()
        let _summary = BenchmarkRunner.Run<SerializedPerfTest>()

        ()
    //let _summary = BenchmarkRunner.Run<SerializedPerfTest>()
    0