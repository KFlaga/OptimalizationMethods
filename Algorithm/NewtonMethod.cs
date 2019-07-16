using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Qfe
{
    public class NewtonMethod : DirectionalMinimalization
    {
        public double MinPointChange { get; set; }
        public bool DisableModTwo;

        const double df2_error = 1e-12;

        GoldenSectionElementaryDirections goldenSection = new GoldenSectionElementaryDirections();

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
                iterations1++;

                double df = NumericalDerivative.First(Function, point, Direction, MaxError/100);
                double df2 = NumericalDerivative.Second(Function, point, Direction, MaxError / 100);
                double step = Math.Abs(df / df2);

                if (Math.Abs(df) < MaxError || step < MinPointChange)
                {
                    // We are at point f'(x) ~= 0
                    double fhl = NumericalDerivative.fhLeft(Function, point, Direction, MaxError);
                    double fhr = NumericalDerivative.fhRight(Function, point, Direction, MaxError);

                    // We cound check if f''(x) > 0, but due to non-continous derivative of penalty function sometimes it may fail
                    if (fhl > lastValue && fhr > lastValue)
                    {
                        return new FunctionPoint()
                        {
                            Point = point,
                            Value = lastValue
                        };
                    }
                    else if(!DisableModTwo)
                    {
                        // Most probably we are close to maximum and function is quite flat. To give it a kick lets use golden section.
                        point = useGoldenSection(point, lastValue, step, fhl, fhr);
                    }
                }
                else
                {
                    if (Math.Abs(df2) < df2_error)
                    {
                        df2 = 1.0;
                    }
                    if (df > 0.0) // Move in direction which minimizes f(x)
                    {
                        point[Direction] -= Math.Abs(df / df2);
                    }
                    else
                    {
                        point[Direction] += Math.Abs(df / df2);
                    }

                    double newValue = Function(point);
                    throwOnInvalidValue(newValue, point, lastPosition, Direction);
                }
            }
            while (iteration < MaxIterations &&
                   (Math.Abs(lastValue - Function(point)) > MaxError ||
                    Math.Abs(lastPosition - point[Direction]) > MinPointChange));

            if(iteration == MaxIterations)
            {
                // It doesnt converge
            }

            return new FunctionPoint()
            {
                Point = point,
                Value = Function(point)
            };
        }

        private Vector useGoldenSection(Vector point, double lastValue, double step, double fhl, double fhr)
        {
            goldenSection.Direction = Direction;
            goldenSection.Function = Function;
            goldenSection.MaxError = MaxError;
            goldenSection.MaxIterations = MaxIterations;
            if (step == 0 || double.IsNaN(step))
            {
                step = MinPointChange;
            }

            FunctionPoint lookLeft()
            {
                goldenSection.LeftInterval = point[Direction] - step * 1000;
                goldenSection.RightInterval = point[Direction];
                return goldenSection.FindMinimum(point);
            };

            FunctionPoint lookRight()
            {
                goldenSection.LeftInterval = point[Direction];
                goldenSection.RightInterval = point[Direction] + step * 1000;
                return goldenSection.FindMinimum(point);
            };

            if (fhl < fhr && fhr > lastValue)
            {
                var p = lookLeft();
                point = p.Point;
            }
            else if (fhr < fhl && fhl > lastValue)
            {
                var p = lookRight();
                point = p.Point;
            }
            else
            {
                var pl = lookLeft();
                var pr = lookRight();
                point = pl.Value < pr.Value ? pl.Point : pr.Point;
            }
            iterations2 = goldenSection.iterations1;
            return point;
        }

        private void throwOnInvalidValue(double currentValue, Vector currentPos, double lastPos, int direction)
        {
            if(double.IsNaN(currentValue) || double.IsInfinity(currentValue))
            {
                throw new ArgumentOutOfRangeException(string.Format("Niepowodzenie podczas minimalizacji kierunkowej, w kierunku {0}, w punkcie: {1}. Wartość poprzedia w kierunku: {2}.", direction, currentPos, lastPos));
            }
        }
    }
}
