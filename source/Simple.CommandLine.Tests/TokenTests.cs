using NUnit.Framework;
using Shouldly;

namespace Simple.CommandLine.Tests
{
    public class TokenTests
    {
        [Test]
        public void TestNameTokenEquals()
        {
            var token = new NameToken("abc");
            
            token.Equals(new NameToken("abc")).ShouldBe(true);
            token.Equals(new NameToken("def")).ShouldBe(false);
            token.Equals(new ValueToken("abc")).ShouldBe(false);
        }

        [Test]
        public void TestValueTokenEquals()
        {
            var token = new ValueToken("abc");
            
            token.Equals(new ValueToken("abc")).ShouldBe(true);
            token.Equals(new ValueToken("def")).ShouldBe(false);
            token.Equals(new NameToken("abc")).ShouldBe(false);
        }
    }
}