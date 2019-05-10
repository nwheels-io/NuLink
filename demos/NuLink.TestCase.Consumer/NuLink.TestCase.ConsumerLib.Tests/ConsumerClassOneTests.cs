using NUnit.Framework;

namespace NuLink.TestCase.ConsumerLib.Tests
{
    public class ConsumerClassOneTests
    {
        [Test]
        public void ClassOneShouldUseLocallyLinkedPackage()
        {
            var consumer = new ConsumerClassOne();
            var stringFromPackage = consumer.ConsumeStringFromFirstPackage();

            var expectedString = System.Environment.GetEnvironmentVariable("TEST_EXPECTED_STRING_1") ?? "???";
            Assert.AreEqual($"consumed-by-class-one:{expectedString}", stringFromPackage);
        }
    }
}