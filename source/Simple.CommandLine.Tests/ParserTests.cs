using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Shouldly;
using static Simple.CommandLine.RuleFactory;

namespace Simple.CommandLine.Tests
{
    public class ParserTests
    {
        [Test]
        public void PositionalArguments()
        {
            string arg1 = null, arg2 = null, arg3 = null;

            var grammar = new Rule("root", Match.AnyValue, Satisfy.Always) {
                new Rule("arg-1", Match.AnyValue, Satisfy.MatchThis(1)) {
                    Act.OnValue(value => arg1 = value),
                    new Rule("arg-2", Match.AnyValue, Satisfy.MatchThis(1)) {
                        Act.OnValue(value => arg2 = value),
                        new Rule("arg-3", Match.AnyValue, Satisfy.MatchThis(1)) {
                            Act.OnValue(value => arg3 = value),
                        }
                    }
                }
            };
            
            var parser = new Parser(grammar);
            var args = new[] {"aaa", "bbb", "ccc"};

            parser.Parse(Tokenizer.Tokenize(args));

            arg1.ShouldBe("aaa");
            arg2.ShouldBe("bbb");
            arg3.ShouldBe("ccc");
        }

        [Test]
        public void OptionArguments()
        {
            string firstOption = null, secondOption = null;

            var grammar = new Rule("root", Match.AnyValue, Satisfy.Always) {
                new Rule("option-first", Match.Names("--first"), Satisfy.MatchChildren(1)) {
                    new Rule("option-first-value", Match.AnyValue, Satisfy.Always) {
                        Act.OnValue(value => firstOption = value),
                        Act.Pop(2)
                    }
                },
                new Rule("option-second", Match.Names("--second"), Satisfy.MatchChildren(1)) {
                    new Rule("option-second-value", Match.AnyValue, Satisfy.Always) {
                        Act.OnValue(value => secondOption = value),
                        Act.Pop(2)
                    }
                }
            };
            
            var parser = new Parser(grammar);
            var args = new[] {"--first", "aaa", "--second", "bbb"};

            parser.Parse(Tokenizer.Tokenize(args));

            firstOption.ShouldBe("aaa");
            secondOption.ShouldBe("bbb");
        }

        [Test]
        public void GlobalOptionsMixedWithPositionalArguments()
        {
            string firstOption = null, secondOption = null;
            string arg1 = null, arg2 = null;

            var grammar = new Rule("root", Match.AnyValue, Satisfy.Always) {
                new Rule("option-first", Match.Names("--first"), Satisfy.MatchChildren(1)) {
                    new Rule("option-first-value", Match.AnyValue, Satisfy.Always) {
                        Act.OnValue(value => firstOption = value),
                        Act.Pop(2)
                    }
                },
                new Rule("option-second", Match.Names("--second"), Satisfy.MatchChildren(1)) {
                    new Rule("option-second-value", Match.AnyValue, Satisfy.Always) {
                        Act.OnValue(value => secondOption = value),
                        Act.Pop(2)
                    }
                },
                new Rule("arg-1", Match.AnyValue, Satisfy.MatchThis(1)) {
                    Act.OnValue(value => arg1 = value),
                    new Rule("arg-2", Match.AnyValue, Satisfy.MatchThis(1)) {
                        Act.OnValue(value => arg2 = value),
                    }
                }
            };
            
            var parser = new Parser(grammar);
            var args = new[] {"aaa", "--first", "fff", "bbb", "--second", "sss"};

            parser.Parse(Tokenizer.Tokenize(args));

            firstOption.ShouldBe("fff");
            secondOption.ShouldBe("sss");
            arg1.ShouldBe("aaa");
            arg2.ShouldBe("bbb");
        }
    }
}