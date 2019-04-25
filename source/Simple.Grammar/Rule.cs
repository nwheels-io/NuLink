using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace Simple.Grammar
{
    public abstract class Rule : IChildOfRule, IEnumerable<IChildOfRule>
    {
        private readonly List<SemanticAction> _semanticActions = new List<SemanticAction>();

        protected Rule(string id)
        {
            Id = id;
        }

        public virtual void Add(IChildOfRule child)
        {
            if (child is SemanticAction action)
            {
                _semanticActions.Add(action);
            }
        }

        public void AddRange(params IChildOfRule[] children)
        {
            foreach (var child in children)
            {
                Add(child);
            }
        }
        
        public virtual IEnumerator<IChildOfRule> GetEnumerator()
        {
            return _semanticActions.OfType<IChildOfRule>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public abstract MatchResult Match(ImmutableQueue<Token> input);

        public void InvokeActions(Token token)
        {
            foreach (var action in _semanticActions)
            {
                action.Callback.Invoke(this, token);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}[{this.Id}]";
        }

        public string Id { get; }
        public IReadOnlyList<SemanticAction> SemanticActions => _semanticActions;
    }

    public interface IChildOfRule
    {
    }

    public class SemanticAction : IChildOfRule
    {
        public SemanticAction(Action<Rule, Token> callback)
        {
            Callback = callback;
        }

        public Action<Rule, Token> Callback { get; }        
    }

    public class MatchResult
    {
        public MatchResult(
            bool success, 
            Rule rule, 
            TokenSpan span,
            ImmutableQueue<Token> remainingInput,
            ImmutableList<MatchResult> innerResults = null)
        {
            Success = success;
            Rule = rule;
            Span = span;
            RemainingInput = remainingInput;
            InnerResults = (innerResults ?? ImmutableList<MatchResult>.Empty);
        }

        public MatchResult Include(MatchResult inner)
        {
            return new MatchResult(
                this.Success && inner.Success, 
                this.Rule, 
                this.Span.Concat(inner.Span), 
                inner.RemainingInput,
                this.InnerResults.Add(inner));
        }

        public void InvokeSemanticActions()
        {
            InnerResults.ForEach(result => result.InvokeSemanticActions());
            Rule?.InvokeActions(Span.Tokens.FirstOrDefault());
        }

        public MatchResult Log()
        {
            Console.WriteLine($"> {(Success ? "MATCHED" : "failed ")} > rule [{Rule.Id}] > span [{Span}]");
            return this;
        }
        
        public bool Success { get; }
        public Rule Rule { get; }
        public TokenSpan Span { get; }
        public ImmutableQueue<Token> RemainingInput { get; }
        public ImmutableList<MatchResult> InnerResults { get; }
    }
    
    public class TokenSpan
    {
        public TokenSpan(ImmutableQueue<Token> input, int count = 1)
        {
            Tokens = input.Take(count).ToArray();
        }

        public TokenSpan(ImmutableQueue<Token> before, ImmutableQueue<Token> after)
            : this(before.Take(before.Count() - after.Count()))
        {
        }

        public TokenSpan(IEnumerable<Token> tokens)
        {
            Tokens = tokens.ToArray();
        }

        public TokenSpan Concat(TokenSpan other)
        {
            return new TokenSpan(this.Tokens.Concat(other.Tokens).Distinct()); 
        }

        public override string ToString()
        {
            return string.Join(",", Tokens.Select(t => t.ToString()));
        }

        public IReadOnlyList<Token> Tokens { get; }
    }

    
    public class TokenRule : Rule
    {
        public TokenRule(string id, Type tokenType)
            : base(id)
        {
            TokenType = tokenType;
        }

        public override MatchResult Match(ImmutableQueue<Token> input)
        {
            if (input.IsEmpty)
            {
                return new MatchResult(false, this, null, input).Log();
            }

            var token = input.Peek();
            var match = (token.GetType() == TokenType);

            return new MatchResult(
                match, 
                this, 
                new TokenSpan(input, 1), 
                match ? input.Dequeue() : input
            ).Log();
        }

        public Type TokenType { get; }
    }

    public class TokenRule<T> : TokenRule
        where T : Token
    {
        public TokenRule(string id)
            : base(id, typeof(T))
        {
        }
    }

    public class RepeatedRule : Rule
    {
        public RepeatedRule(string id, int? minTimes, int? maxTimes) : base(id)
        {
            MinTimes = minTimes;
            MaxTimes = maxTimes;
        }

        public override void Add(IChildOfRule child)
        {
            base.Add(child);
            
            if (child is Rule innerRule)
            {
                if (this.Inner == null)
                {
                    this.Inner = innerRule;
                }
                else
                {
                    throw new InvalidOperationException("Cannot add more than one inner rule to RepeatedRule");
                }
            }
        }

        public override IEnumerator<IChildOfRule> GetEnumerator()
        {
            return Enumerable
                .Repeat(Inner, 1)
                .OfType<IChildOfRule>()
                .Concat(SemanticActions)
                .GetEnumerator();
        }

        public override MatchResult Match(ImmutableQueue<Token> input)
        {
            var result = new MatchResult(true, this, new TokenSpan(input, 1), input);
            Int32 timesRepeated = 0;
            Int32 maxRepetitions = MaxTimes.GetValueOrDefault(1000000);
            
            for (timesRepeated = 0; timesRepeated < maxRepetitions; timesRepeated++)
            {
                var nextResult = Inner.Match(result.RemainingInput);

                if (!nextResult.Success)
                {
                    break;
                }

                result = result.Include(nextResult);
            }

            var success = (timesRepeated >= MinTimes.GetValueOrDefault(0));

            return (
                success
                    ? result.Log()
                    : new MatchResult(false, this, new TokenSpan(input, result.RemainingInput), input).Log());
        }

        public Rule Inner { get; private set; }
        public int? MinTimes { get; }
        public int? MaxTimes { get; }
    }

    public class SequenceRule : Rule
    {
        private readonly List<Rule> _elements = new List<Rule>();
        
        public SequenceRule(string id) : base(id)
        {
        }

        public override void Add(IChildOfRule child)
        {
            base.Add(child);
            
            if (child is Rule element)
            {
                _elements.Add(element);
            }
        }

        public override IEnumerator<IChildOfRule> GetEnumerator()
        {
            return 
                _elements
                .OfType<IChildOfRule>()
                .Concat(SemanticActions)
                .GetEnumerator();
        }

        public override MatchResult Match(ImmutableQueue<Token> input)
        {
            var result = new MatchResult(true, this, new TokenSpan(input, 1), input);

            foreach (var element in _elements)
            {
                var elementResult = element.Match(result.RemainingInput);

                if (!elementResult.Success)
                {
                    return new MatchResult(false, this, new TokenSpan(input, result.RemainingInput), input).Log();
                }

                result = result.Include(elementResult);
            }

            return result.Log();
        }

        public IReadOnlyList<Rule> Elements => _elements;
    }

    public class SwitchRule : Rule
    {
        private readonly List<Rule> _cases = new List<Rule>();
        
        public SwitchRule(string id) : base(id)
        {
        }

        public override void Add(IChildOfRule child)
        {
            base.Add(child);
            
            if (child is Rule @case)
            {
                _cases.Add(@case);
            }
        }

        public override IEnumerator<IChildOfRule> GetEnumerator()
        {
            return 
                _cases
                    .OfType<IChildOfRule>()
                    .Concat(SemanticActions)
                    .GetEnumerator();
        }

        public override MatchResult Match(ImmutableQueue<Token> input)
        {
            foreach (var caseRule in _cases)
            {
                var caseResult = caseRule.Match(input);

                if (caseResult.Success)
                {
                    return caseResult;
                }
            }

            return new MatchResult(false, this, new TokenSpan(input, 1), input).Log();
        }

        public IReadOnlyList<Rule> Cases => _cases;
    }
}
