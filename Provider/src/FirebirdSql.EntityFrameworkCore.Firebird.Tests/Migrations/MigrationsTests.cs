﻿/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Collections.Generic;
using System.Linq;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.Migrations
{
	public class MigrationsTests : EntityFrameworkCoreTestsBase
	{
		[Test]
		public void CreateTableOperation()
		{
			var operation = new CreateTableOperation
			{
				Name = "People",
				Columns =
				{
						new AddColumnOperation
						{
							Name = "Id",
							Table = "People",
							ClrType = typeof(int),
							IsNullable = false,
							[FbAnnotationNames.ValueGenerationStrategy] = FbValueGenerationStrategy.None,
						},
						new AddColumnOperation
						{
							Name = "Id_Identity",
							Table = "People",
							ClrType = typeof(int),
							IsNullable = false,
							[FbAnnotationNames.ValueGenerationStrategy] = FbValueGenerationStrategy.IdentityColumn,
						},
						new AddColumnOperation
						{
							Name = "Id_Sequence",
							Table = "People",
							ClrType = typeof(int),
							IsNullable = false,
							[FbAnnotationNames.ValueGenerationStrategy] = FbValueGenerationStrategy.SequenceTrigger,
						},
						new AddColumnOperation
						{
							Name = "EmployerId",
							Table = "People",
							ClrType = typeof(int),
							IsNullable = true,
						},
						new AddColumnOperation
						{
							Name = "SSN",
							Table = "People",
							ClrType = typeof(string),
							ColumnType = "char(11)",
							IsNullable = true,
						},
						new AddColumnOperation
						{
							Name = "DEF_O",
							Table = "People",
							ClrType = typeof(string),
							MaxLength = 20,
							DefaultValue = "test",
							IsNullable = true,
						},
						new AddColumnOperation
						{
							Name = "DEF_S",
							Table = "People",
							ClrType = typeof(string),
							MaxLength = 20,
							DefaultValueSql = "''",
							IsNullable = true,
						},
				},
				PrimaryKey = new AddPrimaryKeyOperation
				{
					Columns = new[] { "Id" },
				},
				UniqueConstraints =
				{
						new AddUniqueConstraintOperation
						{
							Columns = new[] { "SSN" },
						},
				},
				ForeignKeys =
				{
						new AddForeignKeyOperation
						{
							Columns = new[] { "EmployerId" },
							PrincipalTable = "Companies",
							PrincipalColumns = new[] { "Id" },
						},
				},
			};
			var expectedCreateTable = @"CREATE TABLE ""People"" (
    ""Id"" INTEGER NOT NULL,
    ""Id_Identity"" INTEGER GENERATED BY DEFAULT AS IDENTITY NOT NULL,
    ""Id_Sequence"" INTEGER NOT NULL,
    ""EmployerId"" INTEGER,
    ""SSN"" char(11),
    ""DEF_O"" VARCHAR(20) DEFAULT _UTF8'test',
    ""DEF_S"" VARCHAR(20) DEFAULT '',
    PRIMARY KEY (""Id""),
    UNIQUE (""SSN""),
    FOREIGN KEY (""EmployerId"") REFERENCES ""Companies"" (""Id"")
)";
			var batch = Generate(new[] { operation });
			Assert.AreEqual(3, batch.Count());
			StringAssert.StartsWith(expectedCreateTable, batch[0].CommandText);
			StringAssert.Contains("rdb$generator_name = ", batch[1].CommandText);
			StringAssert.StartsWith("CREATE TRIGGER ", batch[2].CommandText);
		}

		[Test]
		public void DropTableOperation()
		{
			var operation = new DropTableOperation()
			{
				Name = "People",
			};
			var batch = Generate(new[] { operation });
			Assert.AreEqual(1, batch.Count());
			StringAssert.StartsWith(@"DROP TABLE ""People""", batch[0].CommandText);
		}

		IReadOnlyList<MigrationCommand> Generate(IReadOnlyList<MigrationOperation> operations)
		{
			using (var db = GetDbContext<FbTestDbContext>())
			{
				var generator = db.GetService<IMigrationsSqlGenerator>();
				return generator.Generate(operations, db.Model);
			}
		}
	}
}
