using System.Linq;

namespace ProceduralDataflow.Tests
{
    public static class PossibleValuesComparer
    {
        public static bool AreEqual<T>(T[] left, PossibleValues<T>[] right)
        {
            if (left.Length != right.Length)
                return false;

            for (int i = 0; i < left.Length; i++)
            {
                if (!right[i].Values.Contains(left[i]))
                    return false;
            }

            return true;
        }
    }
}