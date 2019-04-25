using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace Simple.CommandLine
{
    public static class Tokenizer
    {
        public static IEnumerable<Token> Tokenize(string[] args)
        {
            var result = new List<Token>();
            
            foreach (var arg in args)
            {
                ParseArg(arg);
            }

            return result;
            
            void ParseArg(string arg)
            {
                if (arg.StartsWith("--"))
                {
                    result.Add(SingleNameToken(arg));
                }
                else if (arg.StartsWith("-"))
                {
                    result.AddRange(SingleOrBundledNameTokens(arg));
                }
                else
                {
                    result.Add(new ValueToken(arg));
                }
            }
            
            NameToken SingleNameToken(string arg) => new NameToken(arg);

            IEnumerable<NameToken> SingleOrBundledNameTokens(string arg)
            {
                return arg.Skip(1).Select(c => new NameToken($"-{c}"));
            }
        }
    }
}