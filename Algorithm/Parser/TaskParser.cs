using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Qfe.Parser
{
    // Input:
    //
    // $variables: 2;
    // 
    // $parameters:
    //   double a = 1.0;
    //   double[] b = new double[] { 2.1, 3.5 };
    //
    // $function:
    //   pow2(x[0] + 1) + b[0] * x[0] + b[1] * x[1] + a;
    // 
    // $constraints:
    //   x[0] + x[1] - 200 <= 0;
    //   a * pow2(x[1]) + 500 <= 0;
    //   x[0] * x[1] - 600.0 == 0;
    //
    // How to prepare scripts:
    //   copy contents of "parameters" on the beginning of each script
    //   let contents of function be F(x), then each script ends with line: (System.Func<double[], double>)( (x) => F(x) )
    //   let contents of constraint be "C(x) op 0" (op is '=', '>' etc), then each script ends with line: (System.Func<double[], double>)( (x) => C(x) )

    public class SpecialFunctions
    {
        public double sin(double x) => Math.Sin(x);
        public double cos(double x) => Math.Cos(x);
        public double tan(double x) => Math.Tan(x);

        public double log(double x, double b) => Math.Log(x, b);
        public double ln(double x) => Math.Log(x);
        public double log2(double x) => Math.Log(x, 2.0);
        public double log10(double x) => Math.Log10(x);

        public double pow(double x, double y) => Math.Pow(x, y);
        public double pow2(double x) => x * x;
        public double pow3(double x) => x * x * x;
        public double pow4(double x) => x * x * x * x;
        public double pow5(double x) => x * x * x * x * x;

        public double exp(double x) => Math.Exp(x);
        public double abs(double x) => Math.Abs(x);
    }

    public class FunctionCompilationFailure : Exception
    {
        public IEnumerable<Diagnostic> Diagnostics { get; set; }
        public string Code { get; set; }
    }

    public class TaskParser
    {
        public Task Parse(string input)
        {
            AllSections sections = SectionsParser.ParseSections(input);
            return new Task
            (
                (int)sections.DimensionSection.Dim,
                compileCostFunction(sections),
                compileConstraints(sections),
                input
            );
        }

        private static ScriptOptions options = null;
        private static ScriptOptions getOptions()
        {
            if(options == null)
            {
                var mathNetNumerics = System.Reflection.Assembly.GetAssembly(typeof(Vector));
                options = ScriptOptions.Default
                    .AddReferences(mathNetNumerics)
                    .AddImports("MathNet.Numerics.LinearAlgebra.Double");
            }
            return options;
        }

        public static void Initialize()
        {
            // Compile empty script to pre-load required assemblies
            CSharpScript.Create<Func<Vector, double>>("0", getOptions(), typeof(SpecialFunctions)).Compile();
        }

        internal CostFunction compileCostFunction(AllSections sections)
        {
            var func = compileFunction(prepareFunctionCode(sections, sections.CostFunctionSection.Function));
            testFunction(func.Item1, sections.DimensionSection.Dim);
            return new CostFunction(func.Item1, func.Item2);
        }

        internal List<Constraint> compileConstraints(AllSections sections)
        {
            if (sections.ConstraintsSection != null)
            {
                return sections.ConstraintsSection.Constraints.Select((s) =>
                {
                    var func = compileFunction(prepareFunctionCode(sections, s.LhsOnlyVersion));
                    testFunction(func.Item1, sections.DimensionSection.Dim);
                    return new Constraint(func.Item1, s.Type, func.Item2);
                }).ToList();
            }
            else
            {
                return new List<Constraint>();
            }
        }

        private string prepareFunctionCode(AllSections sections, string funcCode)
        {
            string code = string.Format("(System.Func<Vector, double>)( (x) => {0} )", funcCode);
            if (sections.ParametersSection != null)
            {
                code = string.Format("{0}\n{1}", sections.ParametersSection.Content, code);
            }

            return code;
        }

        private Tuple<Func<Vector, double>, object> compileFunction(string code)
        {
            var script = CSharpScript.Create<Func<Vector, double>>(code, getOptions(), typeof(SpecialFunctions));
            var diags = script.Compile();

            if (diags.Any((diag) => diag.Severity == DiagnosticSeverity.Error))
            {
                throw new FunctionCompilationFailure()
                {
                    Diagnostics = diags,
                    Code = code
                };
            }

            var task = script.RunAsync(new SpecialFunctions());
            task.Wait();

            return new Tuple<Func<Vector, double>, object>(task.Result.ReturnValue, script);
        }

        private void testFunction(Func<Vector, double> func, uint rank)
        {
            // Try to execute each compiled function to check for some errors - it should work with all-zeros input
            try
            {
                func((DenseVector)DenseVector.Build.Dense((int)rank, 0.0));
            }
            catch (DivideByZeroException) { } // Those two may be thrown becouse zeros are invalid input after all
            catch (OverflowException) { }
            catch(IndexOutOfRangeException ex) // Those are called almost surely becouse parameter is a table
            {
                throw new IndexOutOfRangeException("Odwołanie do parametru o indeksie większym niż zadeklarowano", ex);
            }
            catch (ArgumentOutOfRangeException ex) // Those are called almost surely becouse rank is too small
            {
                throw new IndexOutOfRangeException("Odwołanie do zmiennej o indeksie większym niż zadeklarowano (x[i] -> max i == " + rank.ToString() + ")", ex);
            }
        }
    }
}
