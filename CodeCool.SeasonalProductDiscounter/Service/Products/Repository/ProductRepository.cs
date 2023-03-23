using System.Data;
using System.Globalization;
using System.Text;
using CodeCool.SeasonalProductDiscounter.Model.Products;
using CodeCool.SeasonalProductDiscounter.Service.Logger;
using CodeCool.SeasonalProductDiscounter.Service.Persistence;
using CodeCool.SeasonalProductDiscounter.Utilities;
using Microsoft.Data.Sqlite;

namespace CodeCool.SeasonalProductDiscounter.Service.Products.Repository;

public class ProductRepository : SqLiteConnector, IProductRepository
{
    private readonly string _tableName;

    public IEnumerable<Product> AvailableProducts => GetAvailableProducts();

    public ProductRepository(string dbFile, ILogger logger) : base(dbFile, logger)
    {
        _tableName = DatabaseManager.ProductsTableName;
    }

    private IEnumerable<Product> GetAvailableProducts()
    {
        var query = @$"SELECT * FROM {_tableName}";
        var ret = new List<Product>();

        try
        {
            using var connection = GetPhysicalDbConnection();
            using var command = GetCommand(query, connection);
            using var reader = command.ExecuteReader();
            Logger.LogInfo($"{GetType().Name} executing query: {query}");

            while (reader.Read())
            {
                IFormatProvider formatProvider = new NumberFormatInfo();
                var product = new Product(uint.Parse(reader.GetString(0)), reader.GetString(1),
                    TypeConverters.GetColorEnum(reader.GetString(2)), TypeConverters.GetSeasonEnum(reader.GetString(3)),
                    TypeConverters.ToDouble(reader.GetDecimal(4)), reader.GetBoolean(5));
                ret.Add(product);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
            throw;
        }

        return ret;
    }

    public bool Add(IEnumerable<Product> products)
    {
        var query = @$"INSERT INTO {_tableName} (`product_name`, `color`, `season`, `price`, `sold`) VALUES(@productName, @color, @season, @price, @sold)";
        foreach(var product in products)
        {
        try
        {
            using var connection = GetPhysicalDbConnection();
            using var command = GetCommand(query, connection);
            using var reader = command.ExecuteReader();
            SqliteCommand myCommand = new SqliteCommand(query, connection);
            myCommand.Parameters.AddWithValue("@productName", product.Name);
            myCommand.Parameters.AddWithValue("@color", TypeConverters.ToString(product.Color));
            myCommand.Parameters.AddWithValue("@season", TypeConverters.ToString(product.Season));
            myCommand.Parameters.AddWithValue("@price", product.Price);
            myCommand.Parameters.AddWithValue("@sold", product.Sold);
            myCommand.ExecuteNonQuery();
            //connection.Close();

        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
            throw;
        }
        }
        return false;
    }

    public bool SetProductAsSold(Product product)
    {
        //Set the sold field in the database
        var query = @$"UPDATE {_tableName} SET sold = {product.Sold} WHERE id = {product.Id}";
        return ExecuteNonQuery(query);
    }
}
