using System;
using System.Collections.Generic;
using DemoLibrary;
using Microsoft.QualityTools.Testing.Fakes.Stubs;
using Microsoft.QualityTools.Testing.Fakes;

namespace DemoUnitTests
{
    public class StubIGenericMethods : StubBase<IGenericMethods>, IGenericMethods
    {
        private Dictionary<Type, object> StubGetRandomOf2ListOfM0 = new Dictionary<Type, object>();
        private Dictionary<Type, Dictionary<Type, object>> StubGetRandomAsOfM0M1ListOfM0 = new Dictionary<Type, Dictionary<Type, object>>();
        private Dictionary<Type, Dictionary<Type, object>> StubProjectAllItemsOfListOfM0M1ListOfM0FuncOfM0M1 = new Dictionary<Type, Dictionary<Type, object>>();

        public void GetRandomOf2ListOfM0<TItem>(FakesDelegates.Func<List<TItem>, TItem> func)
        {
            StubGetRandomOf2ListOfM0.Add(typeof(TItem), func);
        }

        public void GetRandomAsOfM0M1ListOfM0<TItem, TOutput>(FakesDelegates.Func<List<TItem>, TOutput> func)
            where TOutput : class
        {
            Dictionary<Type, object> dict = null;
            if (!StubGetRandomAsOfM0M1ListOfM0.TryGetValue(typeof(TItem), out dict))
            {
                dict = new Dictionary<Type, object>();
                StubGetRandomAsOfM0M1ListOfM0.Add(typeof(TItem), dict);
            }

            dict.Add(typeof(TOutput), func);
        }

        public void ProjectAllItemsOfListOfM0M1ListOfM0FuncOfM0M1<TItem, TOutput>(FakesDelegates.Func<List<TItem>, Func<TItem, TOutput>, List<TOutput>> func)
            where TOutput : class
        {
            Dictionary<Type, object> dict = null;
            if (!StubProjectAllItemsOfListOfM0M1ListOfM0FuncOfM0M1.TryGetValue(typeof(TItem), out dict))
            {
                dict = new Dictionary<Type, object>();
                StubProjectAllItemsOfListOfM0M1ListOfM0FuncOfM0M1.Add(typeof(TItem), dict);
            }

            dict.Add(typeof(TOutput), func);
        }

        public TItem GetRandom<TItem>(List<TItem> list)
        {
            var func = StubGetRandomOf2ListOfM0[typeof(TItem)] as FakesDelegates.Func<List<TItem>, TItem>;
            return func(list);
        }

        public TOutput GetRandomAs<TItem, TOutput>(List<TItem> list) where TOutput : class
        {
            var func = StubGetRandomAsOfM0M1ListOfM0[typeof(TItem)][typeof(TOutput)] as FakesDelegates.Func<List<TItem>, TOutput>;
            return func(list);
        }

        public List<TOutput> ProjectAllItemsOfList<TItem, TOutput>(List<TItem> list, Func<TItem, TOutput> projection) where TOutput : class
        {
            var func = StubProjectAllItemsOfListOfM0M1ListOfM0FuncOfM0M1[typeof(TItem)][typeof(TOutput)] as FakesDelegates.Func<List<TItem>, Func<TItem, TOutput>, List<TOutput>>;
            return func(list, projection);
        }
    }
}
