using System;
using System.Collections;

namespace Xunit
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FactAttribute : Attribute { }

    public static class Assert
    {
        public static void Equal<T>(T expected, T actual)
        {
            if (!object.Equals(expected, actual))
                throw new Exception($"Assert.Equal failed. Expected:<{expected}>. Actual:<{actual}>");
        }

        public static void True(bool condition)
        {
            if (!condition) throw new Exception("Assert.True failed.");
        }

        public static void NotNull(object obj)
        {
            if (obj == null) throw new Exception("Assert.NotNull failed.");
        }

        public static void NotEmpty(IEnumerable collection)
        {
            if (collection == null) throw new Exception("Assert.NotEmpty failed - collection is null.");
            var enumerator = collection.GetEnumerator();
            if (!enumerator.MoveNext()) throw new Exception("Assert.NotEmpty failed - collection is empty.");
        }

        public static void IsType<T>(object obj)
        {
            if (!(obj is T)) throw new Exception($"Assert.IsType failed. Expected type {typeof(T).FullName} but was {obj?.GetType().FullName ?? "null"}.");
        }
    }
}
