namespace Utils.Data
{
    using System;

    public class StaticFieldValue<TSubject, TValue> : ITableValuesParameterColumn<TSubject>
    {
        private readonly TValue value;

        public StaticFieldValue(string columnName, TValue value)
        {
            this.Alias = this.Name = columnName;
            this.value = value;
        }

        public string Alias { get; }

        public string Name { get; }

        public object GetValue(TSubject container)
        {
            return this.value;
        }

        public Type GetColumnType()
        {
            return TableValuedParameterHelper.UnwrapType(typeof(TValue));
        }
    }
}