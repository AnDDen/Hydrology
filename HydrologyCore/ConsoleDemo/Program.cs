using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HydrologyCore;
using AlgorithmInterface;

namespace ConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Core core = new Core();

            Experiment experiment = new Experiment();
            experiment.StartFrom("experiment/initial")
                .Then(core.Algorithm("RepresentationCheck").InitFromFolder("experiment/alg1/input/"))
                .Then(core.Algorithm("Statistics").InitFromFolder("experiment/alg2/input/"));

            Console.WriteLine("Starting experiment");
            experiment.Run();
            Console.WriteLine("Experiment finished");
            Console.ReadKey();
        }
    }
}
