using Microsoft.EntityFrameworkCore;
using proceso_etl.Configurations;
using proceso_etl.Models;
using proceso_etl.Service;
using System.Configuration; // <-- necesario para App.config

partial class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = ConfigurationManager.ConnectionStrings["SalesDB"].ConnectionString;

        var options = new DbContextOptionsBuilder<SalesDBContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new SalesDBContext(options);

        var loader = new Loader(context);

        // Ejecutar procesos ETL
        await loader.LoadProducts();
        await loader.LoadClients();
        await loader.LoadOrdersAndDetails();

        var resultsHelper = new ResultsHelper(context);
        Console.WriteLine(await resultsHelper.PrintProductsProcess());
        Console.WriteLine(await resultsHelper.PrintClientsProcess());
        Console.WriteLine(await resultsHelper.PrintOrdersProcess());
        Console.WriteLine(await resultsHelper.PrintOrderDetailsProcess());

        Console.WriteLine("Carga completada.");
    }
}