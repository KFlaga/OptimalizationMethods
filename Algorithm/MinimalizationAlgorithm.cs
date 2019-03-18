using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qfe
{
    public enum InitialPointMethod
    {
        Manual,
        Random,
        RandomMultistart
    }

    public class IterationResults
    {
        public int Iteratrion { get; set; }

        public double CurrentCost { get; set; }
        public double[] CurrentPoint { get; set; }
        
        public double CurrentFunction { get; set; }
        public double LastPointChange { get; set; }
        public double LastFunuctionChange { get; set; }
        public bool CostraintsMet { get; set; }
    }

    public abstract class MinimalizationAlgorithm
    {
        public Qfe.Task Task { get; set; }

        public InitialPointMethod InitializationMethod { get; set; }
        public double[] InitialValues { get; set; }

        public int MaxIterations { get; set; }
        public double MinPositionChange { get; set; }
        public double MinFunctionChange { get; set; }
        public double MysteriusCriteria { get; set; }
        
        protected double cost(double[] x)
        {
            return Task.Cost.Function(x);
        }

        protected double constraint(int index, double[] x)
        {
            // ensure constraints are c(x) >= 0
            double c = Task.Constraints[index].Function(x);
            return Task.Constraints[index].Type == ConstraintType.LessEqual ? -c : c;
        }

        public abstract void Solve();
        public abstract void Terminate();
        public abstract List<IterationResults> GetResults();
    }
}
