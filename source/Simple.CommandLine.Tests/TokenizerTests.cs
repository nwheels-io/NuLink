using Shouldly;
using NUnit.Framework;

namespace Simple.CommandLine.Tests
{
    public class TokenizerTests
    {
        [Test]
        public void RecognizesLongNameToken()
        {
            var input = new[] {"--some-name"};
            var output = Tokenizer.Tokenize(input);

            output.ShouldBe(Token.List(
                new NameToken("--some-name")
            ));
        }

        [Test]
        public void RecognizesShortNameToken()
        {
            var input = new[] {"-s"};
            var output = Tokenizer.Tokenize(input);

            output.ShouldBe(Token.List(
                new NameToken("-s")
            ));
        }

        [Test]
        public void RecognizesValueToken()
        {
            var input = new[] {"some-value"};
            var output = Tokenizer.Tokenize(input);

            output.ShouldBe(Token.List(
                new ValueToken("some-value")
            ));
        }

        [Test]
        public void RecognizesBundledShortNameTokens()
        {
            var input = new[] {"-abc"};
            var output = Tokenizer.Tokenize(input);

            output.ShouldBe(Token.List(
                new NameToken("-a"),
                new NameToken("-b"),
                new NameToken("-c")
            ));
        }

        [Test]
        public void RecognizesMultipleMixedTokens()
        {
            var input = new[] {"value0","--name1", "value2","-def","value3"};
            var output = Tokenizer.Tokenize(input);

            output.ShouldBe(Token.List(
                new ValueToken("value0"),
                new NameToken("--name1"),
                new ValueToken("value2"),
                new NameToken("-d"),
                new NameToken("-e"),
                new NameToken("-f"),
                new ValueToken("value3")
            ));
        }
    }
}