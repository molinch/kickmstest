using DemoLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace DemoUnitTests
{
    [TestClass]
    public class ListWithExtras_GetRandom_Test
    {
        [TestMethod]
        public void DirectInvocation()
        {
            var genericMethods = new GenericMethods(new RandomGenerator(new Math()));
            var sut = new ListWithExtras<int>(genericMethods) { 5, 3, 9, 8, 2, 1 };

            var rand = sut.GetRandom();

            // we cannot assert anything in this case, the number is random
        }

        [TestMethod]
        public void FakesStubInvocation()
        {
            var genericMethods = new StubIGenericMethods();
            genericMethods.GetRandomOf2ListOfM0<int>(list => 8);
            var sut = new ListWithExtras<int>(genericMethods) { 5, 3, 9, 8, 2, 1 };
            
            var rand = sut.GetRandom();

            Assert.AreEqual(8, rand);
        }

        [TestMethod]
        public void MoqInvocation()
        {
            var genericMethods = new Mock<IGenericMethods>();
            genericMethods.Setup(c => c.GetRandom<int>(It.IsAny<List<int>>())).Returns(() => 8);
            var sut = new ListWithExtras<int>(genericMethods.Object) { 5, 3, 9, 8, 2, 1 };

            var rand = sut.GetRandom();

            Assert.AreEqual(8, rand);
        }
    }
}
