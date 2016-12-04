using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.QualityTools.Testing.Fakes.Stubs;
using DemoLibrary;

namespace DemoUnitTests
{
    [TestClass]
    public class UnitTest1: StubBase
    {
        [TestMethod]
        public void DirectInvocation()
        {
            var math = new DemoLibrary.Math();

            var number = new Number(math, 5);
            var result = number.MultiplyBy(3);

            Assert.AreEqual(15, result.Value);
        }

        [TestMethod]
        public void FakesShimInvocation()
        {
            var mathStub = new StubIMath();
            mathStub.SetMultiplyInt32Int32 = (a, b) => 15;

            var number = new Number(mathStub, 5);
            var result = number.MultiplyBy(3);

            Assert.AreEqual(15, result.Value);
        }

        [TestMethod]
        public void MoqInvocation()
        {
            var mathStubDef = new Mock<IMath>();
            mathStubDef
                .Setup(c => c.Multiply(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(15);
            var mathStub = mathStubDef.Object;

            var number = new Number(mathStub, 5);
            var result = number.MultiplyBy(3);

            Assert.AreEqual(15, result.Value);
        }
    }
}
