using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace Simple.Grammar
{
    public static class NetstandardExtensions
    {
        public static T PeekOrDefault<T>(this ImmutableQueue<T> queue)
        {
            return (
                !queue.IsEmpty
                    ? queue.Peek()
                    : default(T));
        }

        public static ImmutableQueue<Token> ToImmutableQueue(this IEnumerable<Token> input)
        {
            return ImmutableQueue.CreateRange<Token>(input);
        }
    }
}