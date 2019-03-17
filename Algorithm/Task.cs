using System;
using System.Collections.Generic;

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
        public readonly Func<double[], double> Function;
        private readonly object _compilation;

        public CostFunction(Func<double[], double> f, object compilation)
        {
            Function = f;
            _compilation = compilation;
        }
    }

    public struct Constraint
    {
        public readonly Func<double[], double> Function;
        public readonly ConstraintType Type;
        private readonly object _compilation;

        public Constraint(Func<double[], double> f, ConstraintType ctype, object compilation)
        {
            Function = f;
            Type = ctype;
            _compilation = compilation;
        }
    }

    public class Task
    {
        public readonly int Rank;
        public readonly CostFunction Cost;
        public readonly List<Constraint> Constraints;

        public Task(int rank, CostFunction costFunction, List<Constraint> constraints)
        {
            Rank = rank;
            Cost = costFunction;
            Constraints = constraints;
        }
    }

}
