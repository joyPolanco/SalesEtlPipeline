using Microsoft.EntityFrameworkCore;
using proceso_etl.Models;
using System.Text;

namespace proceso_etl.Service
{
    public class ResultsHelper
    {
        private readonly SalesDBContext _context;

        public ResultsHelper(SalesDBContext context)
        {
            _context = context;
        }

        // =====================================
        // PRODUCTO - CATEGORÍA
        // =====================================
        public async Task<string> PrintProductsProcess()
        {
            var sb = new StringBuilder();

            var totalCategories = await _context.Categories.CountAsync();
            var totalProducts = await _context.Products.CountAsync();

            sb.AppendLine("=== PRODUCTO - CATEGORÍA ===");
            sb.AppendLine($"Categorías: {totalCategories}");
            sb.AppendLine($"Productos: {totalProducts}");

            return sb.ToString();
        }

        // =====================================
        // CLIENTE - PAÍS - CIUDAD - TELÉFONO
        // =====================================
        public async Task<string> PrintClientsProcess()
        {
            var sb = new StringBuilder();

            var totalCountries = await _context.Countries.CountAsync();
            var totalCities = await _context.Cities.CountAsync();
            var totalClients = await _context.Customers.CountAsync();
            var totalPhones = await _context.Phones.CountAsync();

            sb.AppendLine("=== CLIENTE - PAÍS - CIUDAD - TELÉFONO ===");
            sb.AppendLine($"Países: {totalCountries}");
            sb.AppendLine($"Ciudades: {totalCities}");
            sb.AppendLine($"Clientes: {totalClients}");
            sb.AppendLine($"Teléfonos: {totalPhones}");

            return sb.ToString();
        }

        // =====================================
        // ORDER - ORDER STATUS
        // =====================================
        public async Task<string> PrintOrdersProcess()
        {
            var sb = new StringBuilder();

            var totalStatuses = await _context.OrderStatuses.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();

            sb.AppendLine("=== ORDER - ORDER STATUS ===");
            sb.AppendLine($"Estados: {totalStatuses}");
            sb.AppendLine($"Órdenes: {totalOrders}");

            return sb.ToString();
        }

        // =====================================
        // ORDER - ORDER DETAILS
        // =====================================
        public async Task<string> PrintOrderDetailsProcess()
        {
            var sb = new StringBuilder();

            var totalDetails = await _context.OrderDetails.CountAsync();

            var totalSales = await _context.OrderDetails
                .SumAsync(x => x.TotalPrice) ?? 0;

            sb.AppendLine("=== ORDER - ORDER DETAILS ===");
            sb.AppendLine($"Detalles: {totalDetails}");
            sb.AppendLine($"Ventas Totales: {totalSales}");

            return sb.ToString();
        }
    }
}