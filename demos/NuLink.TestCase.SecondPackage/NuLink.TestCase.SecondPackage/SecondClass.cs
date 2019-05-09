using NuLink.TestCase.FirstPackage;

namespace NuLink.TestCase.SecondPackage
{
    public class SecondClass
    {
        public string GetSecondString()
        {
            var first = new FirstClass();
            return $"SECOND-CLASS-ORIGINAL({first.GetString()})";
        }
    }
}
