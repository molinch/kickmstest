using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLibrary
{
    public class ListWithExtras<TItem>: List<TItem>
    {
        private readonly IGenericMethods genericMethods;

        public ListWithExtras(IGenericMethods genericMethods)
        {
            this.genericMethods = genericMethods;
        }

        public TItem GetRandom()
        {
            return genericMethods.GetRandom(this);
        }

        // Generic method with two type arguments
        public TOutput GetRandomAs<TOutput>()
            where TOutput : class
        {
            return genericMethods.GetRandomAs<TItem, TOutput>(this);
        }

        // Useful to test a method having 'Of' inside it's name
        public List<TOutput> ProjectAllItemsOfList<TOutput>(Func<TItem, TOutput> projection)
            where TOutput : class
        {
            return genericMethods.ProjectAllItemsOfList(this, projection);
        }
    }
}
