namespace Utils
{
    using System;

    public static class PrimitiveExtensions
    {
        public static bool IsNullableType(this Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            return false;
        }
    }
}