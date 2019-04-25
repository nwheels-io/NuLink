using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Security.Principal;

/*

expression     ::= term (BINARY_ADD_OP term)*
term           ::= factor (BINARY_MUL_OP factor)*
factor         ::= primary (UNARY_OP primary)*
primary        ::= CONST | VAR | '(' expression ')'
BINARY_ADD_OP  ::= '+' | '-' 
BINARY_MUL_OP  ::= '*' | '/' | '%' 
UNARY_OP       ::= '-' 

*/

namespace Simple.Grammar.Tests
{
    public static class MathGrammar
    {
        public static Rule DefineMathGrammar(Action<Rule, Token> onOperator, Action<Rule, Token> onOperand)
        {
            var expression = new SequenceRule("EXPRESSION");
            var term = new SequenceRule("TERM");
            var factor = new SequenceRule("FACTOR");
            var primary = new SwitchRule("PRIMARY");
           
            expression.AddRange(
                term,
                new RepeatedRule("binary_add_op_term_rep", minTimes: null, maxTimes: null) {
                    new SequenceRule("binary_add_op_term_seq") {
                        new TokenRule<BinaryAdditiveOpToken>("binary_add_op") {
                            new SemanticAction(onOperator)
                        },
                        term
                    }
                }
            );

            term.AddRange(
                factor,
                new RepeatedRule("binary_mul_op_factor_rep", minTimes: null, maxTimes: null) {
                    new SequenceRule("binary_mul_op_factor_seq") {
                        new TokenRule<BinaryMultiplicativeOpToken>("binary_mul_op") {
                            new SemanticAction(onOperator)
                        },
                        term
                    }
                }
            );

            factor.AddRange(
                primary,
                new RepeatedRule("unary_op_primary_rep", minTimes: null, maxTimes: null) {
                    new SequenceRule("unary_op_primary_seq") {
                        new TokenRule<UnaryOpToken>("unary_op") {
                            new SemanticAction(onOperator)
                        },
                        primary
                    }
                }
            );
            
            primary.AddRange(
                new TokenRule<ConstToken>("CONST") {
                    new SemanticAction(onOperand)
                },
                new TokenRule<VarToken>("VAR") {
                    new SemanticAction(onOperand)
                },
                new SequenceRule("expression_in_parens") {
                    new TokenRule<LeftParenToken>("LPAREN"),
                    expression,
                    new TokenRule<RightParenToken>("RPAREN")
                }
            );
            
            return expression;
        }

        public static ConstToken CONST(string text) => new ConstToken(text);
        public static VarToken VAR(string text) => new VarToken(text);
        public static LeftParenToken LPAREN() => new LeftParenToken();
        public static RightParenToken RPAREN() => new RightParenToken();
        public static UnaryOpToken UNARYOP(char op) => new UnaryOpToken(op);
        public static Token BINARYOP(char op) =>
            ("*/%".Contains($"{op}")
                ? (Token)new BinaryMultiplicativeOpToken(op)
                : (Token)new BinaryAdditiveOpToken(op));
        
        public class VarToken : Token
        {
            public VarToken(string text) : base(text)
            {
            }
        }

        public class ConstToken : Token
        {
            public ConstToken(string text) : base(text)
            {
            }
        }

        public class LeftParenToken : Token
        {
            public LeftParenToken() : base("(")
            {
            }
        }

        public class RightParenToken : Token
        {
            public RightParenToken() : base(")")
            {
            }
        }

        public class UnaryOpToken : Token
        {
            public UnaryOpToken(char op) : base($"{op}")
            {
            }
        }

        public class BinaryAdditiveOpToken : Token
        {
            public BinaryAdditiveOpToken(char op) : base($"{op}")
            {
            }
        }

        public class BinaryMultiplicativeOpToken : Token
        {
            public BinaryMultiplicativeOpToken(char op) : base($"{op}")
            {
            }
        }
    }
}