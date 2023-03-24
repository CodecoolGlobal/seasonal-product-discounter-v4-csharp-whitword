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
       foreach (var product in products)
        {
            var query = @$"INSERT INTO {_tableName} (product_name, color, season, price, sold) 
                VALUES('{product.Name}',
               '{TypeConverters.ToString(product.Color)}',
               '{TypeConverters.ToString(product.Season)}',
                '{product.Price}',
               '{product.Sold}')";

            return ExecuteNonQuery(query);
        }
        return false;
    }

    public bool SetProductAsSold(Product product)
    {
        //Set the sold field in the database
        var query = @$"UPDATE {_tableName} SET sold = {product.Sold} WHERE p_id = {product.Id}";
        return ExecuteNonQuery(query);
    }
}
