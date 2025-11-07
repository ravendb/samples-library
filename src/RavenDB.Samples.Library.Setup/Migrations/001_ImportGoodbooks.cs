using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Migrations;
using RavenDB.Samples.Library.Model;

namespace RavenDB.Samples.Library.Setup.Migrations;

[Migration(1)]
public sealed class ImportGoodBooks : Migration
{
    public override void Up()
    {
        var asm = typeof(ImportGoodBooks).Assembly;
        var name = asm.GetManifestResourceNames().Single(name => name.Contains("books.csv"));
        using var stream = asm.GetManifestResourceStream(name);

        var booksCsvPath = Path.Combine(AppContext.BaseDirectory, "Data", "books.csv");

        var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            BadDataFound = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim
        };
        
        using var reader = new StreamReader(stream!);
        using var csv = new CsvReader(reader, csvConfiguration);
        
        if (!csv.Read() || !csv.ReadHeader())
        {
            throw new InvalidOperationException("The GoodBooks dataset does not contain a header row.");
        }
        
        var authorsByName = new Dictionary<string, Author>(StringComparer.OrdinalIgnoreCase);
        var nextAuthorId = 1;
        var random = new Random(42);
        
        using var bulkInsert = DocumentStore.BulkInsert();

        // ID prefixes
        var conventions = DocumentStore.Conventions;
        var collections = new
        {
            books = conventions.GetCollectionName(typeof(Book)),
            authors = conventions.GetCollectionName(typeof(Author)),
            editions = conventions.GetCollectionName(typeof(BookEdition)),
            bookCopies = conventions.GetCollectionName(typeof(BookCopy))
        };

        while (csv.Read())
        {
            var goodreadsBookId = csv.GetField<long>("goodreads_book_id");
            var workId = csv.GetField<long>("work_id");
            var isbn = csv.GetField("isbn");
            var authorsRaw = csv.GetField("authors");
            var title = csv.GetField("title");
            var languageCode = csv.GetField("language_code");
            var imageUrl = csv.GetField("image_url");
            var smallImageUrl = csv.GetField("small_image_url");
        
            var authorFullName = ExtractPrimaryAuthor(authorsRaw);
            var author = GetOrCreateAuthor(authorFullName, authorsByName, collections.authors, ref nextAuthorId, bulkInsert);

            var bookTitle = string.IsNullOrWhiteSpace(title) ? $"Book #{workId}" : title;
        
            var book = new Book
            {
                Id = $"{collections.books}/{workId}",
                Title = bookTitle,
                AuthorId = author.Id
            };
        
            bulkInsert.Store(book);
        
            var edition = new BookEdition
            {
                Id = $"{collections.editions}/{goodreadsBookId}",
                BookId = book.Id,
                Title = bookTitle,
                Isbn = string.IsNullOrWhiteSpace(isbn) ? null : isbn,
                LanguageCode = string.IsNullOrWhiteSpace(languageCode) ? null : languageCode,
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,
                SmallImageUrl = string.IsNullOrWhiteSpace(smallImageUrl) ? null : smallImageUrl
            };
        
            bulkInsert.Store(edition);
        
            var copiesCount = random.Next(1, 6);
            for (var copyNumber = 1; copyNumber <= copiesCount; copyNumber++)
            {
                var copy = new BookCopy
                {
                    Id = $"{collections.bookCopies}/{workId}-{copyNumber}",
                    BookEditionId = edition.Id
                };
        
                bulkInsert.Store(copy);
            }
        }
    }

    public override void Down()
    {
        DocumentStore.Operations.Send(new DeleteByQueryOperation(new IndexQuery { Query = "from Books" }));
        DocumentStore.Operations.Send(new DeleteByQueryOperation(new IndexQuery { Query = "from BookEditions" }));
        DocumentStore.Operations.Send(new DeleteByQueryOperation(new IndexQuery { Query = "from BookCopies" }));
        DocumentStore.Operations.Send(new DeleteByQueryOperation(new IndexQuery { Query = "from Authors" }));
    }

    private static Author GetOrCreateAuthor(
        string fullName,
        IDictionary<string, Author> authorsByName,
        string collectionName,
        ref int nextAuthorId,
        BulkInsertOperation bulkInsert)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = "Unknown Author";
        }

        if (authorsByName.TryGetValue(fullName, out var existing))
        {
            return existing;
        }

        var (firstName, lastName) = SplitAuthorName(fullName);

        var author = new Author
        {
            Id = $"{collectionName}/{nextAuthorId++}",
            FirstName = firstName,
            LastName = lastName
        };

        bulkInsert.Store(author);
        authorsByName[fullName] = author;

        return author;
    }

    private static string ExtractPrimaryAuthor(string? authorsRaw)
    {
        if (string.IsNullOrWhiteSpace(authorsRaw))
        {
            return "Unknown Author";
        }

        var authors = authorsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return authors.Length == 0 ? "Unknown Author" : authors[0];
    }

    private static (string FirstName, string LastName) SplitAuthorName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return (fullName, string.Empty);
        }

        if (parts.Length == 1)
        {
            return (parts[0], string.Empty);
        }

        var firstName = string.Join(' ', parts[..^1]);
        var lastName = parts[^1];

        return (firstName, lastName);
    }

}
