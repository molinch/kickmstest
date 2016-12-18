using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLibrary
{
    public class RandomGenerator : IRandomGenerator
    {
        private static readonly Random random = new Random();
        private readonly IMath math;

        public RandomGenerator(IMath math)
        {
            this.math = math;
        }

        public Number Get()
        {
            return new Number(math, this, random.Next());
        }
    }
}
