# Data #
### TableValuedParameterExtensionMethods ###
_AsTableValuedParameter_ projects the properties of object instances in a typed collection to a table valued parameter for consumption by Dapper.

The mapping process creates a DataTable instance, with a DataColumn (in order of definition):
* per property on the subject, optionally aliased, and using the property's type; enums are projected to integers
* per static column value

```c#
List<TestSubject> subject = new List<TestSubject>();
var tableParameter =
    subject.AsTableValuedParameter(
        "dbo.nameOfTheSqlServerUdt",
        columns =>
            {
                columns.Add(x => x.Id);
                columns.Add(x => x.NullableInt, "ColumnAlias");
                columns.Static<string>("StaticFieldAlias", "staticValue");
            });

private class TestSubject
{
    public int Id { get; set; }
    public int? NullableInt { get; set; }
}
```