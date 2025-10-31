# Raven Library

![Build](https://github.com/ravendb/sample-blueprint/actions/workflows/build.yml/badge.svg)

## Overview

A simple library management application. Built with [RavenDB](https://ravendb.net), Aspire and Azure Functions.

<img width="950" height="590" alt="screenshot" src="https://github.com/user-attachments/assets/108cbb63-e937-4b40-9cb0-28123fc93125" />

## Live Demo

A live hosted version of this application can be found here:

**[https://library.samples.ravendb.net](https://library.samples.ravendb.net)**

Please bear in mind, this application, for sake of simplicity of the deployment, is deployed in Americas region. This can impact the latency you perceive.

We do clean the environment from time to time.

## Features used

The following RavenDB features are used to build the application:

1. [Vector Search](https://docs.ravendb.net/7.1/ai-integration/vector-search/ravendb-as-vector-database) - RavenDB has a built-in vector database. It's used for searching across similar books.
1. [Document Refresh](https://docs.ravendb.net/7.1/studio/database/settings/document-refresh) - a scheduled update of selected documents is used to inform users about the book copies that should be returned.
1. [Azure Storage Queues ETL](https://docs.ravendb.net/7.1/server/ongoing-tasks/etl/queue-etl/azure-queue) - Azure Storage Queues integration is used to inform about the potential book copies to be returned.

## Technologies

The following technogies were used to build this application:

1. RavenDB 6.2
1. .NET 9
1. Aspire

## Run locally

If you want to run the application locally, please follow the steps:

1. Check out the GIT repository
1. Install prerequisites:
   1. [.NET 9.x](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
   1. [Aspire.dev](https://aspire.dev/get-started/install-cli/)
1. Get the app running by opening `/scr/RavenDB.Samples.Library.sln` and starting the `Aspire` `AppHost` project

## Community & Support

If you spot a bug, have an idea or a question, please let us know by rasing an issue or creating a pull request. 

We do use a [Discord server](https://discord.gg/ravendb). If you have any doubts, don't hesistate to reach out!

## Contributing

We encourage you to contribute! Please read our [CONTRIBUTING](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed with the [MIT license](LICENSE).
