using System;

namespace Simple.CommandLine
{
    public static class RuleFactory
    {
        public static class Child
        {
            public static Rule Rule(string id, MatchPredicate match, SatisfyPredicate satisfy)
            {
                return new Rule(id, match, satisfy);
            }
        }
        
        public static class Match
        {
            public static MatchPredicate Names(params string[] names)
            {
                return new NamesMatchPredicate(names);
            }

            public static MatchPredicate Value(string value)
            {
                return new ValueMatchPredicate(value);
            }

            public static MatchPredicate AnyValue => new AnyValueMatchPredicate();
        }

        public static class Satisfy
        {
            public static SatisfyPredicate Always => new AlwaysSatisfyPredicate();

            public static SatisfyPredicate MatchThis(int exactCount)
            {
                return new MatchCountSatisfyPredicate(exactCount, exactCount);
            }

            public static SatisfyPredicate MatchThis(int? min, int? max)
            {
                return new MatchCountSatisfyPredicate(min, max);
            }

            public static SatisfyPredicate MatchChildren(int exactCount)
            {
                return new ChildrenMatchCountSatisfyPredicate(exactCount, exactCount);
            }

            public static SatisfyPredicate MatchChildren(int? min, int? max)
            {
                return new ChildrenMatchCountSatisfyPredicate(min, max);
            }
        }

        public static class Act
        {
            public static RuleAction Pop(int count)
            {
                return new PopRuleAction(count);
            }
            
            public static RuleAction OnMatch(Action<Rule, Token> callback)
            {
                return new CallbackRuleAction(callback);
            }

            public static RuleAction OnName(Action<string> callback)
            {
                return new CallbackRuleAction((rule, token) => {
                    callback(((NameToken) token).Name);
                });
            }

            public static RuleAction OnValue(Action<string> callback)
            {
                return new CallbackRuleAction((rule, token) => {
                    callback(((ValueToken) token).Value);
                });
            }
        }
    }
}