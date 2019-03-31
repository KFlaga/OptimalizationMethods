using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Qfe
{
    public class NewtonMethod : DirectionalMinimalization
    {
        public double MinPointChange { get; set; }

        public override FunctionPoint FindMinimum(Vector startPoint)
        {
            // Find potential minimum with linear interpolation of derivative (so quadric interpolation of function)
            // f'(x) = f'(x0) + f''(x0) * (x - x0) = 0
            // dx = -f'(x0) / f''(x0)
            
            Vector point = (Vector)startPoint.Clone();
            double lastPosition, lastValue;
            int iteration = 0;
            do
            {
                lastPosition = point[Direction];
                lastValue = Function(point);
                iteration++;

                double df = NumericalDerivative.First(Function, point, Direction);
                double df2 = NumericalDerivative.Second(Function, point, Direction);

                if (Math.Abs(df) < MaxError)
                {
                    // We are at point f'(x) ~= 0
                    if (df2 > 0.0) // Ad we have minumum, so we're good
                    {
                        return new FunctionPoint()
                        {
                            Point = point,
                            Value = lastValue
                        };
                    }
                    else
                    {
                        // let's move a bit in arbitrary direction
                        df = 1.0;
                    }
                }
                // TODO: handle case df2 ~ 0.0
                if(df > 0.0) // Move in direction which minimizes f(x)
                {
                    point[Direction] -= Math.Abs(df / df2);
                }
                else
                {
                    point[Direction] += Math.Abs(df / df2);
                }
            }
            while (iteration < MaxIterations &&
                   (Math.Abs(lastValue - Function(point)) > MaxError ||
                    Math.Abs(lastPosition - point[Direction]) > MinPointChange));

            return new FunctionPoint()
            {
                Point = point,
                Value = Function(point)
            };
        }
    }
}
