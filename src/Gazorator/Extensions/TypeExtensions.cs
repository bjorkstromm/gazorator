using System;

namespace Gazorator.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            return !type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
