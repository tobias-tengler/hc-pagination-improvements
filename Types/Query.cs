using HotChocolate.Types.Pagination;

namespace hc_pagination_improvements.Types;

[QueryType]
public static class Query
{
    [UsePaging]
    public static List<Book> GetBooks()
    {
        Console.WriteLine("Fetching books");

        return new() {
            new Book("Book 1", new Author("Tobias")),
            new Book("Book 2", new Author("Martin")),
            new Book("Book 3", new Author("Daniel")),
        };
    }

    [UsePaging]
    public static Connection<Author> GetAuthors()
    {
        Console.WriteLine("Fetching authors");

        List<Author> authors = new() {
            new Author("Tobias"),
            new Author("Martin"),
            new Author("Daniel"),
        };

        var edges = authors.Select(author => new Edge<Author>(author, "+" + author.Name)).ToArray();

        var pageInfo = new ConnectionPageInfo(false, false, "+Tobias", "+Daniel");

        var connection = new Connection<Author>(edges, pageInfo);

        return connection;
    }
}


[ExtendObjectType("BooksConnection")]
public class BooksConnectionExtensions
{
    public int GetTotalCount()
    {
        Console.WriteLine("Fetching books.totalCount");

        return 3;
    }
}

[ExtendObjectType("AuthorsConnection")]
public class AuthorsConnectionExtensions
{
    public int GetTotalCount()
    {
        Console.WriteLine("Fetching authors.totalCount");

        return 3;
    }
}