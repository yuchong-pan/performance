using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using MicroBenchmarks;

namespace Iterations
{
    [BenchmarkCategory(Categories.Runtime)]
    public class TailRecursions
    {
        public const int MOD = 998244353;

        [Params(1, 2, 4, 8, 16, 32, 64, 128, 256, 512,
                1024, 2048, 4096, 8192, 16384, 32768)]
        public int N;

        public static int execute(int num_iterations)
        {
            return execute(num_iterations, 0, 1);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static int execute(int num_iterations, int a, int b)
        {
            if (num_iterations == 0)
            {
                return a;
            }

            update(ref a, ref b);
            return execute(num_iterations - 1, a, b);
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

        [Benchmark]
        public void bench2()
        {
            execute(65536 * N);
        }
    }
}