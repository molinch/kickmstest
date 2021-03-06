﻿using DemoLibrary;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.QualityTools.Testing.Fakes.Stubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoUnitTests
{
    public class StubIMath: StubBase<IMath>, IMath
    {
        public FakesDelegates.Func<int, int, int> MultiplyInt32Int32 { private get; set; }
        
        int IMath.Multiply(int a, int b)
        {
            return MultiplyInt32Int32(a, b);
        }
    }
}
