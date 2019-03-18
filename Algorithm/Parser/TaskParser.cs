using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;

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
                (int)sections.RankSection.Rank,
                compileCostFunction(sections),
                compileConstraints(sections),
                input
            );
        }

        public static void Initialize()
        {
            // Compile empty script to pre-load required assemblies
            CSharpScript.Create<Func<double[], double>>("0", ScriptOptions.Default, typeof(SpecialFunctions)).Compile();
        }

        internal CostFunction compileCostFunction(AllSections sections)
        {
            var func = compileFunction(prepareFunctionCode(sections, sections.CostFunctionSection.Function));
            return new CostFunction(func.Item1, func.Item2);
        }

        internal List<Constraint> compileConstraints(AllSections sections)
        {
            if (sections.ConstraintsSection != null)
            {
                return sections.ConstraintsSection.Constraints.Select((s) =>
                {
                    var func = compileFunction(prepareFunctionCode(sections, s.LhsOnlyVersion));
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
            string code = string.Format("(System.Func<double[], double>)( (x) => {0} )", funcCode);
            if (sections.ParametersSection != null)
            {
                code = string.Format("{0}\n{1}", sections.ParametersSection.Content, code);
            }

            return code;
        }

        private Tuple<Func<double[], double>, object> compileFunction(string code)
        {
            var script = CSharpScript.Create<Func<double[], double>>(code, ScriptOptions.Default, typeof(SpecialFunctions));
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

            return new Tuple<Func<double[], double>, object>(task.Result.ReturnValue, script);
        }
    }
}
