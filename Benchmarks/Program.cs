using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            ParserBenchmark pb = new ParserBenchmark();
            pb.GlobalSetup();

            BenchmarkSwitcher.FromTypes(new Type[]
            {
                typeof(ParserBenchmark)
            })
            .RunAllJoined();
            Console.ReadKey();
        }
    }
}
