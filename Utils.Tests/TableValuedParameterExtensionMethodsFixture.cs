namespace Utils.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Dapper;

    using FluentAssertions;

    using NUnit.Framework;

    using Utils.Data;

    [TestFixture]
    public class TableValuedParameterExtensionMethodsFixture
    {
        private enum EnumState
        {
            ZeroValue = 0,

            OneValue = 1
        }

        private interface IInterfaceTestSubject
        {
            long RecordId { get; set; }

            INestedTestSubject NestedSubject { get; set; }

            string Comments { get; set; }

            bool IsActive { get; set; }
        }

        private interface INestedTestSubject
        {
            int Id { get; set; }
        }

        // ReSharper disable InconsistentNaming
        public class ConfigurableOverload
        {
            [Test]
            public void GivenAnEmptySubjectList_ShouldCreateTheTvpStructure_TvpShouldBeEmpty()
            {
                // Arrange.
                // ReSharper disable once CollectionNeverUpdated.Local
                List<TestSubject> subject = new List<TestSubject>();

                // Act.
                var tableParameter =
                    subject.AsTableValuedParameter(
                        "dbo.anyTypeDefinitionGoes",
                        columnDefinitions =>
                            {
                                columnDefinitions.Add(x => x.Id);
                                columnDefinitions.Add(x => x.Foo);
                                columnDefinitions.Add(x => x.NullString);
                                columnDefinitions.Add(x => x.NullableInt);
                            });

                // Assert.
                Assertions assertions = new Assertions(tableParameter);
                assertions.TableNameIs("dbo.anyTypeDefinitionGoes");

                assertions.HaveColumnCount(4);
                assertions.ColumnShouldBe(0, nameof(TestSubject.Id), typeof(int));
                assertions.ColumnShouldBe(1, nameof(TestSubject.Foo), typeof(string));
                assertions.ColumnShouldBe(2, nameof(TestSubject.NullString), typeof(string));
                assertions.ColumnShouldBe(3, nameof(TestSubject.NullableInt), typeof(int));

                assertions.NotHaveData();
            }

            [Test]
            public void GivenAnEmptySubjectList_MappingASubsetOfTheFields_ShouldCreateTheTvpStructure_TvpShouldBeEmpty()
            {
                // Arrange.
                // ReSharper disable once CollectionNeverUpdated.Local
                List<TestSubject> subject = new List<TestSubject>();

                // Act.
                var tableParameter =
                    subject.AsTableValuedParameter(
                        "dbo.anyTypeDefinitionGoes",
                        columnDefinitions =>
                            {
                                columnDefinitions.Add(x => x.Id, "IdAlias");
                                columnDefinitions.Add(x => x.Foo);
                            });

                // Assert.
                Assertions assertions = new Assertions(tableParameter);
                assertions.TableNameIs("dbo.anyTypeDefinitionGoes");

                assertions.HaveColumnCount(2);
                assertions.ColumnShouldBe(0, "IdAlias", typeof(int));
                assertions.ColumnShouldBe(1, nameof(TestSubject.Foo), typeof(string));

                assertions.NotHaveData();
            }

            [Test]
            public void GivenASingleItemInTheSubjectList_ShouldCreateTheTvpStructure_TvpShouldContainTheCorrectValues()
            {
                // Arrange.
                List<TestSubject> subject = new List<TestSubject>();
                subject.Add(
                    new TestSubject
                        {
                            Id = 1,
                            Foo = "Bar",
                            NullString = null,
                            NullableInt = null
                        });

                // Act.
                var tableParameter =
                    subject.AsTableValuedParameter(
                        "dbo.anyTypeDefinitionGoes",
                        columnDefinitions =>
                            {
                                columnDefinitions.Add(x => x.Id, "IdAlias");
                                columnDefinitions.Add(x => x.Foo);
                            });

                // Assert.
                var assertions = new Assertions(tableParameter);
                assertions.TableNameIs("dbo.anyTypeDefinitionGoes");

                assertions.HaveColumnCount(2);
                assertions.ColumnShouldBe(0, "IdAlias", typeof(int));
                assertions.ColumnShouldBe(1, nameof(TestSubject.Foo), typeof(string));

                assertions.HaveData(
                    new[]
                        {
                            new object[]
                                {
                                    1,
                                    "Bar"
                                }
                        });
            }

            [Test]
            public void ProjectingWithANullableTypeAsNull_ShouldUseDbNullValue()
            {
                // Arrange.
                List<TestSubject> subject = new List<TestSubject>();
                subject.Add(
                    new TestSubject
                        {
                            Id = 1,
                            NullableInt = null
                        });

                // Act.
                var tableParameter =
                    subject.AsTableValuedParameter(
                        "dbo.anyTypeDefinitionGoes",
                        columnDefinitions =>
                            {
                                columnDefinitions.Add(x => x.Id);
                                columnDefinitions.Add(x => x.NullableInt);
                            });

                // Assert.
                var assertions = new Assertions(tableParameter);
                assertions.TableNameIs("dbo.anyTypeDefinitionGoes");

                assertions.HaveColumnCount(2);
                assertions.ColumnShouldBe(0, nameof(TestSubject.Id), typeof(int));
                assertions.ColumnShouldBe(1, nameof(TestSubject.NullableInt), typeof(int));

                assertions.HaveData(
                    new[]
                        {
                            new object[]
                                {
                                    1,
                                    DBNull.Value
                                }
                        });
            }

            [Test]
            public void GivenANestedSubject_TheNestedSubjectPropertiesShouldBeIncludedInTheTvp()
            {
                // Arrange.
                var subject = new List<TestSubjectWithChild>();
                subject.Add(
                    new TestSubjectWithChild
                        {
                            Id = 1,
                            Child = new TestSubjectWithChild
                                        {
                                            Id = 2
                                        }
                        });

                // Act.
                var tableParameter = subject.AsTableValuedParameter(
                    "foo",
                    columns =>
                        {
                            columns.Add(x => x.Child.Id, "ChildId");
                        });

                // Assert.
                var assertions = new Assertions(tableParameter);
                assertions.TableNameIs("foo");

                assertions.HaveColumnCount(1);
                assertions.ColumnShouldBe(0, "ChildId", typeof(int));

                assertions.HaveData(
                    new[]
                        {
                            new object[]
                                {
                                    2
                                }
                        });
            }

            [Test]
            public void GivenAnInterfaceReference_ShouldProjectTheInterfaceValues()
            {
                // Arrange.
                var subject = new List<IInterfaceTestSubject>();
                subject.Add(
                    new InterfaceTestSubject
                        {
                            RecordId = 1,
                            NestedSubject = new NestedTestSubject
                                                     {
                                                         Id = 4
                                                     },
                            Comments = "Foo",
                            IsActive = true
                        });

                // Act.
                var tableParameter = subject.AsTableValuedParameter(
                    "foo",
                    columns =>
                        {
                            columns.Add(x => x.RecordId);
                            columns.Add(x => x.NestedSubject.Id, "ReasonId");
                            columns.Add(x => x.Comments);
                            columns.Add(x => x.IsActive);
                        });

                // Assert.
                var assertions = new Assertions(tableParameter);
                assertions.TableNameIs("foo");

                assertions.HaveColumnCount(4);
                assertions.ColumnShouldBe(0, nameof(IInterfaceTestSubject.RecordId), typeof(long));
                assertions.ColumnShouldBe(1, "ReasonId", typeof(int));
                assertions.ColumnShouldBe(2, nameof(IInterfaceTestSubject.Comments), typeof(string));
                assertions.ColumnShouldBe(3, nameof(IInterfaceTestSubject.IsActive), typeof(bool));

                assertions.HaveData(
                    new[]
                        {
                            new object[]
                                {
                                    1,
                                    4,
                                    "Foo",
                                    true
                                }
                        });
            }

            [Test]
            public void GivenAnEnumSubject_ShouldProjectTheEnumValueAsAnInt()
            {
                // Arrange.
                var subject = new List<EnumSubject>();
                subject.Add(
                    new EnumSubject
                        {
                            State = EnumState.ZeroValue
                        });
                subject.Add(
                    new EnumSubject
                        {
                            State = EnumState.OneValue
                        });

                // Act.
                var tableParameter = subject.AsTableValuedParameter(
                    "foo",
                    columns =>
                        {
                            columns.Add(x => x.State);
                        });

                // Assert.
                var assertions = new Assertions(tableParameter);
                assertions.TableNameIs("foo");

                assertions.HaveColumnCount(1);
                assertions.ColumnShouldBe(0, nameof(EnumSubject.State), typeof(int));

                assertions.HaveData(
                    new[]
                        {
                            new object[]
                                {
                                    0
                                },
                            new object[]
                                {
                                    1
                                }
                        });
            }

            [Test]
            public void GivenANullableEnumSubject_ShouldMapTheValuesAsNullableInts()
            {
                // Arrange.
                var subject = new List<NullableEnumSubject>();
                subject.Add(
                    new NullableEnumSubject
                        {
                            State = EnumState.ZeroValue
                        });
                subject.Add(
                    new NullableEnumSubject
                        {
                            State = EnumState.OneValue
                        });
                subject.Add(
                    new NullableEnumSubject
                        {
                            State = null
                        });

                // Act.
                var tableParameter = subject.AsTableValuedParameter(
                    "foo",
                    columns =>
                        {
                            columns.Add(x => x.State);
                        });

                // Assert.
                var assertions = new Assertions(tableParameter);
                assertions.TableNameIs("foo");

                assertions.HaveColumnCount(1);

                // Nullables converted from int? -> int -> DBNull.Value.
                assertions.ColumnShouldBe(0, nameof(NullableEnumSubject.State), typeof(int));

                assertions.HaveData(
                    new[]
                        {
                            new object[]
                                {
                                    0
                                },
                            new object[]
                                {
                                    1
                                },
                            new object[]
                                {
                                    DBNull.Value
                                }
                        });
            }

            [Test]
            public void GivenAStaticFieldDefinition_TheStaticValueShouldBeMappedAsIs()
            {
                // Arrange.
                var subject = new List<TestSubject>();
                subject.Add(
                    new TestSubject
                        {
                            Id = 1
                        });

                // Act.
                var tableParameter = subject.AsTableValuedParameter(
                    "foo",
                    columns =>
                        {
                            columns.Add(x => x.Id);
                            columns.Static<int?>("NullSubjectField", null);
                            columns.Static<int?>("SubjectField", 12);
                        });

                // Assert.
                var assertions = new Assertions(tableParameter);
                assertions.TableNameIs("foo");

                assertions.HaveColumnCount(3);

                assertions.ColumnShouldBe(0, nameof(TestSubject.Id), typeof(int));
                assertions.ColumnShouldBe(1, "NullSubjectField", typeof(int));
                assertions.ColumnShouldBe(2, "SubjectField", typeof(int));

                assertions.HaveData(
                    new[]
                        {
                            new object[]
                                {
                                    1,
                                    DBNull.Value,
                                    12
                                }
                        });
            }

            private class Assertions
            {
                private readonly TableValuedParameterValidator parameterValidator;

                private readonly DataTable table;

                public Assertions(SqlMapper.ICustomQueryParameter tableParameter)
                {
                    this.parameterValidator = new TableValuedParameterValidator(tableParameter);

                    this.table = this.parameterValidator.DataTable;
                    this.table.Should().NotBeNull();
                }

                public void TableNameIs(string expected)
                {
                    this.parameterValidator.TypeName.Should().Be(expected);
                }

                public void HaveColumnCount(int expected)
                {
                    this.table.Columns.Count.Should().Be(expected);
                }

                public void ColumnShouldBe(int index, string expectedName, Type expectedType)
                {
                    this.table.Columns[index]
                        .ShouldBeEquivalentTo(
                            new
                                {
                                    ColumnName = expectedName,
                                    DataType = expectedType
                                },
                            config => config.ExcludingMissingMembers());
                }

                public void HaveData(object[][] expectations)
                {
                    this.table.Rows.Count.Should().Be(expectations.Length);

                    for (int i = 0; i < expectations.Length; i++)
                    {
                        this.table.Rows[i]
                            .ItemArray
                            .ShouldAllBeEquivalentTo(expectations[i], $"(source data row {i})");
                    }
                }

                public void NotHaveData()
                {
                    this.table.Rows.Count.Should().Be(0, "no data is expected");
                }
            }
        }

        private class TestSubject
        {
            public int Id { get; set; }

            public string Foo { get; set; }

            public int? NullableInt { get; set; }

            public string NullString { get; set; }
        }

        private class TestSubjectWithChild
        {
            public int Id { get; set; }

            public TestSubjectWithChild Child { get; set; }
        }

        private class InterfaceTestSubject : IInterfaceTestSubject
        {
            public long RecordId { get; set; }

            public INestedTestSubject NestedSubject { get; set; }

            public string Comments { get; set; }

            public bool IsActive { get; set; }
        }

        private class NestedTestSubject : INestedTestSubject
        {
            public int Id { get; set; }
        }

        private class EnumSubject
        {
            public EnumState State { get; set; }
        }

        private class NullableEnumSubject
        {
            public EnumState? State { get; set; }
        }
    }
}