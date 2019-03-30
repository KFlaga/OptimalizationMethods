using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Qfe
{

    public class GoldenSectionElementaryDirections : DirectionalMinimalization
    {
        public const double GoldenRatio = 0.61803398;

        public double LeftInterval { get; set; }
        public double RightInterval { get; set; }

        private double wholeInterval()
        {
            return RightInterval - LeftInterval;
        }

        public override FunctionPoint FindMinimum(Vector startPoint)
        {
            Vector left = (Vector)startPoint.Clone();
            Vector right = (Vector)startPoint.Clone();

            left[Direction] = RightInterval - GoldenRatio * wholeInterval();
            right[Direction] = LeftInterval + GoldenRatio * wholeInterval();

            while (wholeInterval() > MaxError)
            {
                if (Function(left) < Function(right))
                {
                    RightInterval = right[Direction];
                    right[Direction] = left[Direction];
                    left[Direction] = RightInterval - GoldenRatio * wholeInterval();
                }
                else
                {
                    LeftInterval = left[Direction];
                    left[Direction] = right[Direction];
                    right[Direction] = LeftInterval + GoldenRatio * wholeInterval();
                }
            }

            FunctionPoint result = new FunctionPoint()
            {
                Point = (Vector)startPoint.Clone()
            };

            result.Point[Direction] = (LeftInterval + RightInterval) * 0.5;
            result.Value = Function(result.Point);
            return result;
        }
    }

    public class GoldenSectionWithInterpolatedStep : GoldenSectionElementaryDirections
    {
        public override FunctionPoint FindMinimum(Vector startPoint)
        {
            // Find potential minimum with linear interpolation of derivative (so quadric interpolation of function)
            // f'(x) = f'(x0) + f''(x0) * (x - x0) = 0
            // dx = -f'(x0) / f''(x0)

            double lastValue = Function(startPoint);
            double newValue = lastValue;
            Vector point = (Vector)startPoint.Clone();
            FunctionPoint result;
            do
            {
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
                            Value = Function(point)
                        };
                    }
                    else
                    {
                        // let's move a bit in arbitrary direction
                        df = 1.0;
                    }
                }
                else if (Math.Abs(df2) * 1000.0 < Math.Abs(df))
                {
                    // We are at point f''(x) ~= 0, so we may assume function is linear (at least localy)
                    // If so it has no minima, so lets pick arbitrary step of -df
                    df2 = 1.0;
                }

                double dx = -df / df2;

                if (dx > 0.0)
                {
                    base.LeftInterval = point[Direction];
                    base.RightInterval = point[Direction] + dx;
                }
                else
                {
                    base.LeftInterval = point[Direction] + dx;
                    base.RightInterval = point[Direction];
                }
                result = base.FindMinimum(point);
                point = result.Point;

                lastValue = newValue;
                newValue = result.Value;
            }
            while (Math.Abs(lastValue - newValue) > MaxError);

            return result;
        }
    }
}
