namespace ProceduralDataflow.Tests
{
    public class PossibleValues<T>
    {
        public T[] Values { get; }

        public PossibleValues(T[] values)
        {
            Values = values;
        }

        public PossibleValues(T value) : this(new[] { value }) { }

        public static implicit operator PossibleValues<T>(T[] values)
        {
            return new PossibleValues<T>(values);
        }

        public static implicit operator PossibleValues<T>(T value)
        {
            return new PossibleValues<T>(value);
        }
    }
}
