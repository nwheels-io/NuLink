using NUnit.Framework;

namespace NuLink.TestCase.ConsumerLib.Tests
{
    public class ConsumerClassTwoTests
    {
        [Test]
        public void ClassTwoShouldUseLocallyLinkedPackage()
        {
            var consumer = new ConsumerClassTwo();
            var stringFromPackage = consumer.ConsumeStringFromSecondPackage();

            var expectedString = System.Environment.GetEnvironmentVariable("TEST_EXPECTED_STRING_2") ?? "???";
            Assert.AreEqual($"consumed-by-class-two:{expectedString}", stringFromPackage);
        }
    }
}