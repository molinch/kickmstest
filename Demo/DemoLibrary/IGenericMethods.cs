using System;
using System.Collections.Generic;

namespace DemoLibrary
{
    public interface IGenericMethods
    {
        TItem GetRandom<TItem>(List<TItem> list);
        TOutput GetRandomAs<TItem, TOutput>(List<TItem> list) where TOutput : class;
        List<TOutput> ProjectAllItemsOfList<TItem, TOutput>(List<TItem> list, Func<TItem, TOutput> projection) where TOutput : class;
    }
}