using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Qfe
{

    public class GoldenSectionElementaryDirections : DirectionalMinimalization
    {
        //public const double GoldenRatio = 0.61803398;
        public const double GoldenRatio = 0.5;

        public double LeftInterval { get; set; }
        public double RightInterval { get; set; }

        protected double leftInt;
        protected double rightInt;

        protected double wholeInterval()
        {
            return rightInt - leftInt;
        }

        public override FunctionPoint FindMinimum(Vector startPoint)
        {
            Vector left = (Vector)startPoint.Clone();
            Vector right = (Vector)startPoint.Clone();
            leftInt = LeftInterval;
            rightInt = RightInterval;

            left[Direction] = leftInt;
            right[Direction] = rightInt;
            // Sometimes for extermaly big numbers difference between consuequentive double values is bigger than MaxError, so it enters inifinite loop. Iterations limit guards against it.
            int iterations = (int)(wholeInterval() / MaxError) + 1;

            while (wholeInterval() > MaxError && iterations > 0)
            {
                iterations--;
                iterations1++;
                if (Function(left) < Function(right))
                {
                    rightInt = rightInt - GoldenRatio * wholeInterval();
                    right[Direction] = rightInt;
                }
                else
                {
                    leftInt = leftInt + GoldenRatio * wholeInterval();
                    left[Direction] = leftInt;
                }
            }

            FunctionPoint result = new FunctionPoint()
            {
                Point = (Vector)startPoint.Clone()
            };

            result.Point[Direction] = (leftInt + rightInt) * 0.5;
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
                    base.leftInt = point[Direction];
                    base.rightInt = point[Direction] + dx;
                }
                else
                {
                    base.leftInt = point[Direction] + dx;
                    base.rightInt = point[Direction];
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
