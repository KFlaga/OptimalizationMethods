using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Qfe
{
    public static class NumericalDerivative
    {
        public static double First(Func<Vector, double> func, Vector point, int direction, double h = 1e-4)
        {
            // f(x+h) - f(x-h) / 2h 
            point[direction] += h;
            double b = func(point);
            point[direction] -= 2.0 * h;
            double a = func(point);
            point[direction] += h;
            return (b - a) / (2 * h);
        }

        public static double FirstLeft(Func<Vector, double> func, Vector point, int direction, double h = 1e-4)
        {
            // f(x) - f(x-h) / 2
            double b = func(point);
            point[direction] -= h;
            double a = func(point);
            point[direction] += h;
            return (b - a) / h;
        }

        public static double FirstRight(Func<Vector, double> func, Vector point, int direction, double h = 1e-4)
        {
            // f(x+h) - f(x) / h
            double a = func(point);
            point[direction] += h;
            double b = func(point);
            point[direction] -= h;
            return (b - a) / h;
        }

        public static double Second(Func<Vector, double> func, Vector point, int direction, double h = 1e-4)
        {
            // f(x+h) - 2 f(x) + f(x-h) / h^2
            point[direction] += h;
            double c = func(point);
            point[direction] -= h;
            double b = func(point);
            point[direction] -= h;
            double a = func(point);
            point[direction] += h;
            return (c - 2.0 * b + a) / (h * h);
        }

        public static double fhLeft(Func<Vector, double> func, Vector point, int direction, double h = 1e-4)
        {
            // f(x-h)
            point[direction] -= h;
            double fx = func(point);
            point[direction] += h;
            return fx;
        }

        public static double fhRight(Func<Vector, double> func, Vector point, int direction, double h = 1e-4)
        {
            // f(x+h)
            point[direction] += h;
            double fx = func(point);
            point[direction] -= h;
            return fx;
        }
    }
}
