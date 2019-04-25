using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using static Simple.Grammar.Tests.MathGrammar;

namespace Simple.Grammar.Tests
{
    public class MathGrammarTests
    {
        [Test]
        public void AddTwoNumbers()
        {
            var output = new List<string>();
            var success = ParseMathExpression(output,
                CONST("1") //, BINARYOP('+'), CONST("2")//, BINARYOP('*'), CONST("3")
            );
            
            success.ShouldBe(true);
            output.ShouldBe(new[] {
                "BinaryAdditiveOpToken[+]", 
                "ConstToken[1]",
                "ConstToken[2]"
            });
        }
        
        private bool ParseMathExpression(
            List<string> output,
            params Token[] tokens)
        {
            var input = tokens.ToImmutableQueue();
            var grammar = DefineMathGrammar(
                onOperand: (rule, token) => output.Add($"{rule.Id}->{token}"),
                onOperator: (rule, token) => output.Add($"{rule.Id}->{token}")
            );

            var result = grammar.Match(input);

            if (result.Success)
            {
                result.InvokeSemanticActions();
            }
            
            return result.Success;
        }
    }
}