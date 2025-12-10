# The Library of Ravens

![Build](https://github.com/ravendb/sample-blueprint/actions/workflows/build.yml/badge.svg)

## Overview

A simple library management application. Built with [RavenDB](https://ravendb.net), [Aspire](https://aspire.dev) and [Azure Functions](https://azure.microsoft.com/en-us/products/functions).

<img width="950" height="590" alt="screenshot" src="https://github.com/user-attachments/assets/108cbb63-e937-4b40-9cb0-28123fc93125" />

## Features used

The following RavenDB features are used to build the application:

1. [Include](https://docs.ravendb.net/7.1/client-api/session/loading-entities#load-with-includes) - loading related documents in one request
1. [Document Refresh](https://docs.ravendb.net/7.1/studio/database/settings/document-refresh) - used for timeouts handling
1. [Azure Storage Queues ETL](https://docs.ravendb.net/7.1/server/ongoing-tasks/etl/queue-etl/azure-queue) - used with the `@Refresh` feature to send information about expiring timeouts
1. [Vector Search](https://docs.ravendb.net/7.1/ai-integration/vector-search/ravendb-as-vector-database) - used for searching across similar books

## Technologies

The following technologies were used to build this application:

1. [RavenDB](https://ravendb.net/)
1. [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
1. [Aspire](https://aspire.dev/)
1. [Azure Functions](https://azure.microsoft.com/en-us/products/functions)
1. [Azure Storage Queues](https://azure.microsoft.com/products/storage/queues)
1. [SvelteKit](https://svelte.dev/)

## Run locally

If you want to run the application locally, please follow the steps:

1. Check out the GIT repository
1. Install prerequisites:
   1. [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
   1. [Aspire.dev](https://aspire.dev/get-started/install-cli/)
1. Get the app running by opening `/src/RavenDB.Samples.Library.sln` and starting the `Aspire` `AppHost` project

## Community & Support

If you spot a bug, have an idea or a question, please let us know by raising an issue or creating a pull request. 

We do use a [Discord server](https://discord.gg/ravendb). If you have any doubts, don't hesitate to reach out!

## Contributing

We encourage you to contribute! Please read our [CONTRIBUTING](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed with the [MIT license](LICENSE).
