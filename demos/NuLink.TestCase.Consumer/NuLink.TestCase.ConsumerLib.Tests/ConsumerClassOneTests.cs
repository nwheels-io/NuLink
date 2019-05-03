using NUnit.Framework;

namespace NuLink.TestCase.ConsumerLib.Tests
{
    public class ConsumerClassOneTests
    {
        [Test]
        public void ClassShouldUseLocallyLinkedPackage()
        {
            var consumer = new ConsumerClassOne();
            var stringFromPackage = consumer.ConsumeStringFromFirstPackage();

            Assert.AreEqual("consumer-class-one:FIRST-CLASS-SYMLINKED", stringFromPackage);
        }
    }
}