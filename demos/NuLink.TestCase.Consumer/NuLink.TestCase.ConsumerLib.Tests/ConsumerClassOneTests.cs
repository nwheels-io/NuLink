using NUnit.Framework;

namespace NuLink.TestCase.ConsumerLib.Tests
{
    public class ConsumerClassOneTests
    {
        [Test]
        public void ClassOneShouldUseLocallyLinkedPackage()
        {
            var consumer = new ConsumerClassOne();
            var actualString = consumer.ConsumeStringFromFirstPackage();
            var expectedString = 
                System.Environment.GetEnvironmentVariable($"TEST_{nameof(ClassOneShouldUseLocallyLinkedPackage)}") 
                ?? "???";

            Assert.AreEqual($"consumed-by-class-one:{expectedString}", actualString);
        }
    }
}