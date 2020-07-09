// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Running;
using System.IO;
using BenchmarkDotNet.Extensions;

namespace MicroBenchmarks
{
    class Program
    {
        static int Main(string[] args)
        {
            var argsList = new List<string>(args);
            int? partitionCount;
            int? partitionIndex;
            List<string> exclusionFilterValue;
            List<string> categoryExclusionFilterValue;
            bool getDiffableDisasm;
            bool interpTc;
            bool jitMinOpts;
            bool interpOnly;
            bool tier1Only;

            // Parse and remove any additional parameters that we need that aren't part of BDN
            try {
                argsList = CommandLineOptions.ParseAndRemoveIntParameter(argsList, "--partition-count", out partitionCount);
                argsList = CommandLineOptions.ParseAndRemoveIntParameter(argsList, "--partition-index", out partitionIndex);
                argsList = CommandLineOptions.ParseAndRemoveStringsParameter(argsList, "--exclusion-filter", out exclusionFilterValue);
                argsList = CommandLineOptions.ParseAndRemoveStringsParameter(argsList, "--category-exclusion-filter", out categoryExclusionFilterValue);
                CommandLineOptions.ParseAndRemoveBooleanParameter(argsList, "--disasm-diff", out getDiffableDisasm);
                CommandLineOptions.ParseAndRemoveBooleanParameter(argsList, "--interp-tc", out interpTc);
                CommandLineOptions.ParseAndRemoveBooleanParameter(argsList, "--jit-min-opts", out jitMinOpts);
                CommandLineOptions.ParseAndRemoveBooleanParameter(argsList, "--interp-only", out interpOnly);
                CommandLineOptions.ParseAndRemoveBooleanParameter(argsList, "--tier1-only", out tier1Only);

                if (Convert.ToInt32(interpTc) + Convert.ToInt32(jitMinOpts) + Convert.ToInt32(interpOnly) + Convert.ToInt32(tier1Only) > 1)
                {
                    throw new ArgumentException("Specify exactly one of --interp-tc, --jit-min-opts, --interp-only, and --tier1-only.");
                }

                CommandLineOptions.ValidatePartitionParameters(partitionCount, partitionIndex);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("ArgumentException: {0}", e.Message);
                return 1;
            }

            return BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(argsList.ToArray(), RecommendedConfig.Create(
                    artifactsPath: new DirectoryInfo(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "BenchmarkDotNet.Artifacts")), 
                    mandatoryCategories: ImmutableHashSet.Create(Categories.Libraries, Categories.Runtime, Categories.ThirdParty),
                    partitionCount: partitionCount,
                    partitionIndex: partitionIndex,
                    exclusionFilterValue: exclusionFilterValue,
                    categoryExclusionFilterValue: categoryExclusionFilterValue,
                    getDiffableDisasm: getDiffableDisasm,
                    interpTc: interpTc,
                    jitMinOpts: jitMinOpts,
                    interpOnly: interpOnly,
                    tier1Only: tier1Only))
                .ToExitCode();
        }
    }
}