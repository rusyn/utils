namespace Utils.Data
{
    using System;

    public class PropertyBoundColumn<TSubject> : ITableValuesParameterColumn<TSubject>
    {
        private readonly Func<TSubject, object> valueAccessor;

        public PropertyBoundColumn(string memberName, Type memberType, Func<TSubject, object> valueAccessor, string alias)
        {
            this.Name = memberName;
            this.PropertyType = memberType;
            this.valueAccessor = valueAccessor;
            this.Alias = alias;
        }

        public string Alias { get; }

        public string Name { get; }

        public Type PropertyType { get; }

        public object GetValue(TSubject container)
        {
            object value = this.valueAccessor.Invoke(container);

            // ReSharper disable once UseNullPropagation  -- invalid cast exception if attempting to cast to (int?).
            if (this.PropertyType.IsEnum && value != null)
            {
                value = (int)value;
            }

            return value;
        }

        public Type GetColumnType()
        {
            return TableValuedParameterHelper.UnwrapType(this.PropertyType);
        }
    }
}