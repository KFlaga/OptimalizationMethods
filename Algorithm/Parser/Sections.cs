using System;
using System.Collections.Generic;
using System.Linq;

namespace Qfe.Parser
{
    internal enum SectionType
    {
        Rank,
        Parameters,
        CostFunction,
        Constraints
    }

    internal abstract class Section
    {
        public string Content { get; set; }

        public abstract void Parse();
    }

    internal class RankSection : Section // TODO: rename to dimension
    {
        public uint Rank;

        public override void Parse()
        {
            // Must constain single unsigned integer and semicolon
            if(!Content.Contains(';'))
            {
                throw new ArgumentException("Sekcja '$variables' musi kończyć się średnikiem");
            }
            Input input = new Input(Content);
            string possiblyInt = input.ReadWhile((c) => c != ';');
            if(!uint.TryParse(possiblyInt, out Rank) || Rank == 0)
            {
                throw new ArgumentException("Sekcja '$variables' musi zaiwerać wyłącznie jedną dodatnią liczbę");
            }
        }
    }

    internal class ParametersSection : Section
    {
        public override void Parse()
        {
            // Parameters section is left unchanged (validated during script compilation)
        }
    }

    internal class CostFunctionSection : Section
    {
        public string Function;

        public override void Parse()
        {
            // Must contain non-empty content and end with semicolon
            if (!Content.Contains(';'))
            {
                throw new ArgumentException("Sekcja '$function' musi konczyc się średnikiem");
            }
            Input input = new Input(Content);
            Function = input.ReadWhile((c) => c != ';');
        }
    }

    internal class RawConstraint
    {
        public string Lhs;
        public string Rhs;
        public string Operator;
        public ConstraintType Type;

        public string LhsOnlyVersion
        {
            get
            {
                return string.Format("{0} - ({1})", Lhs, Rhs);
            }
        }

        public string ComparisonWithZero
        {
            get
            {
                return string.Format("{0} - ({1}) {2} 0", Lhs, Rhs, Operator);
            }
        }
    }

    internal class ConstraintsSection : Section
    {
        public List<RawConstraint> Constraints;

        readonly static Dictionary<string, ConstraintType> constraintTypes = new Dictionary<string, ConstraintType>()
        {
            { "<=", ConstraintType.LessEqual },
            { "==", ConstraintType.Equality },
            { ">=", ConstraintType.GreaterEqual }
        };

        readonly static string[] separators = constraintTypes.Keys.ToArray();

        public override void Parse()
        {
            Constraints = new List<RawConstraint>();
            
            Input input = new Input(Content);
            while(input.Contains(';')) // Each constraints terminates with semicolon
            {
                string constraintContent = input.ReadWhile((c) => c != ';');
                
                var leftRightSide = constraintContent.Split(separators, 3, StringSplitOptions.None);
                if(leftRightSide.Length != 2)
                {
                    throw new ArgumentException("Każde ograniczenie musi mieć dokładnie jeden operator porównania");
                }

                string op = constraintContent.Substring(leftRightSide[0].Length, 2);
                Constraints.Add(new RawConstraint()
                {
                    Lhs = leftRightSide[0],
                    Rhs = leftRightSide[1],
                    Operator = op,
                    Type = constraintTypes[op]
                });
                
                input.MoveNext();
            }
        }
    }

    internal class AllSections
    {
        public RankSection RankSection { get; set; }
        public ParametersSection ParametersSection { get; set; }
        public CostFunctionSection CostFunctionSection { get; set; }
        public ConstraintsSection ConstraintsSection { get; set; }
    }

    internal class SectionsParser
    {
        public const char sectionStart = '$';
        public readonly static Dictionary<string, Func<string, Section>> sectionTypes = new Dictionary<string, Func<string, Section>>()
        {
            { "$variables:", (c) => new RankSection() { Content = c } },
            { "$parameters:", (c) => new ParametersSection() { Content = c } },
            { "$function:", (c) => new CostFunctionSection() { Content = c } },
            { "$constraints:", (c) => new ConstraintsSection() { Content = c } }
        };

        public static AllSections ParseSections(string c)
        {
            List<Section> sections = Split(c);
            EnforceSectionsAreUnique(sections);
            foreach(var s in sections)
            {
                s.Parse();
            }

            try
            {
                AllSections result = new AllSections()
                {
                    RankSection = sections.Single((s) => s.GetType() == typeof(RankSection)) as RankSection,
                    ParametersSection = sections.SingleOrDefault((s) => s.GetType() == typeof(ParametersSection)) as ParametersSection,
                    CostFunctionSection = sections.Single((s) => s.GetType() == typeof(CostFunctionSection)) as CostFunctionSection,
                    ConstraintsSection = sections.SingleOrDefault((s) => s.GetType() == typeof(ConstraintsSection)) as ConstraintsSection
                };

                return result;
            }
            catch(InvalidOperationException)
            {
                throw new ArgumentException("Nie znaleziono wymaganych sekcji: '$variables', '$fuction'");
            }
        }

        public static List<Section> Split(string c)
        {
            List<Section> sections = new List<Section>();
            Input input = new Input(c);

            input.SkipWhile((x) => x != sectionStart);
            while (!input.IsEnd)
            {
                try
                {
                    var s = sectionTypes.First((k) => input.StartsWith(k.Key));
                    input.SkipN(s.Key.Length);
                    sections.Add(s.Value(input.ReadWhile((x) => x != sectionStart)));
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentException("Wejście zawiera niewspierane sekcje");
                }
            }
            return sections;
        }

        public static void EnforceSectionsAreUnique(List<Section> sections)
        {
            foreach(var s in sections)
            {
                if(sections.Count((x) => x.GetType() == s.GetType()) > 1)
                {
                    throw new ArgumentException("Wejście zawiera zduplikowane sekcje");
                }
            }
        }
    }
}
