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

            Assert.AreEqual("consumed-by-class-one:FIRST-CLASS-SYMLINKED", stringFromPackage);
        }
    }
}