namespace Utils.Tests
{
    using System;
    using System.Data;
    using System.Reflection;

    using Dapper;

    public class TableValuedParameterValidator
    {
        public TableValuedParameterValidator(SqlMapper.ICustomQueryParameter tableParameter)
        {
            BindingFlags privateInstanceFieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            Type tableParameterType = tableParameter.GetType();
            FieldInfo tableField = (FieldInfo)tableParameterType.GetMember("table", privateInstanceFieldFlags)[0];
            this.DataTable = (DataTable)tableField.GetValue(tableParameter);

            var typeNameField = (FieldInfo)tableParameterType.GetMember("typeName", privateInstanceFieldFlags)[0];
            this.TypeName = (string)typeNameField.GetValue(tableParameter);
        }

        public DataTable DataTable { get; }

        public string TypeName { get; }
    }
}