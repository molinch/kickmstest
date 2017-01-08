using DemoLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace DemoUnitTests
{
    [TestClass]
    public class GenericMethods_GetRandom_Test
    {
        [TestMethod]
        public void DirectInvocation()
        {
            var sut = new GenericMethods(new RandomGenerator(new Math()));
            var rand = sut.GetRandom(new List<int> { 5, 3, 9, 8, 2, 1 });

            // we cannot assert anything in this case, the number is random
        }

        [TestMethod]
        public void FakesStubInvocation()
        {
            var randomGenerator = new StubIRandomGenerator();
            randomGenerator.GetInt32Int32 = (min, max) => { return new Number(new StubIMath(), randomGenerator, 3); };
            var sut = new GenericMethods(randomGenerator);

            var rand = sut.GetRandom(new List<int> { 5, 3, 9, 8, 2, 1 });

            Assert.AreEqual(8, rand);
        }

        [TestMethod]
        public void MoqInvocation()
        {
            var math = new Mock<IMath>();

            var randomGenerator = new Mock<IRandomGenerator>();
            randomGenerator.Setup(c => c.Get(It.IsAny<int>(), It.IsAny<int>())).Returns(() => new Number(math.Object, randomGenerator.Object, 3));
            var sut = new GenericMethods(randomGenerator.Object);

            var rand = sut.GetRandom(new List<int> { 5, 3, 9, 8, 2, 1 });

            Assert.AreEqual(8, rand);
        }
    }
}
