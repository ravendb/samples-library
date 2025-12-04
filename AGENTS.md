# RavenDB Samples Library - Project Knowledge

## Project Overview

This is a library management application showcasing RavenDB features, built with:
- **Backend**: .NET 10 + Azure Functions + RavenDB 7.1
- **Frontend**: SvelteKit 2 + TypeScript + Vite
- **Orchestration**: .NET Aspire 13.0
- **Data**: Goodbooks-10k dataset (git submodule)

The app demonstrates:
- Vector Search for finding similar books
- Document Refresh for timeout handling
- Azure Storage Queues ETL for notifications
- Aspire for local development orchestration

## Architecture

### Solution Structure

```
src/
├── RavenDB.Samples.Library.AppHost/     # Aspire orchestration host
├── RavenDB.Samples.Library.App/         # Azure Functions API backend
├── RavenDB.Samples.Library.Frontend/    # SvelteKit web frontend
├── RavenDB.Samples.Library.Model/       # Shared data models & indexes
├── RavenDB.Samples.Library.Setup/       # Database migrations
└── RavenDB.Samples.Library.ServiceDefaults/  # Aspire service defaults
```

### Key Components

**AppHost (AppHost.cs)**:
- Configures RavenDB server and database ("library")
- Sets up Azure Storage emulator for queues
- Orchestrates Functions app and npm frontend
- Defines migration command with HTTP trigger
- Uses Aspire service discovery

**API Backend (RavenDB.Samples.Library.App)**:
- Azure Functions with isolated worker model (.NET 10)
- HTTP triggers for RESTful endpoints
- Key endpoints:
  - `GET /api/books/{id}` - Fetch book with author (uses Include)
  - `GET /api/authors/{id}` - Fetch author with books (uses LazilyAsync)
  - `GET /api/search` - Full-text search via GlobalSearchIndex
  - `GET /api/home/books` - Random books for homepage
  - `GET /api/user/profile` - User borrowed books
  - `POST /api/user/borrow` - Borrow a book
  - `POST /api/user/return` - Return a book
  - `POST /api/migrate` - Run database migrations (admin command)
- Uses RavenDB AsyncDocumentSession for all operations
- Implements HTTP caching via ETag and Last-Modified headers
- CORS enabled for local development

**Data Models**:
- `Book` - Book metadata (title, author reference)
- `Author` - Author info (first/last name)
- `BookEdition` - Specific edition of a book
- `BookCopy` - Physical copy available for borrowing
- `User` - Library user
- `UserBook` - Junction for borrowed books
- All models implement `IRoot` and use static `BuildId()` methods

**RavenDB Indexes**:
- `GlobalSearchIndex` - Multi-map index for searching books and authors
- `BooksByAuthor` - Books grouped by author
- `BorrowedBooksByUserIdIndex` - User's borrowed books

**Frontend (SvelteKit)**:
- Routes:
  - `/` - Homepage with random books
  - `/books/{id}` - Book details
  - `/authors/{id}` - Author details with books list
  - `/profile` - User profile with borrowed books
- Services layer (`lib/services/`) for API calls
- Shared components (`lib/components/`):
  - `BookCard.svelte` - Book display card
  - `SearchModal.svelte` - Search overlay
  - `TipBox.svelte` - Helpful tips display
- User ID stored in localStorage and sent via `X-User-Id` header

**Migrations (RavenDB.Samples.Library.Setup)**:
- Migration 001: Import Goodbooks-10k dataset from CSV
  - Creates authors, books, editions, and copies
  - Uses RavenDB bulk insert for performance
  - CSV embedded as resource from goodbooks submodule
- Migration 002: Create indexes
  - Deploys all custom indexes to database

## Technology Stack

### Backend
- .NET 10
- Azure Functions Worker 2.51.0
- RavenDB.Client 7.1.4
- RavenMigrations 6.0.2
- Aspire.Hosting 13.0.1
- CommunityToolkit.Aspire.RavenDB 13.0.0

### Frontend
- Node.js 22.x
- SvelteKit 2.49.0
- Svelte 5.45.2
- TypeScript 5.9.3
- Vite 7.2.6
- Vitest 4.0.14 (testing)
- Playwright 1.57.0 (browser testing)
- ESLint 9.39.1 + Prettier 3.7.3

### Infrastructure
- .NET Aspire 13.0.1
- Azure Storage (Emulator for local dev)
- RavenDB 6.2+ (via Aspire community toolkit)

## Build & Development

### Prerequisites
- .NET 10 SDK ([download](https://dotnet.microsoft.com/download/dotnet/10.0))
- Node.js 22.x ([download](https://nodejs.org/))
- Aspire CLI: `dotnet workload install aspire`

### First-time Setup

1. **Clone repository with submodules**:
   ```bash
   git clone --recursive https://github.com/ravendb/samples-library.git
   cd samples-library
   ```
   
   Or if already cloned:
   ```bash
   git submodule update --init --recursive
   ```

2. **Restore dependencies**:
   ```bash
   cd src
   dotnet restore RavenDB.Samples.Library.sln
   ```

3. **Frontend dependencies**:
   ```bash
   cd RavenDB.Samples.Library.Frontend
   npm install
   cd ..
   ```

### Running Locally

**Start the application** via Aspire:
```bash
cd src/RavenDB.Samples.Library.AppHost
dotnet run
```

This will:
1. Start RavenDB server
2. Launch Azure Storage Emulator
3. Start the Functions backend
4. Start the Svelte frontend dev server
5. Open Aspire dashboard (usually at http://localhost:15000)

**Access the application**:
- Frontend: Check Aspire dashboard for assigned port
- Backend API: Check Aspire dashboard for Functions endpoint
- RavenDB Studio: Check Aspire dashboard for RavenDB management URL

**Run database migrations**:
- In Aspire dashboard, use the "Migrate DB" command button
- Or via HTTP: `POST /api/migrate` with header `X-Command-Key: <your-secret>`

### Build Commands

**Backend build**:
```bash
cd src
dotnet build RavenDB.Samples.Library.sln
```

**Frontend build**:
```bash
cd src/RavenDB.Samples.Library.Frontend
npm run build
```

**Frontend linting**:
```bash
npm run lint
npm run format  # auto-fix with Prettier
```

**Frontend testing**:
```bash
npm test        # run tests once
npm run test:unit  # run tests in watch mode
```

## Development Patterns

### RavenDB Best Practices Used

1. **Include Pattern** - Avoid N+1 queries:
   ```csharp
   var book = await session
       .Include<Book>(b => b.AuthorId)
       .LoadAsync<Book>(id);
   var author = await session.LoadAsync<Author>(book.AuthorId); // No DB call!
   ```

2. **Lazy Loading** - Batch multiple queries:
   ```csharp
   var lazyBooks = session.Query<Book>()
       .Where(b => b.AuthorId == authorId)
       .LazilyAsync();
   var author = await session.LoadAsync<Author>(authorId);
   var books = await lazyBooks.Value; // Both requests sent together
   ```

3. **Bulk Insert** - Efficient data import:
   ```csharp
   using var bulkInsert = DocumentStore.BulkInsert();
   bulkInsert.Store(entity);
   ```

4. **Static Indexes** - Predefined for performance:
   ```csharp
   public class BooksByAuthor : AbstractIndexCreationTask<Book>
   {
       // Index definition
   }
   ```

5. **HTTP Caching** - ETag-based caching:
   ```csharp
   return req.TryCachePublicly(result, session, entities...);
   ```

### Code Conventions

**C# (Backend)**:
- Use top-level statements where appropriate
- Primary constructors for dependency injection
- Static methods for ID builders: `Book.BuildId(id)`
- Async/await for all I/O operations
- Nullable reference types enabled

**TypeScript/Svelte (Frontend)**:
- Svelte 5 runes syntax (`$state`, `$derived`, etc.)
- Services pattern for API calls
- Type-safe API responses
- ESLint + Prettier for code formatting

### File Naming Conventions

**Backend**:
- API endpoints: `Api.{Entity}.cs` (e.g., `Api.User.cs`)
- Models: `{EntityName}.cs` in Model project
- Indexes: `{Name}Index.cs` in `Model/Indexes/`
- Migrations: `{Number}_{Description}.cs`

**Frontend**:
- Routes: `+page.svelte`, `+layout.svelte`
- Components: `{ComponentName}.svelte`
- Services: `{entity}.ts` in `lib/services/`
- Tests: `{filename}.spec.ts`

## Testing

### Backend Tests
- No unit tests currently in solution
- Testing done via:
  - Aspire dashboard for integration testing
  - Manual API testing via frontend
  - Migration validation

### Frontend Tests
- Unit tests with Vitest
- Component tests with `@vitest/browser` + Playwright
- Run: `npm test` or `npm run test:unit`
- Test files: `*.spec.ts` next to source files

## Common Issues & Solutions

### Build Fails: "Could not find file 'books.csv'"
**Cause**: Goodbooks submodule not initialized
**Solution**: 
```bash
git submodule update --init --recursive
```

### RavenDB Connection Errors
**Cause**: RavenDB server not started or wrong connection string
**Solution**: 
- Ensure Aspire AppHost is running
- Check Aspire dashboard for RavenDB status
- Verify connection string in `appsettings.json`

### Frontend Won't Start
**Cause**: Dependencies not installed
**Solution**:
```bash
cd src/RavenDB.Samples.Library.Frontend
npm install
```

### Migration Already Ran Error
**Cause**: Database already has data from previous run
**Solution**: Delete RavenDB data directory or use new database name

### Port Conflicts
**Cause**: Aspire-assigned ports already in use
**Solution**: Stop other services or restart Aspire (ports are dynamic)

## CI/CD

**GitHub Actions** (`.github/workflows/build.yml`):
- Triggers on push/PR to `main` branch
- Two jobs:
  1. **backend**: Build .NET solution
  2. **frontend**: Build SvelteKit app (currently incomplete in workflow)

**Environment Variables**:
- `NODE_VERSION`: 22.x
- `DOTNET_VERSION`: 10.0.x

## External Resources

- [RavenDB Documentation](https://docs.ravendb.net/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [SvelteKit Documentation](https://kit.svelte.dev/)
- [Azure Functions Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)
- [Goodbooks-10k Dataset](https://github.com/zygmuntz/goodbooks-10k)

## Quick Reference Commands

```bash
# Clone with submodules
git clone --recursive https://github.com/ravendb/samples-library.git

# Or init submodules after clone
git submodule update --init --recursive

# Restore .NET dependencies
cd src && dotnet restore

# Build backend
dotnet build src/RavenDB.Samples.Library.sln

# Install frontend dependencies
cd src/RavenDB.Samples.Library.Frontend && npm install

# Run application (from src/RavenDB.Samples.Library.AppHost)
dotnet run

# Format frontend code
cd src/RavenDB.Samples.Library.Frontend && npm run format

# Lint frontend
npm run lint

# Test frontend
npm test

# Check git status (disable pager)
git --no-pager status

# View changes (disable pager)
git --no-pager diff
```

## Project History & Context

This is a sample/demo application designed to showcase RavenDB features in a real-world scenario. It's intended as a learning resource and reference implementation for developers exploring RavenDB with modern .NET and web technologies.

The application uses the Goodbooks-10k dataset, which contains 10,000 books from Goodreads, to provide realistic data for demonstration purposes.

Key learning goals:
- Understanding RavenDB's document model
- Implementing efficient query patterns (Include, Lazy)
- Using RavenDB indexes for search and filtering
- Integrating RavenDB with modern .NET stack
- Building full-stack applications with Aspire orchestration
