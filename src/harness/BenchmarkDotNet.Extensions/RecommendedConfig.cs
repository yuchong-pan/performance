﻿using System.Collections.Immutable;
using System.IO;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using Perfolizer.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using System.Collections.Generic;

namespace BenchmarkDotNet.Extensions
{
    public static class RecommendedConfig
    {
        private const string COMPlusPrefix                           = "COMPlus_";
        private const string Interpret                               = COMPlusPrefix + "Interpret";
        private const string InterpreterJITThreshold                 = COMPlusPrefix + "InterpreterJITThreshold";
        private const string TC_CallCountThreshold                   = COMPlusPrefix + "TC_CallCountThreshold";
        private const string TieredCompilation                       = COMPlusPrefix + "TieredCompilation";
        private const string InterpreterHWIntrinsicsIsSupportedFalse = COMPlusPrefix + "InterpreterHWIntrinsicsIsSupportedFalse";
        private const string InterpreterDoLoopMethods                = COMPlusPrefix + "InterpreterDoLoopMethods";
        private const string InterpreterPrintPostMortem              = COMPlusPrefix + "InterpreterPrintPostMortem";
        private const string JitMinOpts                              = COMPlusPrefix + "JitMinOpts";
        private const string ForceInterpreter                        = COMPlusPrefix + "ForceInterpreter";

        public static IConfig Create(
            DirectoryInfo artifactsPath,
            ImmutableHashSet<string> mandatoryCategories,
            int? partitionCount = null,
            int? partitionIndex = null,
            List<string> exclusionFilterValue = null,
            List<string> categoryExclusionFilterValue = null,
            Job job = null,
            bool getDiffableDisasm = false,
            bool interpTc = false,
            bool jitMinOpts = false,
            bool interpOnly = false)
        {
            if (job is null)
            {
                job = Job.Default
                    .WithWarmupCount(1) // 1 warmup is enough for our purpose
                    .WithIterationTime(TimeInterval.FromMilliseconds(250)) // the default is 0.5s per iteration, which is slighlty too much for us
                    .WithMinIterationCount(15)
                    .WithMaxIterationCount(20) // we don't want to run more than 20 iterations
                    .DontEnforcePowerPlan(); // make sure BDN does not try to enforce High Performance power plan on Windows

                // See https://github.com/dotnet/roslyn/issues/42393
                job = job.WithArguments(new Argument[] { new MsBuildArgument("/p:DebugType=portable") });
            }
            
            if (interpTc)
            {
                job = job
                .WithEnvironmentVariables(
                    new EnvironmentVariable(Interpret, "*"),
                    new EnvironmentVariable(InterpreterJITThreshold, "0"),
                    new EnvironmentVariable(TC_CallCountThreshold, "0"),
                    new EnvironmentVariable(TieredCompilation, "1"),
                    new EnvironmentVariable(InterpreterHWIntrinsicsIsSupportedFalse, "1"),
                    new EnvironmentVariable(InterpreterDoLoopMethods, "1"),
                    new EnvironmentVariable(InterpreterPrintPostMortem, "1")
                )
                .WithId("Interpreter Tiered Compilation");
            }
            else if (jitMinOpts)
            {
                job = job
                .WithEnvironmentVariables(
                    new EnvironmentVariable(JitMinOpts, "1")
                )
                .WithId("JIT Min Opts");
            }
            else if (interpOnly)
            {
                job = job
                .WithEnvironmentVariables(
                    new EnvironmentVariable(Interpret, "*"),
                    new EnvironmentVariable(InterpreterJITThreshold, "9999999"),
                    new EnvironmentVariable(TC_CallCountThreshold, "9999999"),
                    new EnvironmentVariable(TieredCompilation, "0"),
                    new EnvironmentVariable(InterpreterHWIntrinsicsIsSupportedFalse, "1"),
                    new EnvironmentVariable(InterpreterDoLoopMethods, "1"),
                    new EnvironmentVariable(InterpreterPrintPostMortem, "1"),
                    new EnvironmentVariable(ForceInterpreter, "1")
                )
                .WithId("Interpreter Only");
            }
            else
            {
                job = job.WithId("Default");
            }

            var config = DefaultConfig.Instance
                .AddJob(job.AsDefault()) // tell BDN that this is our default settings
                .WithArtifactsPath(artifactsPath.FullName)
                .AddDiagnoser(MemoryDiagnoser.Default) // MemoryDiagnoser is enabled by default
                .AddFilter(new OperatingSystemFilter())
                .AddFilter(new PartitionFilter(partitionCount, partitionIndex))
                .AddFilter(new ExclusionFilter(exclusionFilterValue))
                .AddFilter(new CategoryExclusionFilter(categoryExclusionFilterValue))
                .AddExporter(JsonExporter.Full) // make sure we export to Json
                .AddExporter(new PerfLabExporter())
                .AddColumn(StatisticColumn.Median, StatisticColumn.Min, StatisticColumn.Max)
                .AddValidator(TooManyTestCasesValidator.FailOnError)
                .AddValidator(new UniqueArgumentsValidator()) // don't allow for duplicated arguments #404
                .AddValidator(new MandatoryCategoryValidator(mandatoryCategories))
                .WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(36)); // the default is 20 and trims too aggressively some benchmark results

            if (getDiffableDisasm)
            {
                config = config.AddDiagnoser(CreateDisassembler());
            }

            return config;
        }

        private static DisassemblyDiagnoser CreateDisassembler()
            => new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(
                maxDepth: 1, // TODO: is depth == 1 enough?
                formatter: null, // TODO: enable diffable format
                printSource: false, // we are not interested in getting C#
                printInstructionAddresses: false, // would make the diffing hard, however could be useful to determine alignment
                exportGithubMarkdown: false,
                exportHtml: false,
                exportCombinedDisassemblyReport: false,
                exportDiff: false));
    }
}
