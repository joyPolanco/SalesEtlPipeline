using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using proceso_etl.Configurations;
using proceso_etl.Models;
using proceso_etl.Models.Csv;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proceso_etl.Service
{
    public class Loader
    {
        private readonly SalesDBContext _context;
        static string  basePath = AppContext.BaseDirectory;
        static string inputPath = Path.Combine(basePath, "Data", "Input");

        public Loader(SalesDBContext salesDBContext)
        {
            _context = salesDBContext;
        }


        public async Task LoadProducts()

        {

            //Load categories

            var ProductCsvReader = new CsvReader<ProductCsvModel>();

            var productsCsvList = ProductCsvReader.Extract(Path.Combine(inputPath, "products.csv"));

            var categoriasArchivo = productsCsvList
            .Select(p => p.Category.Trim())
            .Distinct()
            .ToList();

            var categoriasExistentes = await _context.Categories
                .Where(c => categoriasArchivo.Contains(c.Name))
                .ToListAsync();
            var nombresExistentes = categoriasExistentes
            .Select(c => c.Name)
            .ToHashSet();

            var nuevasCategorias = categoriasArchivo
              .Where(nombre => !nombresExistentes.Contains(nombre))
              .Select(nombre => new Category { Name = nombre })
              .ToList();



            //Insertar las categorias
            if (nuevasCategorias.Any())
            {
                await _context.BulkInsertAsync(nuevasCategorias, options =>
                {
                    options.SetOutputIdentity = true;

                });
            }


            var todasCategorias = await _context.Categories
                .ToListAsync();

            var diccionarioCategorias = await _context.Categories
         .Where(c => categoriasArchivo.Contains(c.Name))
         .ToDictionaryAsync(c => c.Name, c => c.CategoryId);

            //Crear lista de productos
            var productsFinal = productsCsvList.Select(p => new Product
            {
                ProductName = p.ProductName,
                Price = p.Price,
                CategoryId = diccionarioCategorias[p.Category]
            }).ToList();

            //Insertar los productos 
            await _context.BulkInsertAsync(productsFinal);

        }


        public async Task LoadClients()
        {
            var customerCsvReader = new CsvReader<CustomerCsvModel>();
            var customerCsvs = customerCsvReader
                .Extract(Path.Combine(inputPath, "customers.csv"));

            // =============================
            // 1️⃣ PROCESAR PAISES
            // =============================

            var countriesCsv = customerCsvs
                .Select(p => p.Country.Trim())
                .Distinct()
                .ToList();

            var existingCountryNames = await _context.Countries
                .Where(c => countriesCsv.Contains(c.CountryName))
                .Select(c => c.CountryName)
                .ToHashSetAsync();

            var newCountries = countriesCsv
                .Where(name => !existingCountryNames.Contains(name))
                .Select(name => new Country { CountryName = name })
                .ToList();

            if (newCountries.Any())
                await _context.BulkInsertAsync(newCountries);

            var countryDictionary = await _context.Countries
                .Where(c => countriesCsv.Contains(c.CountryName))
                .ToDictionaryAsync(c => c.CountryName, c => c.CountryId);

            // =============================
            // 2️⃣ PROCESAR CIUDADES
            // =============================

            var citiesCsv = customerCsvs
                .Select(p => new
                {
                    City = p.City.Trim(),
                    CountryId = countryDictionary[p.Country.Trim()]
                })
                .Distinct()
                .ToList();

            var cityKeys = citiesCsv
                .Select(x => $"{x.City}|{x.CountryId}")
                .ToList();

            var existingCityKeys = await _context.Cities
                .Where(c => cityKeys.Contains(c.CityName + "|" + c.CountryId))
                .Select(c => c.CityName + "|" + c.CountryId)
                .ToHashSetAsync();

            var newCities = citiesCsv
                .Where(x => !existingCityKeys.Contains($"{x.City}|{x.CountryId}"))
                .Select(x => new City
                {
                    CityName = x.City,
                    CountryId = x.CountryId
                })
                .ToList();

            if (newCities.Any())
                await _context.BulkInsertAsync(newCities);

            var cityDictionary = await _context.Cities
                .Where(c => cityKeys.Contains(c.CityName + "|" + c.CountryId))
                .ToDictionaryAsync(
                    c => (c.CityName, c.CountryId),
                    c => c.CityId
                );

            // =============================
            // 3️⃣ PROCESAR CLIENTES
            // =============================

            var emailsCsv = customerCsvs
                .Select(c => c.Email.Trim())
                .Distinct()
                .ToList();

            var existingEmails = await _context.Customers
                .Where(c => emailsCsv.Contains(c.Email))
                .Select(c => c.Email)
                .ToHashSetAsync();

            var clientes = customerCsvs
                .GroupBy(c => c.Email.Trim())
                .Select(g => g.First())
                .Where(p => !existingEmails.Contains(p.Email.Trim()))
                .Select(p => new Customer
                {
                    CustomerId = p.CustomerID,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Email = p.Email.Trim(),
                    CityId = cityDictionary[
                        (
                            p.City.Trim(),
                            countryDictionary[p.Country.Trim()]
                        )
                    ]
                })
                .ToList();

            if (clientes.Any())
            {
                await _context.BulkInsertAsync(clientes, options =>
                {
                    options.SetOutputIdentity = true;
                });
            }

            // =============================
            // 4️⃣ INSERTAR TELEFONOS
            // =============================

            var telefonos = customerCsvs
                .Where(p => !existingEmails.Contains(p.Email.Trim()))
                .Zip(clientes, (csv, cliente) => new Phone
                {
                    PhoneNumber = csv.Phone,
                    CustomerId = cliente.CustomerId
                })
                .ToList();

            if (telefonos.Any())
                await _context.BulkInsertAsync(telefonos);

            _context.ChangeTracker.Clear();
        }

        public async Task LoadOrdersAndDetails()
        {
            var basePath = AppContext.BaseDirectory;
            var inputPath = Path.Combine(basePath, "Data", "Input");

            // Obtener clientes existentes
            var clients = await _context.Customers
                .Select(x => x.CustomerId)
                .ToHashSetAsync();

            // Leer CSV y filtrar por clientes válidos
            var ordersCsvReader = new CsvReader<OrderCsvModel>();
            var ordersCsv = ordersCsvReader
                .Extract(Path.Combine(inputPath, "orders.csv"))
                .Where(r => clients.Contains(r.CustomerID))
                .ToList();

            // =========================
            // PROCESAR ORDER STATUS
            // =========================
            var statusNames = ordersCsv.Select(p => p.Status.Trim()).Distinct().ToList();
            var existingStatuses = await _context.OrderStatuses
                .Where(s => statusNames.Contains(s.Name))
                .ToListAsync();

            var existingStatusNames = existingStatuses.Select(s => s.Name).ToHashSet();
            var newStatuses = statusNames
                .Where(name => !existingStatusNames.Contains(name))
                .Select(name => new OrderStatus { Name = name })
                .ToList();

            if (newStatuses.Any())
                await _context.BulkInsertAsync(newStatuses);

            var statusDictionary = await _context.OrderStatuses
                .Where(s => statusNames.Contains(s.Name))
                .ToDictionaryAsync(s => s.Name, s => s.StatusId);

            // =========================
            // CREAR ÓRDENES
            // =========================
            var orders = ordersCsv
                .Where(csv => DateTime.TryParse(csv.OrderDate, out _))
                .Select(csv => new Order
                {
                    CustomerId = csv.CustomerID,
                    OrderDate = DateTime.Parse(csv.OrderDate),
                    StatusId = statusDictionary[csv.Status.Trim()]
                })
                .ToList();

            if (orders.Any())
            {
                await _context.BulkInsertAsync(orders, options =>
                {
                    options.SetOutputIdentity = true;
                });
            }

            // =========================
            // CARGAR DETALLES DE ÓRDENES
            // =========================
            var orderIds = orders.Select(o => o.OrderId).ToHashSet();
            var products = await _context.Products.Select(p => p.ProductId).ToHashSetAsync();

            var reader = new CsvReader<OrderDetailsCsvModel>();
            var detailsCsv = reader.Extract(Path.Combine(inputPath, "order_details.csv")).ToList();

            // Filtrar solo registros válidos y eliminar duplicados en el CSV
            var validDetails = detailsCsv
                .Where(d => orderIds.Contains(d.OrderID) && products.Contains(d.ProductID))
                .GroupBy(d => new { d.OrderID, d.ProductID })
                .Select(g => g.First())
                .ToList();

            // Filtrar detalles que ya existen en la base de datos
            var existingKeys = await _context.OrderDetails
                .Select(od => new { od.OrderId, od.ProductId })
                .ToListAsync();

            var existingKeysHash = existingKeys
                .Select(k => new { OrderID = k.OrderId, ProductID = k.ProductId })
                .ToHashSet();

            // Precalcular precios en un diccionario
            var productDictionary = await _context.Products
                .ToDictionaryAsync(p => p.ProductId, p => p.Price);

            // Crear lista de OrderDetail sin usar await dentro de LINQ
            var orderDetails = validDetails
                .Where(d => !existingKeysHash.Contains(new { d.OrderID, d.ProductID }))
                .Select(d => new OrderDetail
                {
                    OrderId = d.OrderID,
                    ProductId = d.ProductID,
                    Quantity = d.Quantity,
                    UnitPrice = productDictionary[d.ProductID],
                    TotalPrice = productDictionary[d.ProductID] * d.Quantity
                })
                .ToList();

            if (orderDetails.Any())
                await _context.BulkInsertAsync(orderDetails);
        }
    }
}
