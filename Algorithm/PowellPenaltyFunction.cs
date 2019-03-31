using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;

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

        public abstract void NextIteration();
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
        public enum SigmaChangeMethod
        {
            Constant,
            Additive,
            Multiplicative
        }

        public SigmaChangeMethod ChangeMethod { get; set; }
        public double SigmaIncrement { get; set; }

        private double initialSigma;

        public ZeroThetaPenalty(
            List<Constraint> constraints,
            SigmaChangeMethod changeMethod = SigmaChangeMethod.Multiplicative,
            double initialValue = 1.0,
            double increment = 10.0) :
            base(constraints)
        {
            ChangeMethod = changeMethod;
            initialSigma = initialValue;
            SigmaIncrement = increment;
            Reset();
        }

        public override void NextIteration()
        {
            if (N > 0)
            {
                if (ChangeMethod == SigmaChangeMethod.Additive)
                {
                    Sigma = (Vector)Sigma.Add(SigmaIncrement);
                }
                else if (ChangeMethod == SigmaChangeMethod.Multiplicative)
                {
                    Sigma = (Vector)Sigma.Multiply(SigmaIncrement);
                }
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
}
