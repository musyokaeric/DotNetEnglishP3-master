using System;
using System.Collections.Generic;
using System.Text;

using Moq;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using System.Threading.Tasks;
using Xunit;
using P3AddNewFunctionalityDotNetCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductRepositoryTests
    {
        /// <summary>
        /// INTEGRATION TEST DB SETUP
        /// </summary>
        /// 

        private P3Referential _context;

        public ProductRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<P3Referential>()
                .UseSqlServer("Server=.\\SQLEXPRESS;Database=P3Referential-2f561d3b-493f-46fd-83c9-6e2643e7bd0a;Trusted_Connection=True;MultipleActiveResultSets=true")
                .Options;
            _context = new P3Referential(options);
        }

        /// <summary>
        /// IN-MEMORY TEST SEUP
        /// </summary>
        /// 

        private static List<Product> _testProductList => GenerateProductData();

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

        private DbContextOptions<P3Referential> TestDBContextOptionsBuilder()
        {
            return new DbContextOptionsBuilder<P3Referential>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private void SeedTestDb(DbContextOptions<P3Referential> options)
        {
            using (var context = new P3Referential(options))
            {
                foreach (var item in _testProductList)
                {
                    context.Product.Add(item);
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// PRODUCT REPOSITORY TESTS
        /// </summary>
        /// 

        [Theory]
        [InlineData(1, "(2nd Generation) - Black")]
        [InlineData(2, "Tangle-Free Micro USB Cable")]
        [InlineData(3, "Riptidz, In-Ear")]
        public async Task TestGetProductIntegrationTestParametizedTestData(int id, string description)
        {
            // Arrange
            var productRepository = new ProductRepository(_context);

            //Act
            var result = await productRepository.GetProduct(id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Product>(result);
            Assert.Equal(description, result.Description);

            //worst case, id that does not exist = 6, -2
        }

        [Fact]
        public async Task TestGetProductIntegrationTest()
        {
            //Arrange
            var productRepository = new ProductRepository(_context);

            //Act
            var result = await productRepository.GetProduct();

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.IsType<List<Product>>(result);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public void TestGetAllProductsIntegrationTest()
        {
            //Arrange
            var productRepository = new ProductRepository(_context);

            //Act
            var result = productRepository.GetAllProducts();

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.IsType<List<Product>>(result);
            Assert.Equal(5, result.Count());
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 10)]
        [InlineData(3, 15)]
        [InlineData(4, 20)]
        public void TestUpdateProductStocksInMemoryTestParametizedTestData(int id, int quantityToUpdate)
        {
            // Arrange
            var product = new Product
            {
                Id = id,
                Name = "name",
                Description = "description",
                Price = 12.95,
                Quantity = 40
            };
            var options = TestDBContextOptionsBuilder();
            using (var context = new P3Referential(options))
            {
                context.Product.Add(product);
                context.SaveChanges();

                var productRepository = new ProductRepository(context);
                var quantity = product.Quantity - quantityToUpdate;

                //Act
                productRepository.UpdateProductStocks(id, quantityToUpdate);

                //Assert
                var result = context.Product.FirstOrDefault(p => p.Id == id);

                Assert.NotNull(result);
                Assert.Equal(quantity, result.Quantity);

                context.Database.EnsureDeleted();
            }

            //worst case = putting the wrong id
            //worst case = entering a quantityToRemove to be of a higher quantity
            //worst case = entering a nwgative quantityToRemove
        }

        [Fact]
        public void TestSaveProductInMemoryTest()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "one",
                Description = "oneDescription",
                Price = 12.05,
                Quantity = 15
            };

            var options = TestDBContextOptionsBuilder();
            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);

                // Act
                productRepository.SaveProduct(product);

                // Assert
                var result = context.Product.ToList();

                Assert.Single(result);
                Assert.NotNull(result.First());
                Assert.Equal("one", result.First().Name);

                context.Database.EnsureDeleted();
            }

            //worstcase = saving a null product
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void TestDeleteProductInMemoryTestParametizedTestData(int id)
        {
            // Arrange
            var options = TestDBContextOptionsBuilder();
            SeedTestDb(options);

            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var product = context.Product.FirstOrDefault(p => p.Id == id);

                // Act
                productRepository.DeleteProduct(id);

                //Assert
                var result = context.Product.FirstOrDefault(p => p.Id == id);

                Assert.Null(result);
                Assert.DoesNotContain(product, context.Product.ToList());

                context.Database.EnsureDeleted();
            }

            //worst case = enting the wrong id
        }
    }
}
