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
    public class StubIRandomGenerator : StubBase<IRandomGenerator>, IRandomGenerator
    {
        public FakesDelegates.Func<Number> Get { private get; set; }

        public FakesDelegates.Func<int, Number> GetInt32 { private get; set; }

        public FakesDelegates.Func<int, int, Number> GetInt32Int32 { private get; set; }

        Number IRandomGenerator.Get()
        {
            return Get();
        }

        Number IRandomGenerator.Get(int max)
        {
            return GetInt32(max);
        }

        Number IRandomGenerator.Get(int min, int max)
        {
            return GetInt32Int32(min, max);
        }
    }
}
