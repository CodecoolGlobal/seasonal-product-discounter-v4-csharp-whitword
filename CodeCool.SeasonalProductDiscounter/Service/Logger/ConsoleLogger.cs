namespace CodeCool.SeasonalProductDiscounter.Service.Logger;

public class ConsoleLogger: LoggerBase
{
    
    protected override void LogMessage(string message, string type)
    {
        Console.WriteLine(CreateLogEntry(message, type));
    }
}