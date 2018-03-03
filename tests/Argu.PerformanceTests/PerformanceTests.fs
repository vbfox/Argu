module Argu.PerformanceTests.Program

open BenchmarkDotNet.Running

[<EntryPoint>]
let main (args: string[]) =
    if args.Length = 1 && args.[0] = "single" then
        let instance = PerfTest()
        instance.Args <- "add nuget Foo.Bar"
        instance.BypassDependencyGraphChecks <- true
        instance.CmdLineOnly <- true
        instance.GlobalSetup()
        instance.Parse() |> ignore
    else if args.Length = 1 && args.[0] = "save" then
        let parser = PaketCommands.commandParser true
        printfn "%s" (parser.Save())
        System.IO.File.WriteAllText("parser.txt", parser.Save())
        System.IO.File.WriteAllBytes("parser.bin", System.Convert.FromBase64String(parser.Save()))
    else if args.Length >= 1 && args.[0] = "load" then
        let saved = System.IO.File.ReadAllText("parser.txt")
        let parser =
            Argu.ArgumentParser.Create<PaketCommands.Command>(
                programName = "paket",
                errorHandler = new Argu.ExceptionExiter(),
                serializedParser = saved)

        let parseResult = parser.Parse([|"add";"nuget";"Foo.Bar"|])
        printfn "%A" parseResult

    else
        //let _summary = BenchmarkRunner.Run<PerfTest>()
        let _summary = BenchmarkRunner.Run<SerializedPerfTest>()

        ()
    0