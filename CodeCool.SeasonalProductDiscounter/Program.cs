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
        ILogger logger = null;
        string dbFile = WorkDir + "\\Resources\\SeasonalProductDiscounter.db";

        IDatabaseManager dbManager = null;

        dbManager.CreateTables();

        IProductRepository productRepository = null;
        IDiscountRepository discountRepository = null;
        IUserRepository userRepository = null;
        ITransactionRepository transactionRepository = null;
        IAuthenticationService authenticationService = null;
        IDiscounterService discounterService = null;

        InitializeDatabase(productRepository);

        var simulator = new TransactionsSimulator(logger, userRepository, productRepository,
            authenticationService, discounterService, transactionRepository);


        //RunSimulation(simulator, productRepository, transactionRepository);

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    private static void InitializeDatabase(IProductRepository productRepository)
    {
        if (!productRepository.AvailableProducts.Any())
        {
            var randomProductGenerator = new RandomProductGenerator(1000, 20, 80);
            //add products to repo
        }
    }

    private static void RunSimulation(TransactionsSimulator simulator, IProductRepository productRepository, ITransactionRepository transactionRepository)
    {
        int days = 0;
        var date = DateTime.Today;

        // set your own condition
        while (true)
        {
            Console.WriteLine("Starting simulation...");
            simulator.Run(new TransactionsSimulatorSettings(date, 100, 70));

            var transactions = transactionRepository.GetAll().ToList();

            Console.WriteLine($"{date} ended, total transactions: {transactions.Count}, total income: {transactions.Sum(t => t.PricePaid)}");
            Console.WriteLine($"Products left to sell: {productRepository.AvailableProducts.Count()}");
        }
    }
}
