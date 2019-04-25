using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Simple.CommandLine
{
    public class Parser
    {
        public Parser(Rule grammar)
        {
            Grammar = grammar;
        }

        public void Parse(IEnumerable<Token> input)
        {
            var state = new Stack<Rule>(new[] { Grammar });
            var tokenQueue = new TokenQueue(input);
            Token token;

            while ((token = tokenQueue.TryTake()) != null)
            {
                if (state.Count == 0)
                {
                    throw CreateParserError();
                }
                
                var currentRule = state.Peek();
                var nextRule = currentRule.Children.FirstOrDefault(r => r.Matches(token));

                if (nextRule != null)
                {
                    state.Push(nextRule);
                    nextRule.IncrementMatchCount();
                    nextRule.InvokeActions(state, token);
                }
                else if (currentRule.IsSatisfied())
                {
                    tokenQueue.Untake();
                    state.Pop();
                }
                else
                {
                    throw CreateParserError();
                }
            }

            if (state.Count > 0 && !state.Peek().IsSatisfied())
            {
                throw CreateParserError(endOfInput: true);
            }
            
            Exception CreateParserError(bool endOfInput = false)
            {
                var stateString = string.Join("->", state.Reverse().Select(r => r.Id));
                
                return new Exception(
                    endOfInput 
                        ? $"Parser failed: end of input, state [{stateString}]" 
                        : $"Parser failed: state [{stateString}], token [{token}]");
            }
        }
        
        public Rule Grammar { get; }
    }
}