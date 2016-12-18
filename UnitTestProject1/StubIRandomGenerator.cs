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

        Number IRandomGenerator.Get()
        {
            return Get();
        }
    }
}
