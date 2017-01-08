namespace DemoLibrary
{
    public interface IRandomGenerator
    {
        Number Get();

        Number Get(int max);

        Number Get(int min, int max);
    }
}