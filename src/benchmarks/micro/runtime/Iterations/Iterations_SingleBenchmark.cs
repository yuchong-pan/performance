using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using MicroBenchmarks;

namespace Iterations
{
    [BenchmarkCategory(Categories.Runtime)]
    public class Iterations_SingleBenchmark
    {
        public const int MOD = 998244353;

        [Params(268435456)]
        public int N;

        public static int execute(int num_iterations)
        {
            int i;
            int a = 0, b = 1;

            for (i = 0; i < num_iterations; i++)
            {
                update(ref a, ref b);
            }

            return a;
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static void update(ref int a, ref int b)
        {
            int c = a + b;

            if (c >= MOD)
            {
                c -= MOD;
            }

            a = b;
            b = c;
        }

        [Benchmark]
        public void bench()
        {
            execute(N);
        }
    }
}