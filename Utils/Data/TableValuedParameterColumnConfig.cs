namespace Utils.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public class TableValuedParameterColumnConfig<TSubject>
    {
        public TableValuedParameterColumnConfig()
        {
            this.Registrations = new List<ITableValuesParameterColumn<TSubject>>();
        }

        internal List<ITableValuesParameterColumn<TSubject>> Registrations { get; }

        public void Add(Expression<Func<TSubject, object>> getBoundMember, string columnNameAlias = null)
        {
            MemberExpression memberExpression = getBoundMember.Body as MemberExpression;
            if (memberExpression == null)
            {
                UnaryExpression unaryExpression = (UnaryExpression)getBoundMember.Body;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new InvalidOperationException("Couldn't find the target member.");
            }

            PropertyInfo targetedProperty = memberExpression.Member as PropertyInfo;
            if (targetedProperty == null)
            {
                throw new InvalidOperationException($"Member {memberExpression.Member.Name} must be a property.");
            }

            PropertyBoundColumn<TSubject> column = new PropertyBoundColumn<TSubject>(
                targetedProperty.Name,
                targetedProperty.PropertyType,
                getBoundMember.Compile(),
                columnNameAlias);

            this.Registrations.Add(column);
        }

        public void Static<TValue>(string columnName, TValue value)
        {
            StaticFieldValue<TSubject, TValue> staticColumn = new StaticFieldValue<TSubject, TValue>(columnName, value);

            this.Registrations.Add(staticColumn);
        }
    }
}