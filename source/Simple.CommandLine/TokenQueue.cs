using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.CommandLine
{
    public class TokenQueue
    {
        private readonly Token[] _tokens;
        private int _index = 0;

        public TokenQueue(IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToArray();
        }

        public Token TryTake()
        {
            return (_index < _tokens.Length ? _tokens[_index++] : null);
        }

        public void Untake()
        {
            if (_index > 0)
            {
                _index--;
            }
            else
            {
                throw new InvalidOperationException("No tokens were taken");
            }
        }

        public bool Empty => _index >= _tokens.Length;
    }
}