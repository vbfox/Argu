module Argu.PerformanceTests.Program

open BenchmarkDotNet.Running

[<EntryPoint>]
let main (_args: string[]) =
    let _summary = BenchmarkRunner.Run<PerfTest>()
    0