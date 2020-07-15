using BenchmarkDotNet.Attributes;
using MicroBenchmarks;

namespace Iterations
{
    [BenchmarkCategory(Categories.Runtime)]
    public class Iterations
    {
        public const int MOD = 998244353;

        [Params(1, 2, 4, 8, 16, 32, 64, 128, 256, 512,
                1024, 2048, 4096, 8192, 16384, 32768)]
        public int N;

        public static int execute(int num_iterations)
        {
            int i;
            int a = 0, b = 1;

            for (i = 0; i < num_iterations; i++)
            {
                int c = a + b;

                if (c >= MOD)
                {
                    c -= MOD;
                }

                a = b;
                b = c;
            }

            return a;
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