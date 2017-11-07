namespace Utils.Data
{
    using System;

    internal static class TableValuedParameterHelper
    {
        public static Type UnwrapType(Type type)
        {
            if (type.IsGenericType)
            {
                Type underlyingType = Nullable.GetUnderlyingType(type);

                if (underlyingType?.IsEnum == true)
                {
                    return typeof(int?);
                }
            }

            if (type.IsEnum)
            {
                return typeof(int);
            }

            return type;
        }
    }
}