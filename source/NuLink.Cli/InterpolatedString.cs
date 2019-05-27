using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace NuLink.Cli
{
    public class InterpolatedString
    {
        public InterpolatedString(Expression<Func<string>> lambda)
        {
            if (TryParseLambda(lambda, out var formatString, out var formatParts, out var formatArgs))
            {
                this.FormatString = formatString;
                this.FormatParts = formatParts;
                this.FormatArgs = formatArgs;
            }
            else
            {
                this.FormatString = lambda.Compile()();
                this.FormatParts = new[] { FormatString };
                this.FormatArgs = new object[0];
            }
        }

        public string FormatString { get; }
        public IReadOnlyList<string> FormatParts { get; }
        public IReadOnlyList<object> FormatArgs { get; }

        private static bool TryParseLambda(
            Expression<Func<string>> lambda,
            out string formatString,
            out string[] formatParts,
            out object[] formatArgs)
        {
            if (!(lambda.Body is MethodCallExpression methodCall) || !IsStringFormatMethod(methodCall.Method))
            {
                formatString = null;
                formatParts = null;
                formatArgs = null;
                return false;
            }

            int formatArgCount;
            int formatArgSkip;
            ReadOnlyCollection<Expression> formatArgExpressions;
            
            if (methodCall.Arguments.Count == 2 && methodCall.Arguments[1] is NewArrayExpression newArray)
            {
                formatArgCount = newArray.Expressions.Count;
                formatArgSkip = 0;
                formatArgExpressions = newArray.Expressions;
            }
            else
            {
                formatArgCount = methodCall.Arguments.Count - 1;
                formatArgSkip = 1;
                formatArgExpressions = methodCall.Arguments;
            }
                
            formatString = (string)EvaluateExpression(methodCall.Arguments[0]);
            formatParts = ParseFormatParts(formatString, formatArgCount);
            formatArgs = new object[formatArgCount];

            for (int i = 0; i < formatArgCount; i++)
            {
                formatArgs[i] = EvaluateExpression(formatArgExpressions[formatArgSkip + i]);
            }

            return true;
        }

        private static bool IsStringFormatMethod(MethodInfo method)
        {
            return (method.DeclaringType == typeof(string) && method.Name == nameof(string.Format));
        }
        
        private static object EvaluateExpression(Expression expr)
        {
            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(expr);
            Func<object> compiled = lambda.Compile();
            var value = compiled();
            return value;
        }

        private static string[] ParseFormatParts(string formatString, int argCount)
        {
            var parts = new string[argCount * 2 + 1];

            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = string.Empty;
            }
            
            var parserData = new FormatParserData(formatString, parts);

            for (int i = 0; i < formatString.Length; i++)
            {
                FormatParser.HandleInputChar(parserData, formatString[i]);
            }
            
            return parts;
        }

        private enum FormatParserState
        {
            AtLiteral = 0,
            AteBackslash = 1,
            AteLeftBrace = 2,
            AtArgIndexDigit = 3,
            AtArgFormatSpec = 4,
            AteRightBrace = 5
        }

        private class FormatParserData
        {
            public FormatParserData(string input, string[] parts)
            {
                this.Input = input;
                this.Parts = parts;
                CurrentPartIndex = 0;
                State = FormatParserState.AtLiteral;
            }

            public void AppendToPart(char c)
            {
                Parts[CurrentPartIndex] += c;
            }
            
            public string Input { get; }
            public string[] Parts { get; }
            public int CurrentPartIndex { get; set; }
            public FormatParserState State { get; set; }
        }

        private static class FormatParser
        {
            private static readonly IReadOnlyDictionary<FormatParserState, Action<FormatParserData, char>> StateMachine =
                new Dictionary<FormatParserState, Action<FormatParserData, char>> {
                    [FormatParserState.AtLiteral] = (data, c) => {
                        switch (c)
                        {
                            case '\\':
                                data.State = FormatParserState.AteBackslash;
                                break;
                            case '{':
                                data.State = FormatParserState.AteLeftBrace;
                                break;
                            case '}':
                                data.State = FormatParserState.AteRightBrace;
                                break;
                            default:
                                data.AppendToPart(c);
                                break;
                        }
                    },
                    [FormatParserState.AteBackslash] = (data, c) => {
                        data.AppendToPart(GetEscapedChar(c));
                        data.State = FormatParserState.AtLiteral;
                    },
                    [FormatParserState.AteLeftBrace] = (data, c) => {
                        switch (c)
                        {
                            case '{':
                                data.AppendToPart('{');
                                data.State = FormatParserState.AtLiteral;
                                break;
                            default:
                                data.CurrentPartIndex++;
                                data.State = FormatParserState.AtArgIndexDigit;
                                break;
                        }                        
                    },
                    [FormatParserState.AtArgIndexDigit] = (data, c) => {
                        switch (c)
                        {
                            case ':':
                                data.State = FormatParserState.AtArgFormatSpec;
                                break;
                            case '}':
                                data.CurrentPartIndex++;
                                data.State = FormatParserState.AtLiteral;
                                break;
                        }
                    },
                    [FormatParserState.AtArgFormatSpec] = (data, c) => {
                        switch (c)
                        {
                            case '}':
                                data.CurrentPartIndex++;
                                data.State = FormatParserState.AtLiteral;
                                break;
                            default:
                                data.AppendToPart(c);
                                break;
                        }
                    },
                    [FormatParserState.AteRightBrace] = (data, c) => {
                        switch (c)
                        {
                            case '}':
                                data.AppendToPart('}');
                                break;
                        }
                        data.State = FormatParserState.AtLiteral;
                    },
                };

            private static char GetEscapedChar(char c)
            {
                switch (c)
                {
                    case 'n': return '\n';
                    case 'r': return '\r';
                    case 't': return '\t';
                    default: return c;
                }
            }

            public static void HandleInputChar(FormatParserData data, char input)
            {
                StateMachine[data.State](data, input);
            }
        }
    }
}
