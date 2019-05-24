using NUnit.Framework;

namespace NuLink.TestCase.ConsumerLib.Tests
{
    public class ConsumerClassTwoTests
    {
        [Test]
        public void ClassTwoShouldUseLocallyLinkedPackage()
        {
            var consumer = new ConsumerClassTwo();
            var actualString = consumer.ConsumeStringFromSecondPackage();
            var expectedString = 
                System.Environment.GetEnvironmentVariable($"TEST_{nameof(ClassTwoShouldUseLocallyLinkedPackage)}") 
                ?? "???";

            Assert.AreEqual($"consumed-by-class-two:{expectedString}", actualString);
        }
    }
}