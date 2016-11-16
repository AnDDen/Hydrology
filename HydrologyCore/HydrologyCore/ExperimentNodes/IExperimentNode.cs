using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore
{
    public interface IExperimentNode
    {
        /// <summary>
        /// Next node in execution chain
        /// </summary>
        IExperimentNode Next { get; set; }

        /// <summary>
        /// Previous node in execution chain
        /// </summary>
        IExperimentNode Prev { get; set; }

        /// <summary>
        /// Run current node with context ctx
        /// </summary>
        /// <param name="ctx">Context</param>
        void Run(IContext ctx);

        /// <summary>
        /// Data set with node execution results
        /// </summary>
        DataSet Results { get; }

        /// <summary>
        /// Node name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Sets if execution results should be saved
        /// </summary>
        bool IsSaveResults { get; set; }

        /// <summary>
        /// Name for the folder where results will be saved
        /// </summary>
        string SaveResultsFolder { get; set; }

        /// <summary>
        /// Sets the path where execution results will be saved if SaveResults property is true
        /// </summary>
        string SaveResultsPath { get; set; }

        /// <summary>
        /// Saves execution results to SaveResultsPath if SaveResults property is true
        /// </summary>
        void SaveResults();

        /// <summary>
        /// Sets if node should be stored in context
        /// </summary>
        bool IsStoreInContext { get; }
    }
}
