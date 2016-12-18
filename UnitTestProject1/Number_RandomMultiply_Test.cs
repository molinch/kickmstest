using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.QualityTools.Testing.Fakes.Stubs;
using DemoLibrary;

namespace DemoUnitTests
{
    [TestClass]
    public class Number_RandomMultiply_Test : StubBase
    {
        [TestMethod]
        public void DirectInvocation()
        {
            var math = new DemoLibrary.Math();
            var random = new RandomGenerator(math);

            var number = new Number(math, random, 5);
            var result = number.RandomMultiply();

            // direct invocation cannot be asserted since it is random
        }

        [TestMethod]
        public void FakesStubInvocation()
        {
            var mathStub = new StubIMath();
            mathStub.MultiplyInt32Int32 = (a, b) => 15;
            var randomStub = new StubIRandomGenerator();
            randomStub.Get = () => new Number(mathStub, randomStub, 17);

            var number = new Number(mathStub, randomStub, 5);
            var result = number.RandomMultiply();

            Assert.AreEqual(15, result.Value);
        }

        [TestMethod]
        public void MoqInvocation()
        {
            var mathStubDef = new Mock<IMath>();
            mathStubDef
                .Setup(c => c.Multiply(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(15);

            var randomStubDef = new Mock<IRandomGenerator>();
            randomStubDef
                .Setup(c => c.Get())
                .Returns(new Number(mathStubDef.Object, randomStubDef.Object, 17));

            var number = new Number(mathStubDef.Object, randomStubDef.Object, 5);
            var result = number.MultiplyBy(3);

            Assert.AreEqual(15, result.Value);
        }
    }
}
