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
        
        public double CurrentFunction { get; set; } // TODO: This is a bit confusing -> consider removing it or renaming fields
        public double LastPointChange { get; set; }
        public double LastFunuctionChange { get; set; }
        public double MaxConstraintValue { get; set; }
    }

    public abstract class IterativeMinimalization
    {
        public Qfe.Task Task { get; set; }
        
        public Vector InitialPoint { get; set; }

        public int MaxIterations { get; set; }
        public double MinPositionChange { get; set; }
        public double MinFunctionChange { get; set; }

        protected List<IterationResults> iterations = new List<IterationResults>();

        public virtual bool Terminated { get; set; }
        public ReadOnlyCollection<IterationResults> Results => new ReadOnlyCollection<IterationResults>(iterations);

        public virtual IterationResults Solve()
        {
            Init();
            int iteration = 1;
            Vector point = (Vector)InitialPoint.Clone();

            do
            {
                point = SolveIteration(point, iteration).CurrentPoint;
                ++iteration;
            }
            while (!ShouldEnd());
            return iterations.Last();
        }

        protected abstract IterationResults SolveIteration(Vector point, int iteration);
        protected abstract void Init();

        protected virtual bool ShouldEnd()
        {
            var lastIteration = iterations.Last();
            return Terminated ||
                   lastIteration.Iteration > MaxIterations ||
                   (lastIteration.LastPointChange < MinPositionChange &&
                    lastIteration.LastFunuctionChange < MinFunctionChange);
        }
    }
}
