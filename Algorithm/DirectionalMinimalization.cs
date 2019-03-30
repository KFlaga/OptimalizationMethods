using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Qfe
{
    public class FunctionPoint
    {
        public Vector Point { get; set; }
        public double Value { get; set; }
    }

    public abstract class DirectionalMinimalization
    {
        public Func<Vector, double> Function { get; set; }
        public int Direction { get; set; }
        public double MaxError { get; set; } = 1e-3;

        public abstract FunctionPoint FindMinimum(Vector startPoint); 
    }
}
