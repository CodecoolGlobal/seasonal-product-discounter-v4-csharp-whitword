using CodeCool.SeasonalProductDiscounter.Service.Logger;

namespace CodeCool.SeasonalProductDiscounter.Service.Persistence;

public class DatabaseManager : SqLiteConnector, IDatabaseManager
{
    #region Tables

    public const string ProductsTableName = "products";
    public const string UsersTableName = "users";
    public const string TransactionsTableName = "transactions";

    private const string ProductsTableStatement = 
        @$"CREATE TABLE IF NOT EXISTS {ProductsTableName} (
	                p_id INTEGER PRIMARY KEY,
	                product_name TEXT NOT NULL,
	                color TEXT NOT NULL,
	                season TEXT NOT NULL,
	                price REAL NOT NULL,
	                sold TEXT NOT NULL
        );";

    private const string UsersTableStatement = 
        @$"CREATE TABLE IF NOT EXISTS {UsersTableName} (
	                u_id INTEGER PRIMARY KEY,
	                user_name TEXT NOT NULL UNIQUE,
	                password TEXT NOT NULL
        );";

    private const string TransactionsTableStatement =
        @$"CREATE TABLE IF NOT EXISTS {TransactionsTableName} (
	                t_id INTEGER PRIMARY KEY,
	                date TEXT NOT NULL,
	                user_id INTEGER NOT NULL,
	                product_id INTEGER NOT NULL,
	                price_paid REAL NOT NULL
        );";

    #endregion

    private readonly string[] _tableStatements;

    public DatabaseManager(string dbFile, ILogger logger) : base(dbFile, logger)
    {
        _tableStatements = new[]
        {
            ProductsTableStatement,
            UsersTableStatement,
            TransactionsTableStatement,
        };
    }

    public bool CreateTables()
    {
        return ExecuteNonQuery(_tableStatements);
    }
}
