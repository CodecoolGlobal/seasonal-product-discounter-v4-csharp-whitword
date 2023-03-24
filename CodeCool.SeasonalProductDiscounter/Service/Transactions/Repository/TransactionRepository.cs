using System.Data;
using CodeCool.SeasonalProductDiscounter.Model.Products;
using CodeCool.SeasonalProductDiscounter.Model.Transactions;
using CodeCool.SeasonalProductDiscounter.Model.Users;
using CodeCool.SeasonalProductDiscounter.Service.Logger;
using CodeCool.SeasonalProductDiscounter.Service.Persistence;
using CodeCool.SeasonalProductDiscounter.Service.Products.Repository;
using CodeCool.SeasonalProductDiscounter.Service.Users;
using CodeCool.SeasonalProductDiscounter.Utilities;
using Microsoft.Data.Sqlite;

namespace CodeCool.SeasonalProductDiscounter.Service.Transactions.Repository;

public class TransactionRepository : SqLiteConnector, ITransactionRepository
{
    private readonly string _tableName;
    private static string _dbFile;
    private static ILogger _logger;


    public TransactionRepository(string dbFile, ILogger logger) : base(dbFile, logger)
    {
        _tableName = DatabaseManager.TransactionsTableName;
        _dbFile = dbFile;
        _logger = logger;
    }

    public bool Add(Transaction transaction)
    {
        var query = @$"INSERT INTO {_tableName} (date, user_id, product_id, price_paid) 
        VALUES('{transaction.Date}',
       '{transaction.User.Id}',
       '{transaction.Product.Id}',
       '{transaction.PricePaid}')";
        
        return ExecuteNonQuery(query);
    }
    
    public IEnumerable<Transaction> GetAll()
    {
        var products = DatabaseManager.ProductsTableName;
        var users = DatabaseManager.UsersTableName;
        
        var query = @$"SELECT * FROM {_tableName} INNER JOIN {products} ON {products}.p_id = {_tableName}.product_id INNER JOIN {users} ON u_id = user_id";

        try
        {
            using var connection = GetPhysicalDbConnection();
            using var command = GetCommand(query, connection);
            using var reader = command.ExecuteReader();
            Logger.LogInfo($"{GetType().Name} executing query: {query}");


            var dt = new DataTable();

            //This is required otherwise the DataTable tries to force the DB constrains on the result set, which can cause problems in some cases (e.g. UNIQUE)
            using var ds = new DataSet { EnforceConstraints = false };
            ds.Tables.Add(dt);
            dt.Load(reader);
            ds.Tables.Remove(dt);

            var lst = new List<Transaction>();
            foreach (DataRow row in dt.Rows)
            {
                var user = ToUser(row);
                var product = ToProduct(row);
                var transaction = ToTransaction(row, user, product);

                lst.Add(transaction);
            }

            return lst;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static User ToUser(DataRow row)
    {
        var userId = TypeConverters.ToInt(row["user_id"]);
        var userName = TypeConverters.ToString(row["user_name"]);
        var userPassword = TypeConverters.ToString(row["password"]);
        var user = new User(userId, userName, userPassword);
        return user;
    }

    private static Product ToProduct(DataRow row)
    {
        var productId = uint.Parse(TypeConverters.ToString(row["product_id"]));
        var productName = TypeConverters.ToString(row["product_name"]);
        var productColor = TypeConverters.GetColorEnum(TypeConverters.ToString(row["color"]));
        var productSeason = TypeConverters.GetSeasonEnum(TypeConverters.ToString(row["season"]));
        var price = TypeConverters.ToDouble(row["price"]);
        var sold = bool.Parse(TypeConverters.ToString(row["sold"]));
        var product = new Product(productId, productName, productColor, productSeason, price, sold);
        return product;

    }

    private static Transaction ToTransaction(DataRow row, User user, Product product)
    {
        var id = TypeConverters.ToInt(row["t_id"]);
        var date = TypeConverters.ToDateTime(TypeConverters.ToString(row["date"]));
        var pricePaid = TypeConverters.ToDouble(row["price_paid"]);
        Transaction transaction = new Transaction(id, date, user, product, pricePaid);
        return transaction;
    }
}
