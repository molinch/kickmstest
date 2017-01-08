namespace DemoLibrary
{
#pragma warning disable CS0659
    public class Number : INumber
#pragma warning restore CS0659
    {
        private readonly IMath math;
        private readonly IRandomGenerator random;
        private int value;
        public Number(IMath math, IRandomGenerator random, int value)
        {
            this.math = math;
            this.random = random;
            this.value = value;
        }

        public virtual Number MultiplyBy(int value)
        {
            return new Number(math, random, math.Multiply(Value, value));
        }

        public virtual Number RandomMultiply()
        {
            return MultiplyBy(random.Get().Value);
        }

        public int Value
        {
            get
            {
                return value;
            }
        }

        public override bool Equals(object obj)
        {
            var number = obj as INumber;
            if (number == null)
                return false;

            return number.Value.Equals(Value);
        }
    }
}