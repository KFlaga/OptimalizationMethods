using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Algorithm;
using Algorithm.Parser;

namespace AlgorithmTests
{
    [TestClass]
    public class SectionSplitTests
    {
        [TestMethod]
        public void RankSectionOnly()
        {
            string input = "$variables: 3;";

            var sections = SectionsParser.Split(input);

            Assert.AreEqual(1, sections.Count);
            Assert.IsInstanceOfType(sections[0], typeof(RankSection));
            Assert.AreEqual("3;", sections[0].Content.Trim());
        }

        [TestMethod]
        public void ParametersSectionOnly()
        {
            string input = "$parameters:\na = 10; b = 20;\n";

            var sections = SectionsParser.Split(input);

            Assert.AreEqual(1, sections.Count);
            Assert.IsInstanceOfType(sections[0], typeof(ParametersSection));
            Assert.AreEqual("a = 10; b = 20;", sections[0].Content.Trim());
        }

        [TestMethod]
        public void CostFunctionSectionOnly()
        {
            string input = "$function:\na * x[0] + b;";

            var sections = SectionsParser.Split(input);

            Assert.AreEqual(1, sections.Count);
            Assert.IsInstanceOfType(sections[0], typeof(CostFunctionSection));
            Assert.AreEqual("a * x[0] + b;", sections[0].Content.Trim());
        }

        [TestMethod]
        public void ConstraintsSectionOnly()
        {
            string input = "$constraints:\nx[0] <= 100;\nx[1] >= 200;\n";

            var sections = SectionsParser.Split(input);

            Assert.AreEqual(1, sections.Count);
            Assert.IsInstanceOfType(sections[0], typeof(ConstraintsSection));
            Assert.AreEqual("x[0] <= 100;\nx[1] >= 200;", sections[0].Content.Trim());
        }

        [TestMethod]
        public void MultipleSections()
        {
            string input = @"$variables: 2;
                             $parameters: a = 10;
                             $function: x[0] + a;";

            var sections = SectionsParser.Split(input);

            Assert.AreEqual(3, sections.Count);
            Assert.IsInstanceOfType(sections[0], typeof(RankSection));
            Assert.IsInstanceOfType(sections[1], typeof(ParametersSection));
            Assert.IsInstanceOfType(sections[2], typeof(CostFunctionSection));
        }

        [TestMethod]
        public void InvalidSection()
        {
            string input = @"$variables: 2;
                             $unknown: a = 10;
                             $function: x[0] + a;";

            TestUtils.ExpectThrow(() => SectionsParser.Split(input));
        }

        [TestMethod]
        public void NonUniqueSections()
        {
            string input = @"$variables: 2;
                             $parameters: a = 10;
                             $variables: 3;";

            var sections = SectionsParser.Split(input);
            Assert.AreEqual(3, sections.Count);
            TestUtils.ExpectThrow(() => SectionsParser.EnforceSectionsAreUnique(sections));
        }
    }

    [TestClass]
    public class SectionParsersTests
    {
        [TestMethod]
        public void ParseRankSection()
        {
            RankSection section = new RankSection();

            section.Content = "1;";
            section.Parse();
            Assert.AreEqual(1u, section.Rank);

            section.Content = " 2 ; ";
            section.Parse();
            Assert.AreEqual(2u, section.Rank);

            section.Content = "0  ; ";
            section.Parse();
            Assert.AreEqual(0u, section.Rank);

            section.Content = "1";
            TestUtils.ExpectThrow(() => section.Parse());

            section.Content = "-1;";
            TestUtils.ExpectThrow(() => section.Parse());

            section.Content = "a;";
            TestUtils.ExpectThrow(() => section.Parse());
        }

        [TestMethod]
        public void ParseCostFunctionSection()
        {
            CostFunctionSection section = new CostFunctionSection();

            section.Content = "1;";
            section.Parse();
            Assert.AreEqual("1", section.Function);

            section.Content = "2 * x[0];";
            section.Parse();
            Assert.AreEqual("2 * x[0]", section.Function);

            section.Content = " \n  2   *   x[0] + a  ;   ";
            section.Parse(); // Let ws handling be unspecified -> its good enough that it passed

            section.Content = "1";
            TestUtils.ExpectThrow(() => section.Parse());
        }

        [TestMethod]
        public void ParseConstraintsSection_singleConstraint()
        {
            ConstraintsSection section = new ConstraintsSection();

            section.Content = "x[0] <= 0;";
            section.Parse();
            Assert.AreEqual(1, section.Constraints.Count);
            Assert.AreEqual("x[0]", section.Constraints[0].Lhs.Trim());
            Assert.AreEqual("0", section.Constraints[0].Rhs.Trim());
            Assert.AreEqual(ConstraintType.LessEqual, section.Constraints[0].Type);

            section.Content = "x[0] + x[1] + a == c + d;";
            section.Parse();
            Assert.AreEqual(1, section.Constraints.Count);
            Assert.AreEqual("x[0] + x[1] + a", section.Constraints[0].Lhs.Trim());
            Assert.AreEqual("c + d", section.Constraints[0].Rhs.Trim());
            Assert.AreEqual(ConstraintType.Equality, section.Constraints[0].Type);
        }

        [TestMethod]
        public void ParseConstraintsSection_multipleConstraints()
        {
            ConstraintsSection section = new ConstraintsSection();

            section.Content = "x[0] <= 0;\nx[1] >= 1;";
            section.Parse();
            Assert.AreEqual(2, section.Constraints.Count);
            Assert.AreEqual("x[0]", section.Constraints[0].Lhs.Trim());
            Assert.AreEqual("0", section.Constraints[0].Rhs.Trim());
            Assert.AreEqual(ConstraintType.LessEqual, section.Constraints[0].Type);
            Assert.AreEqual("x[1]", section.Constraints[1].Lhs.Trim());
            Assert.AreEqual("1", section.Constraints[1].Rhs.Trim());
            Assert.AreEqual(ConstraintType.GreaterEqual, section.Constraints[1].Type);
        }

        [TestMethod]
        public void ParseConstraintsSection_invalid()
        {
            ConstraintsSection section = new ConstraintsSection();

            section.Content = "x[0] <= 0";
            TestUtils.ExpectThrow(() => section.Parse());

            section.Content = "x[0] <= 0; x[1] <= 0";
            TestUtils.ExpectThrow(() => section.Parse());

            section.Content = "x[0] 0;";
            TestUtils.ExpectThrow(() => section.Parse());

            section.Content = "x[0] <= 0; ; x[1] <= 0;";
            TestUtils.ExpectThrow(() => section.Parse());
        }
    }
}
