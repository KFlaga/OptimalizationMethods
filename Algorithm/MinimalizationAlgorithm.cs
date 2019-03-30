using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

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
        public int Iteration { get; set; }

        public double CurrentCost { get; set; }
        public Vector CurrentPoint { get; set; }
        
        public double CurrentFunction { get; set; }
        public double LastPointChange { get; set; }
        public double LastFunuctionChange { get; set; }
        public bool CostraintsMet { get; set; }
    }

    public abstract class IterativeMinimalization
    {
        public Qfe.Task Task { get; set; }

        public InitialPointMethod InitializationMethod { get; set; } = InitialPointMethod.Manual;
        public Vector InitialPoint { get; set; }

        public int MaxIterations { get; set; }
        public double MinPositionChange { get; set; }
        public double MinFunctionChange { get; set; }
        public double MysteriusCriteria { get; set; }

        protected List<IterationResults> iterations = new List<IterationResults>();

        public bool Terminated { get; set; }
        public ReadOnlyCollection<IterationResults> Results => new ReadOnlyCollection<IterationResults>(iterations);

        public virtual IterationResults Solve()
        {
            Init();
            int iteration = 1;
            Vector point = InitialPoint;

            do
            {
                point = SolveIteration(point, iteration).CurrentPoint;
                ++iteration;
            }
            while (!ShouldEnd());
            return iterations.Last();
        }

        protected abstract IterationResults SolveIteration(Vector point, int iteration);

        protected virtual void Init()
        {
            double fvalue = cost(InitialPoint);
            iterations = new List<IterationResults>(MaxIterations + 1)
            {
                new IterationResults()
                {
                    Iteration = 0,
                    CurrentPoint = InitialPoint,
                    CurrentFunction = fvalue,
                    CurrentCost = fvalue,
                    CostraintsMet = true,
                    LastFunuctionChange = Math.Abs(fvalue),
                    LastPointChange = InitialPoint.L2Norm(),

                }
            };
        }

        protected virtual bool ShouldEnd()
        {
            var lastIteration = iterations.Last();
            return Terminated ||
                   lastIteration.Iteration > MaxIterations ||
                   (lastIteration.LastPointChange < MinPositionChange &&
                    lastIteration.LastFunuctionChange < MinFunctionChange);
        }

        protected double cost(Vector x)
        {
            return Task.Cost.Function(x);
        }

        protected double constraint(int index, Vector x)
        {
            return Task.Constraints[index].Evaluate(x);
        }
    }
}
