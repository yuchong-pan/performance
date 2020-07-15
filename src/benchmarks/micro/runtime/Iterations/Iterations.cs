using BenchmarkDotNet.Attributes;
using MicroBenchmarks;

namespace Iterations
{
    [BenchmarkCategory(Categories.Runtime)]
    public class Iterations
    {
        [Params(1, 2, 4, 8, 16, 32, 64, 128, 256, 512,
                1024, 2048, 4096, 8192, 16384, 32768)]
        public int N;

        public static int execute(int num_iterations)
        {
            int i;
            for (i = 0; i < num_iterations; i++);
            return i;
        }

        [Benchmark]
        public void bench()
        {
            execute(N);
        }
    }
}