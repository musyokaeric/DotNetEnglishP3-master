using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class InMemoryDbTests
    {
        private static List<Product> _testProductsList = GenerateProductData();

        /// <summary>
        /// Generate the default list of products
        /// </summary>
        private static List<Product> GenerateProductData()
        {
            List<Product> testProductsList = new List<Product>
            {
                 new Product
                     {
                         Name = "Echo Dot",
                         Description = "(2nd Generation) - Black",
                         Quantity = 10,
                         Price = 92.50
                     },

                     new Product
                     {
                         Name = "Anker 3ft / 0.9m Nylon Braided",
                         Description = "Tangle-Free Micro USB Cable",
                         Quantity = 20,
                         Price = 9.99
                     },

                     new Product
                     {
                         Name = "JVC HAFX8R Headphone",
                         Description = "Riptidz, In-Ear",
                         Quantity = 30,
                         Price = 69.99
                     },

                   new Product
                   {
                       Name = "VTech CS6114 DECT 6.0",
                       Description = "Cordless Phone",
                       Quantity = 40,
                       Price = 32.50
                   },

                   new Product
                   {
                       Name = "NOKIA OEM BL-5J",
                       Description = "Cell Phone",
                       Quantity = 50,
                       Price = 895.00
                   }
            };
            return testProductsList;
        }

        private DbContextOptions<P3Referential> TestDbContextOptionsBuilder()
        {
            return new DbContextOptionsBuilder<P3Referential>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), new InMemoryDatabaseRoot())
                .Options;
        }

        private void SeedTestDb(DbContextOptions<P3Referential> options)
        {
            using (var context = new P3Referential(options))
            {
                foreach (var p in _testProductsList)
                {
                    context.Product.Add(p);
                }

                context.SaveChanges();
            }
        }

        [Fact]
        public void TestSaveProductPopulatedProduct()
        {
            // Arrange
            var options = TestDbContextOptionsBuilder();
            ProductViewModel testProductViewModel = new ProductViewModel
            {
                Name = "VTech CS6114 DECT 6.0",
                Description = "Cordless Phone",
                Details = "I am details",
                Stock = "40",
                Price = "32.50"
            };

            using (var context = new P3Referential(options))
            {
                var cart = new Cart();

                var productRepository = new ProductRepository(context);

                var productService = new ProductService(cart, productRepository, null, null);

                // Act
                productService.SaveProduct(testProductViewModel);
            }

            using (var context = new P3Referential(options))
            {
                var result = context.Product.ToList();

                // Assert
                Assert.Single(result);
                Assert.IsAssignableFrom<List<Product>>(result);
                Assert.Equal("VTech CS6114 DECT 6.0", result.First().Name);
                
                context.Database.EnsureDeleted();
            }
        }
    }
}