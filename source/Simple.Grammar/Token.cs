using System.Collections.Immutable;

namespace Simple.Grammar
{
    public abstract class Token
    {
        protected Token(string text)
        {
            Text = text;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}[{this.Text}]";
        }

        public string Text { get; }
    }
}
