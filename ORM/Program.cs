using System.Reflection;
using ORM;
using ORM.Migrations;

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? "Host=localhost;Database=medical_db;Username=postgres;Password=root";

if (args.Length > 0)
{
    var connectionIndex = Array.IndexOf(args, "--connection");
    if (connectionIndex != -1 && connectionIndex + 1 < args.Length)
    {
        connectionString = args[connectionIndex + 1];
    }

    switch (args[0])
    {
        case "migrate":
            var runner = new MigrationRunner(connectionString);
            runner.MigrateUp(Assembly.GetExecutingAssembly());
            return;
        case "migrate:down":
            var downRunner = new MigrationRunner(connectionString);
            downRunner.MigrateDown(Assembly.GetExecutingAssembly());
            return;
        case "migrate:status":
            var statusRunner = new MigrationRunner(connectionString);
            statusRunner.ShowStatus(Assembly.GetExecutingAssembly());
            return;
    }
}

var app = new MedicalApp(connectionString);
app.Run();
