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

            Assert.AreEqual("consumed-by-class-two:SECOND-CLASS-SYMLINKED(FIRST-CLASS-SYMLINKED)", stringFromPackage);
        }
    }
}