using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Qfe
{
    // F(x, O, S) = f(x) + 0.5 * H((c(x) - O)^T S (c(x) - O))
    // F(x, O, S) = f(x) + 0.5 * Sum{si * H(ci(x) - Oi)^2}
    // H(a) = min(a, 0)
    // c(x) >= 0
    public abstract class PowellPenaltyFunction
    {
        public PowellPenaltyFunction(List<Constraint> constraints)
        {
            Constraints = constraints;
            if (N > 0)
            {
                Theta = new DenseVector(N);
                Sigma = new DenseVector(N);
            }
        }

        public List<Constraint> Constraints { get; protected set; }

        public Vector Theta { get; protected set; }
        public Vector Sigma { get; protected set; }

        protected int N => Constraints.Count;

        public double Evaluate(Vector x)
        {
            double c_sum = 0.0;
            for (int i = 0; i < N; ++i)
            {
                double c = Constraints[i].Evaluate(x);
                double h = Math.Min(0.0, c - Theta[i]);
                c_sum += Sigma[i] * h * h;
            }
            return 0.5 * c_sum;
        }

        public double MaxConstraint(Vector x)
        {
            double m = 0.0;
            for (int i = 0; i < N; ++i)
            {
                m = Math.Max(m, Math.Abs(Math.Min(Constraints[i].Evaluate(x), Theta[i])));
            }
            return m;
        }

        public abstract void NextIteration(Vector x);
        public abstract void Reset();

        protected PowellPenaltyFunction(List<Constraint> constraints, DenseVector initialTheta, DenseVector initialSigma)
        {
            Constraints = constraints;
            Theta = initialTheta;
            Sigma = initialSigma;
        }
    }

    public class ZeroThetaPenalty : PowellPenaltyFunction
    {
        public double SigmaMultiplier { get; set; }

        private double initialSigma;

        public ZeroThetaPenalty(
            List<Constraint> constraints,
            double initialValue = 1.0,
            double increment = 10.0) :
            base(constraints)
        {
            initialSigma = initialValue;
            SigmaMultiplier = increment;
            Reset();
        }

        public override void NextIteration(Vector x)
        {
            if (N > 0)
            {
                Sigma = (Vector)Sigma.Multiply(SigmaMultiplier);
            }
        }

        public override void Reset()
        {
            if (N > 0)
            {
                Sigma = (Vector)new DenseVector(N).Add(initialSigma);
            }
        }
    }

    // dOi = -min(ci, 0.0)
    // To be accurate requires dS >> S (e.g. multiplier = 10)
    // O(0) = 0
    public class SimpleThetaPenalty : PowellPenaltyFunction
    {
        public double SigmaMultiplier { get; set; }

        private double initialSigma;

        public SimpleThetaPenalty(
            List<Constraint> constraints,
            double initialValue = 1.0,
            double increment = 10.0) :
            base(constraints)
        {
            initialSigma = initialValue;
            SigmaMultiplier = increment;
            Reset();
        }

        public override void NextIteration(Vector x)
        {
            if (N > 0)
            {
                Sigma = (Vector)Sigma.Multiply(SigmaMultiplier);
                for(int i = 0; i < N; ++i)
                {
                    Theta[i] -= Math.Min(Constraints[i].Evaluate(x), Theta[i]);
                    Theta[i] /= SigmaMultiplier;
                }
            }
        }

        public override void Reset()
        {
            if (N > 0)
            {
                Sigma = (Vector)new DenseVector(N).Add(initialSigma);
                Theta = (Vector)new DenseVector(N);
            }
        }
    }

    // As suggested by Powell with Fletcher modification
    public class ProperThetaPenalty : PowellPenaltyFunction
    {
        public const double SigmaMultiplier = 10.0;

        private double initialSigma;
        private double previousMaxConstraint = 0.0;

        public ProperThetaPenalty(
            List<Constraint> constraints,
            double initialSigma = 1.0) :
            base(constraints)
        {
            this.initialSigma = initialSigma;
            Reset();
        }

        public override void NextIteration(Vector x)
        {
            if (N > 0)
            {
                double maxConstraint = MaxConstraint(x);
                if (maxConstraint >= previousMaxConstraint)
                {
                    IncrementSigma(x);
                }
                else
                {
                    for (int i = 0; i < N; ++i)
                    {
                        Theta[i] -= Math.Min(Constraints[i].Evaluate(x), Theta[i]);
                    }
                    if (maxConstraint > 0.25 * previousMaxConstraint)
                    {
                        IncrementSigma(x);
                    }
                    previousMaxConstraint = maxConstraint;
                }
            }
        }

        private void IncrementSigma(Vector x)
        {
            for (int i = 0; i < N; ++i)
            {
                if (Math.Abs(Math.Min(Constraints[i].Evaluate(x), Theta[i])) >= 0.25 * previousMaxConstraint)
                {
                    Sigma[i] = Sigma[i] * SigmaMultiplier;
                    Theta[i] = Theta[i] / SigmaMultiplier;
                }
            }
        }

        public override void Reset()
        {
            if (N > 0)
            {
                Sigma = (Vector)new DenseVector(N).Add(initialSigma);
                Theta = (Vector)new DenseVector(N);
                previousMaxConstraint = 0.0;
            }
        }
    }
}
