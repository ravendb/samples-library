using System;
using System.Data.Common;
using Raven.Client.Documents;

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

// var logger = new ServiceCollection()
//     .AddLogging((loggingBuilder) => loggingBuilder
//         .SetMinimumLevel(LogLevel.Information)
//         .AddConsole()
//     )
//     .BuildServiceProvider()
//     .GetRequiredService<ILoggerFactory>()
//     .CreateLogger<MigrationRunner>();
//

// new MigrationRunner(store, new MigrationOptions(), NullLogger<MigrationRunner>.Instance).Run();