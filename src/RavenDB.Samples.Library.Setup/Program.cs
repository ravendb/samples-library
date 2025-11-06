using System.Data.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Raven.Client.Documents;
using Raven.Migrations;

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__library");
var connectionBuilder = new DbConnectionStringBuilder
{
    ConnectionString = connectionString
};

string[] urls = [(string)connectionBuilder["URL"]];
string database = (string)connectionBuilder["Database"];

using var store = new DocumentStore();

store.Urls = urls;
store.Database = database;

store.Initialize();

new MigrationRunner(store, new MigrationOptions(), NullLogger<MigrationRunner>.Instance).Run();