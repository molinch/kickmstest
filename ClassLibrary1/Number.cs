namespace DemoLibrary
{
    public class Number
    {
        private readonly IMath _math;
        private int _value;
        public Number(IMath math, int value)
        {
            _math = math;
            _value = value;
        }

        public Number MultiplyBy(int value)
        {
            return new Number(_math, _math.Multiply(_value, value));
        }

        public int Value
        {
            get
            {
                return _value;
            }
        }
    }
}