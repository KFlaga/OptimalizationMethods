using System;
using System.Collections.Generic;

namespace Algorithm
{
    public enum ConstraintType
    {
        Equality,
        LessEqual,
        GreaterEqual
    }

    public struct CostFunction
    {
        public Func<double[], double> Function;

        internal object _compilation;
    }

    public struct Constraint
    {
        public Func<double[], double> Function;
        public ConstraintType Type;

        internal object _compilation;
    }

    public class Task
    {
        public int Rank;
        public CostFunction Cost;
        public List<Constraint> Constraints;
    }

}
