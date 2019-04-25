using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Simple.CommandLine
{
    public class Rule : IRuleChildObject, IEnumerable<IRuleChildObject>
    {
        private readonly List<Rule> _children;
        private readonly List<RuleAction> _actions;
        private int _matchCount = 0;
        
        public Rule(string id, MatchPredicate match, SatisfyPredicate satisfy, IEnumerable<Rule> children = null)
        {
            Id = id;
            Match = match;
            Satisfy = satisfy;
            
            _children = children?.ToList() ?? new List<Rule>();
            _actions = new List<RuleAction>();
        }

        public IEnumerator<IRuleChildObject> GetEnumerator()
        {
            return _children.OfType<IRuleChildObject>()
                .Concat(_actions.OfType<IRuleChildObject>())
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void IncrementMatchCount()
        {
            _matchCount++;
        }

        public void Add(IRuleChildObject child)
        {
            switch (child)
            {
                case Rule childRule:
                    _children.Add(childRule);
                    break;
                case RuleAction childAction:
                    _actions.Add(childAction);
                    break;
            }
        }

        public void InvokeActions(Stack<Rule> state, Token token)
        {
            foreach (var action in Actions)
            {
                action.Invoke(state, this, token);
            }
        }

        public bool Matches(Token token)
        {
            return Match.Matches(token);
        }
        
        public bool IsSatisfied()
        {
            return Satisfy.IsSatisfied(this);
        }
        
        public string Id { get; }
        public MatchPredicate Match { get; }
        public SatisfyPredicate Satisfy { get; }
        public IReadOnlyList<Rule> Children => _children;
        public IReadOnlyList<RuleAction> Actions => _actions;
        public int MatchCount => _matchCount;
        
    }

    public abstract class MatchPredicate
    {
        public abstract bool Matches(Token token);
    }

    public abstract class SatisfyPredicate
    {
        public abstract bool IsSatisfied(Rule rule);
    }

    public abstract class RuleAction : IRuleChildObject
    { 
        public abstract void Invoke(Stack<Rule> parserState, Rule rule, Token token);
    }

    public interface IRuleChildObject
    {
    }

    public class NamesMatchPredicate : MatchPredicate
    {
        public NamesMatchPredicate(IEnumerable<string> names)
        {
            Names = names.ToArray();
        }

        public override bool Matches(Token token)
        {
            if (token is NameToken nameToken)
            {
                return this.Names.Contains(nameToken.Name);
            }

            return false;
        }
        
        public IReadOnlyList<string> Names { get; }
    }

    public class ValueMatchPredicate : MatchPredicate
    {
        public ValueMatchPredicate(string value)
        {
            Value = value;
        }

        public override bool Matches(Token token)
        {
            return (token as ValueToken)?.Value == this.Value;
        }
        
        public string Value { get; }
    }

    public class AnyValueMatchPredicate : MatchPredicate
    {
        public override bool Matches(Token token)
        {
            return (token is ValueToken);
        }
    }

    public class AlwaysSatisfyPredicate : SatisfyPredicate
    {
        public override bool IsSatisfied(Rule rule)
        {
            return true;
        }
    }

    public class MatchCountSatisfyPredicate : SatisfyPredicate
    {
        public MatchCountSatisfyPredicate(int? min, int? max)
        {
            Min = min;
            Max = max;
        }

        public override bool IsSatisfied(Rule rule)
        {
            if (Min.HasValue && rule.MatchCount < Min.Value)
            {
                return false;
            }

            if (Max.HasValue && rule.MatchCount > Max.Value)
            {
                return false;
            }

            return true;
        }
        
        public int? Min { get; }
        public int? Max { get; }
    }

    public class ChildrenMatchCountSatisfyPredicate : SatisfyPredicate
    {
        public ChildrenMatchCountSatisfyPredicate(int? min, int? max)
        {
            Min = min;
            Max = max;
        }

        public override bool IsSatisfied(Rule rule)
        {
            var count = (
                rule.Children.Count > 0
                    ? rule.Children.Sum(child => child.MatchCount)
                    : 0);

            if (Min.HasValue && count < Min.Value)
            {
                return false;
            }

            if (Max.HasValue && count > Max.Value)
            {
                return false;
            }

            return true;
        }
        
        public int? Min { get; }
        public int? Max { get; }
    }

    public class PopRuleAction : RuleAction
    {
        public PopRuleAction(int count)
        {
            Count = count;
        }

        public override void Invoke(Stack<Rule> parserState, Rule rule, Token token)
        {
            for (int i = 0; i < Count; i++)
            {
                parserState.Pop();
            }
        }
        
        public int Count { get; }
    }

    public class CallbackRuleAction : RuleAction
    {
        public CallbackRuleAction(Action<Rule, Token> callback)
        {
            Callback = callback;
        }

        public override void Invoke(Stack<Rule> parserState, Rule rule, Token token)
        {
            Callback(rule, token);
        }
        
        public Action<Rule, Token> Callback { get; }
    }
}
