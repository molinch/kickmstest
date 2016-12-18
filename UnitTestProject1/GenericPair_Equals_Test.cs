using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.QualityTools.Testing.Fakes.Stubs;
using DemoLibrary;

namespace DemoUnitTests
{
    [TestClass]
    public class GenericPair_Equals_Test : StubBase
    {
        [TestMethod]
        public void DirectInvocation()
        {
            var pair = new GenericPair<string, int>("blah", 1);
            var pair2 = new GenericPair<string, int>("blah", 1);

            Assert.IsTrue(pair.Equals(pair2));
        }

        [TestMethod]
        public void FakesStubInvocation()
        {
            var pair = new GenericPair<string, int>("blah", 1);
            var pair2 = new StubIGenericPair<string, int>()
            {
                KeyGet = () => "blah",
                ValueGet = () => 1,
            };

            Assert.IsTrue(pair.Equals(pair2));
        }

        [TestMethod]
        public void MoqInvocation()
        {
            var pair = new GenericPair<string, int>("blah", 1);
            var pair2Def = new Mock<IGenericPair<string, int>>();
            pair2Def.Setup(c => c.Key).Returns("blah");
            pair2Def.Setup(c => c.Value).Returns(1);

            Assert.IsTrue(pair.Equals(pair2Def.Object));
        }
    }
}
