using DemoLibrary;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.QualityTools.Testing.Fakes.Stubs;

namespace DemoUnitTests
{
    // Stubs class Number, which has only two virtual methods, hence only these are stubbable
    /* Not sure yet about what should be the inheritance, interface, ...
    public class StubNumber: Number, 
    {
        public FakesDelegates.Func<int, Number> MultiplyByInt32 { private get; set; }

        public FakesDelegates.Func<Number> RandomMultiply { private get; set; }

        public RandomMultiply()
    }*/
}
