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
            IList<AlgorithmNode> algs = new List<AlgorithmNode>();
            algs.Add(new AlgorithmNode(core.AlgorithmTypes[0], "experiment/alg1/input/", "experiment/alg1/output/"));
            algs.Add(new AlgorithmNode(core.AlgorithmTypes[0], "experiment/alg2/input/", "experiment/alg2/output/"));
            algs.Add(new AlgorithmNode(core.AlgorithmTypes[0], "experiment/alg3/input/", "experiment/alg3/output/"));
            AlgorithmNode.Connect(algs[0], algs[1]);
            AlgorithmNode.Connect(algs[1], algs[2]);

            Experiment experiment = new Experiment(algs);
            //Experiment experiment = new Experiment();
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
