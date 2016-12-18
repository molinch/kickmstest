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
    public class StubIGenericPair<TKey, TValue> : StubBase<IGenericPair<TKey, TValue>>, IGenericPair<TKey, TValue>
    {
        public FakesDelegates.Func<TKey> KeyGet { private get; set; }

        public FakesDelegates.Func<TValue> ValueGet { private get; set; }

        public FakesDelegates.Func<object, bool> EqualsObject { private get; set; }

        TKey IGenericPair<TKey, TValue>.Key
        {
            get
            {
                return KeyGet();
            }
        }

        TValue IGenericPair<TKey, TValue>.Value
        {
            get
            {
                return ValueGet();
            }
        }


        bool IGenericPair<TKey, TValue>.Equals(object obj)
        {
            return EqualsObject(obj);
        }
    }
}
