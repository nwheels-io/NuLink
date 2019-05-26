using NuLink.Cli;
using NUnit.Framework;
using Shouldly;

namespace NuLink.Tests.Unit
{
    public class InterpolatedStringTests
    {
        [Test]
        public void CanParse()
        {
            var num = 123;
            var str = "abc";
            var parsed = new InterpolatedString(() => $"N={num}, S={str}.");
            
            parsed.FormatString.ShouldBe("N={0}, S={1}.");
            parsed.FormatParts.ShouldBe(new[] { "N=", "", ", S=", "", "." });
            parsed.FormatArgs.ShouldBe(new object[] { 123, "abc" });
        }

        [Test]
        public void CanParseFormatSpecs()
        {
            var parsed = new InterpolatedString(() => $"N={123:#,###}, S={"abc":-15}.");
            
            parsed.FormatString.ShouldBe("N={0:#,###}, S={1:-15}.");
            parsed.FormatParts.ShouldBe(new[] { "N=", "#,###", ", S=", "-15", "." });
            parsed.FormatArgs.ShouldBe(new object[] { 123, "abc" });
        }

        [Test]
        public void CanEvaluateArgumentExpressions()
        {
            var num = 123;
            var str = "abc";
            var parsed = new InterpolatedString(() => $"N={num * 2}, S={str.ToUpper()}.");
            
            parsed.FormatArgs.ShouldBe(new object[] { 246, "ABC" });
        }

        [Test]
        public void CanParseJustLiteral()
        {
            var parsed = new InterpolatedString(() => $"just-text");
            
            parsed.FormatString.ShouldBe("just-text");
            parsed.FormatParts.ShouldBe(new[] { "just-text" });
            parsed.FormatArgs.ShouldBe(new object[0]);
        }

        [Test]
        public void CanParseJustSinglePlaceholder()
        {
            var parsed = new InterpolatedString(() => $"{123}");
            
            parsed.FormatString.ShouldBe("{0}");
            parsed.FormatParts.ShouldBe(new[] { "", "", "" });
            parsed.FormatArgs.ShouldBe(new object[] { 123 });
        }

        [Test]
        public void CanParseJustPlaceholders()
        {
            var parsed = new InterpolatedString(() => $"{123}{"abc"}{456}");
            
            parsed.FormatString.ShouldBe("{0}{1}{2}");
            parsed.FormatParts.ShouldBe(new[] { "", "", "", "", "", "", "" });
            parsed.FormatArgs.ShouldBe(new object[] { 123, "abc", 456 });
        }

        [Test]
        public void CanParseMoreThanThreePlaceholders()
        {
            var parsed = new InterpolatedString(() => $"aa{123}bb{"abc"}cc{456}dd{"def"}ee");
            
            parsed.FormatString.ShouldBe("aa{0}bb{1}cc{2}dd{3}ee");
            parsed.FormatParts.ShouldBe(new[] { "aa", "", "bb", "", "cc", "", "dd", "", "ee" });
            parsed.FormatArgs.ShouldBe(new object[] { 123, "abc", 456, "def" });
        }
    }
}