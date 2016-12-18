using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.QualityTools.Testing.Fakes.Stubs;
using DemoLibrary;

namespace DemoUnitTests
{
    [TestClass]
    public class Number_Equals_Test : StubBase
    {
        [TestMethod]
        public void DirectInvocation()
        {
            var math = new DemoLibrary.Math();
            var random = new RandomGenerator(math);

            var number = new Number(math, random, 5);
            var number2 = new Number(math, random, 5);

            Assert.IsTrue(number.Equals(number2));
        }

        [TestMethod]
        public void FakesStubInvocation()
        {
            var mathStub = new StubIMath();
            var randomStub = new StubIRandomGenerator();

            var number = new Number(mathStub, randomStub, 5);
            var number2 = new StubINumber();
            number2.ValueGet = () => 5;

            Assert.IsTrue(number.Equals(number2));
        }

        [TestMethod]
        public void MoqInvocation()
        {
            var mathStubDef = new Mock<IMath>();
            var randomStubDef = new Mock<IRandomGenerator>();

            var number = new Number(mathStubDef.Object, randomStubDef.Object, 5);
            var number2Def = new Mock<INumber>();
            number2Def
                .Setup(c => c.Value)
                .Returns(5);

            Assert.IsTrue(number.Equals(number2Def.Object));
        }
    }
}
