namespace DemoLibrary
{
    public interface IGenericPair<TKey, TValue>
    {
        TKey Key { get; }
        TValue Value { get; }

        bool Equals(object obj);
    }
}