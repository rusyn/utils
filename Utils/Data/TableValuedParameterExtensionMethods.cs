namespace Utils.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Dapper;

    public static class TableValuedParameterExtensionMethods
    {
        public static SqlMapper.ICustomQueryParameter AsTableValuedParameter<T>(
            this IEnumerable<T> values,
            string typeName,
            Action<TableValuedParameterColumnConfig<T>> columnConfigCollect)
        {
            var columnConfigurer = new TableValuedParameterColumnConfig<T>();
            columnConfigCollect(columnConfigurer);

            var array = new DataTable();
            array.SetTypeName(typeName);

            foreach (var columnDefinition in columnConfigurer.Registrations)
            {
                string name = columnDefinition.Alias ?? columnDefinition.Name;
                Type type = columnDefinition.GetColumnType();

                if (type.IsNullableType())
                {
                    type = Nullable.GetUnderlyingType(type);
                }

                array.Columns.Add(name, type);
            }

            array.BeginLoadData();
            foreach (var value in values)
            {
                var row = array.NewRow();
                int index = 0;

                foreach (var columnDefinition in columnConfigurer.Registrations)
                {
                    row[index++] = columnDefinition.GetValue(value) ?? DBNull.Value;
                }

                array.Rows.Add(row);
            }

            array.EndLoadData();

            return array.AsTableValuedParameter(typeName);
        }
    }
}