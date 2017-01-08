using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLibrary
{
    public class GenericMethods : IGenericMethods
    {
        private readonly IRandomGenerator randomGenerator;

        public GenericMethods(IRandomGenerator randomGenerator)
        {
            this.randomGenerator = randomGenerator;
        }

        // Generic method with one type argument
        public TItem GetRandom<TItem>(List<TItem> list)
        {
            var randomNumber = randomGenerator.Get(0, list.Count - 1);
            return list[randomNumber.Value];
        }

        // Generic method with two type arguments
        public TOutput GetRandomAs<TItem,TOutput>(List<TItem> list)
            where TOutput: class
        {
            return GetRandom(list) as TOutput;
        }

        // Useful to test a method having 'Of' inside it's name
        public List<TOutput> ProjectAllItemsOfList<TItem, TOutput>(List<TItem> list, Func<TItem, TOutput> projection)
            where TOutput : class
        {
            return list.Select(projection).ToList();
        }
    }
}
