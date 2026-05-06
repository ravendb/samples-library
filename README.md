# The Library of Ravens

[![Build](https://github.com/ravendb/samples-library/actions/workflows/build.yml/badge.svg)](https://github.com/ravendb/samples-library/actions/workflows/build.yml)

## Overview

The Library of Ravens **solves the "architecture bloat"** of managing separate databases by **consolidating full-text and vector search into a single database**. Instead of wrestling with data synchronization across multiple platforms, you get a unified search experience that keeps your infrastructure lean and your development cycle fast.

The app demonstrates **robust integration patterns using Azure Storage Queues**. By leveraging RavenDB’s change tracking and ETL services, the system ensures that critical library updates are never lost, providing architectural resilience.

Finally, the app **addresses the "cloud tax" of high egress fees** by utilizing ETags and native HTTP caching driven by RavenDB’s metadata. This approach significantly reduces the volume of data sent over the wire, slashing public cloud billing while providing a snappier, more responsive experience for the end user through efficient data reuse.

Built with [RavenDB](https://ravendb.net), [Aspire](https://aspire.dev), [Azure Storage Queues](azure.microsoft.com/en-us/products/storage/queues), and [Azure Functions](https://azure.microsoft.com/en-us/products/functions).

<img width="2019" height="1606" alt="image" src="https://github.com/user-attachments/assets/13e86383-c7f6-4e0e-bb55-2966d51dea9e" />

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
   2. [nodejs](https://nodejs.org)
1. Request a [dev license](https://ravendb.net/license/request/dev-ai-agent-inside)
1. Get the app running by opening `/src/RavenDB.Samples.Library.sln` and starting the `Aspire` `AppHost` project
1. When prompted, provide the dev license as the `ravendb-license` parameter (formatted as JSON)

## Community & Support

If you spot a bug, have an idea or a question, please let us know by raising an issue or creating a pull request. 

We do use a [Discord server](https://discord.gg/ravendb). If you have any doubts, don't hesitate to reach out!

## Contributing

We encourage you to contribute! Please read our [CONTRIBUTING](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed with the [MIT license](LICENSE).
