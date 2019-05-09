using NuLink.TestCase.SecondPackage;

namespace NuLink.TestCase.ConsumerLib
{
    public class ConsumerClassTwo
    {
        public string ConsumeStringFromSecondPackage()
        {
            var second = new SecondClass();
            var secondString = second.GetSecondString();
            return $"consumed-by-class-two:{secondString}";
        }
    }
}