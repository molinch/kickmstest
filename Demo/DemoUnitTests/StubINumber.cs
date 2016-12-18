using DemoLibrary;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.QualityTools.Testing.Fakes.Stubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoUnitTests
{
    public class StubINumber: StubBase<INumber>, INumber
    {
        public FakesDelegates.Func<int> ValueGet { private get; set; }

        public FakesDelegates.Func<object, bool> EqualsObject { private get; set; }

        public FakesDelegates.Func<int, Number> MultiplyByInt32 { private get; set; }

        public FakesDelegates.Func<Number> RandomMultiply { private get; set; }

        int INumber.Value
        {
            get
            {
                return ValueGet();
            }
        }

        bool INumber.Equals(object obj)
        {
            return EqualsObject(obj);
        }
        
        Number INumber.MultiplyBy(int value)
        {
            return MultiplyByInt32(value);
        }

        Number INumber.RandomMultiply()
        {
            return RandomMultiply();
        }
    }
}
