using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Qfe
{
    public enum ConstraintType
    {
        Equality,
        LessEqual,
        GreaterEqual
    }

    public struct CostFunction
    {
        public readonly Func<Vector, double> Function;
        private readonly object _compilation;

        public CostFunction(Func<Vector, double> f, object compilation)
        {
            Function = f;
            _compilation = compilation;
        }
    }

    public struct Constraint
    {
        public readonly Func<Vector, double> Function;
        public readonly ConstraintType Type;
        private readonly object _compilation;

        public Constraint(Func<Vector, double> f, ConstraintType ctype, object compilation)
        {
            Function = f;
            Type = ctype;
            _compilation = compilation;
        }

        public double Evaluate(Vector x)
        {
            // Returns c for type c >= 0 || c == 0 and -c for type c <= 0
            double c = Function(x);
            if (Type == ConstraintType.LessEqual)
            {
                return -c;
            }
            return c;
        }

        public bool IsMet(Vector x, double maxError = 1e-3)
        {
            double c = Evaluate(x);
            return Type == ConstraintType.Equality ? Math.Abs(c - maxError) < maxError : c > -maxError;
        }
    }

    public class Task
    {
        public readonly int Rank;
        public readonly CostFunction Cost;
        public readonly List<Constraint> Constraints;
        public readonly string Input;

        public Task(int rank, CostFunction costFunction, List<Constraint> constraints, string input)
        {
            Rank = rank;
            Cost = costFunction;
            Constraints = constraints;
            Input = input;
        }
    }

}
