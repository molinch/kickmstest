using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLibrary
{
    public static class GenericPair
    {
        public static bool Equals<TKey, TValue>(IGenericPair<TKey, TValue> pair1, IGenericPair<TKey, TValue> pair2)
        {
            return pair1.Key.Equals(pair2.Key) && pair1.Value.Equals(pair2.Value);
        }
    }

#pragma warning disable CS0659
    public class GenericPair<TKey, TValue> : IGenericPair<TKey, TValue>
#pragma warning restore CS0659
    {
        public GenericPair(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            Key = key;
            Value = value;
        }

        public TKey Key { get; }

        public TValue Value { get; }

        public override bool Equals(object obj)
        {
            var pair = obj as IGenericPair<TKey, TValue>;
            if (obj == null)
                return false;

            return GenericPair.Equals(this, pair);
        }
    }
}
