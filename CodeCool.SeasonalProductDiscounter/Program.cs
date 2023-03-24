using CodeCool.SeasonalProductDiscounter.Model.Transactions;
using CodeCool.SeasonalProductDiscounter.Service.Authentication;
using CodeCool.SeasonalProductDiscounter.Service.Discounts;
using CodeCool.SeasonalProductDiscounter.Service.Discounts.Repository;
using CodeCool.SeasonalProductDiscounter.Service.Logger;
using CodeCool.SeasonalProductDiscounter.Service.Persistence;
using CodeCool.SeasonalProductDiscounter.Service.Products.Generator;
using CodeCool.SeasonalProductDiscounter.Service.Products.Repository;
using CodeCool.SeasonalProductDiscounter.Service.Transactions.Repository;
using CodeCool.SeasonalProductDiscounter.Service.Transactions.Simulator;
using CodeCool.SeasonalProductDiscounter.Service.Users;

class Program
{
    private static readonly string WorkDir = AppDomain.CurrentDomain.BaseDirectory;

    public static void Main(string[] args)
    {
        ILogger logger = new ConsoleLogger();
        string dbFile = WorkDir + "\\Resources\\SeasonalProductDiscounter.db";

        IDatabaseManager dbManager = new DatabaseManager(dbFile, logger);

        dbManager.CreateTables();

        IProductRepository productRepository = new ProductRepository(dbFile, logger);
        IDiscountRepository discountRepository = new DiscountRepository();
        IUserRepository userRepository = new UserRepository(dbFile, logger);
        ITransactionRepository transactionRepository = new TransactionRepository(dbFile, logger);
        IAuthenticationService authenticationService = new AuthenticationService(userRepository);
        IDiscounterService discounterService = new DiscounterService(discountRepository);

        InitializeDatabase(productRepository);

        var simulator = new TransactionsSimulator(logger, userRepository, productRepository,
            authenticationService, discounterService, transactionRepository);


        RunSimulation(simulator, productRepository, transactionRepository);

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    private static void InitializeDatabase(IProductRepository productRepository)
    {
        if (!productRepository.AvailableProducts.Any())
        {
            var randomProductGenerator = new RandomProductGenerator(1000, 20, 80);
            //add products to repo
            productRepository.Add(randomProductGenerator.Products);
        }
    }

    private static void RunSimulation(TransactionsSimulator simulator, IProductRepository productRepository, ITransactionRepository transactionRepository)
    {
        int days = 0;
        var date = DateTime.Today;

        // set your own condition
        while (days < 3 && productRepository.AvailableProducts.Any())
        {
            Console.WriteLine("Starting simulation...");
            simulator.Run(new TransactionsSimulatorSettings(date, 100, 70));

            var transactions = transactionRepository.GetAll().ToList();

            Console.WriteLine($"{date} ended, total transactions: {transactions.Count}, total income: {transactions.Sum(t => t.PricePaid)}");
            Console.WriteLine($"Products left to sell: {productRepository.AvailableProducts.Count()}");

            days++;
            date = date.AddDays(1);
        }
    }
}
