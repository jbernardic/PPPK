using System.Reflection;
using ORM.Schema;

if (args.Length == 0)
{
    Console.WriteLine("Usage: ORM <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  migrate --connection <connection-string>  Create tables from models");
    return;
}

var command = args[0];

switch (command)
{
    case "migrate":
        var connectionIndex = Array.IndexOf(args, "--connection");
        if (connectionIndex == -1 || connectionIndex + 1 >= args.Length)
        {
            Console.WriteLine("Error: --connection <connection-string> is required");
            return;
        }
        var connectionString = args[connectionIndex + 1];
        var generator = new SchemaGenerator(connectionString);
        generator.ExecuteMigration(Assembly.GetExecutingAssembly());
        break;
    default:
        Console.WriteLine($"Unknown command: {command}");
        break;
}
