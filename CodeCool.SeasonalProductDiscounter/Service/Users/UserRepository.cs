using System.Data;
using System.Runtime.InteropServices;
using CodeCool.SeasonalProductDiscounter.Model.Users;
using CodeCool.SeasonalProductDiscounter.Service.Logger;
using CodeCool.SeasonalProductDiscounter.Service.Persistence;
using Microsoft.Data.Sqlite;

namespace CodeCool.SeasonalProductDiscounter.Service.Users;

public class UserRepository : SqLiteConnector, IUserRepository
{
    public UserRepository(string dbFile, ILogger logger) : base(dbFile, logger)
    {
    }

    public IEnumerable<User> GetAll()
    {
        var query = @$"SELECT * FROM {DatabaseManager.UsersTableName}";
        var ret = new List<User>();

        try
        {
            using var connection = GetPhysicalDbConnection();
            using var command = GetCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var user = new User(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                ret.Add(user);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
            throw;
        }

        return ret;
    }

    public bool Add(User user)
    {
        var query = @$"INSERT INTO {DatabaseManager.UsersTableName} (`user_name`, `password`) VALUES(@userName, @password)";
        try
        {
            using var connection = GetPhysicalDbConnection();
            using var command = GetCommand(query, connection);
            using var reader = command.ExecuteReader();
            SqliteCommand myCommand = new SqliteCommand(query, connection);
            myCommand.Parameters.AddWithValue("@userName", user.UserName);
            myCommand.Parameters.AddWithValue("@password", user.Password);
            myCommand.ExecuteNonQuery();
            //connection.Close();

        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
            throw;
        }
        return false;
    }

    public User Get(string name)
    {
        var query = @$"SELECT * FROM users WHERE user_name LIKE %{name}%";
        
        try
        {
            using var connection = GetPhysicalDbConnection();
            using var command = GetCommand(query, connection);
            using var reader = command.ExecuteReader();
            var user = new User(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
            return user;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
