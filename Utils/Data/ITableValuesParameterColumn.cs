namespace Utils.Data
{
    using System;

    public interface ITableValuesParameterColumn<in TSubject>
    {
        string Alias { get; }

        string Name { get; }

        object GetValue(TSubject container);

        Type GetColumnType();
    }
}