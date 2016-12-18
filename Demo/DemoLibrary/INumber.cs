namespace DemoLibrary
{
    public interface INumber
    {
        int Value { get; }

        bool Equals(object obj);
        Number MultiplyBy(int value);
        Number RandomMultiply();
    }
}