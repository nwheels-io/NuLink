using System.Collections.Generic;
using System.Linq;

namespace Simple.CommandLine
{
    public abstract class Token
    {
        public static List<Token> List(params Token[] tokens)
        {
            return tokens.ToList();
        }
    }

    public class NameToken : Token
    {
        public NameToken(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"name[{Name}]";
        }

        public override bool Equals(object obj)
        {
            return (obj is NameToken other && other.Name == this.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() | 0x01;
        }

        public string Name { get; }
    }

    public class ValueToken : Token
    {
        public ValueToken(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"value[{Value}]";
        }
        
        public override bool Equals(object obj)
        {
            return (obj is ValueToken other && other.Value == this.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode() & ~0x01;
        }

        public string Value { get; }
    }
}
