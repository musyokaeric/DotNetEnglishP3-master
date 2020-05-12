﻿using Moq;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using System.Threading.Tasks;
using Xunit;
using P3AddNewFunctionalityDotNetCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Collections.Generic;
using System;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using P3AddNewFunctionalityDotNetCore.Models;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductServiceTests
    {
        /// <summary>
        /// INTEGRATION TEST DB SETUP
        /// </summary>
        /// 

        private P3Referential _context;

        public ProductServiceTests()
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
        /// PRODUCT SERVICE TESTS
        /// </summary>

        [Fact]
        public void TestGetAllProductsViewModelMockTest()
        {
            // Arrange
            var moqProductRepository = new Mock<IProductRepository>();
            moqProductRepository.Setup(x => x.GetAllProducts()).Returns(GenerateProductData());
            var productService = new ProductService(null, moqProductRepository.Object, null, null);

            //Act
            var result = productService.GetAllProductsViewModel();

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.IsType<List<ProductViewModel>>(result);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public void TestGetAllProductsIntegrationTest()
        {
            // Arrange
            var productRepository = new ProductRepository(_context);
            var productService = new ProductService(null, productRepository, null, null);

            // Act
            var result = productService.GetAllProducts();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.IsType<List<Product>>(result);
            Assert.Equal(5, result.Count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void TestGetProductByIdViewModelMockTestParametizedTestData(int id)
        {
            // Aarrange
            var products = new List<Product>
            {
                new Product{ Id = 1, Name = "one", Description = "oneDescription"},
                new Product{ Id = 2, Name = "two", Description = "twoDescription"},
                new Product{ Id = 3, Name = "three", Description = "threeDescription"},
                new Product{ Id = 4, Name = "four", Description = "fourDescription"},
                new Product{ Id = 5, Name = "five", Description = "fiveDescription"}
            };
            var name = products.FirstOrDefault(p => p.Id == id).Name;
            var moqProductRepository = new Mock<IProductRepository>();
            moqProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            var productService = new ProductService(null, moqProductRepository.Object, null, null);

            // Act
            var result = productService.GetProductByIdViewModel(id);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ProductViewModel>(result);
            Assert.Equal(name, result.Name);

            //worst case, entering the wrong id, -2, 10
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void TestGetProductByIdMockTestParametizedTestData(int id)
        {
            // Aarrange
            var products = new List<Product>
            {
                new Product{ Id = 1, Name = "one", Description = "oneDescription"},
                new Product{ Id = 2, Name = "two", Description = "twoDescription"},
                new Product{ Id = 3, Name = "three", Description = "threeDescription"},
                new Product{ Id = 4, Name = "four", Description = "fourDescription"},
                new Product{ Id = 5, Name = "five", Description = "fiveDescription"}
            };
            var name = products.FirstOrDefault(p => p.Id == id).Name;
            var moqProductRepository = new Mock<IProductRepository>();
            moqProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            var productService = new ProductService(null, moqProductRepository.Object, null, null);

            // Act
            var result = productService.GetProductById(id);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Product>(result);
            Assert.Equal(name, result.Name);

            //worst case, entering the wrong id, -2, 10
        }

        [Theory]
        [InlineData(1, "one", "oneDescription")]
        [InlineData(2, "two", "twoDescription")]
        [InlineData(3, "three", "threeDescription")]
        [InlineData(4, "four", "fourDescription")]
        [InlineData(5, "five", "fiveDescription")]
        public async Task TestGetProductByIdAsyncMockTestParametizedTestData(int id, string name, string description)
        {
            // Aarrange
            var product = new Product
            {
                Id = id,
                Name = name,
                Description = description
            };

            var moqProductRepository = new Mock<IProductRepository>();
            moqProductRepository.Setup(x => x.GetProduct(id)).Returns(Task.FromResult(product));
            var productService = new ProductService(null, moqProductRepository.Object, null, null);

            //Act
            var result = await productService.GetProduct(id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Product>(result);
            Assert.Equal(name, result.Name);

            //Question: worst case by entering the wrong id?
        }

        [Fact]
        public async Task TestGetAllProductsAsyncIntegrationTest()
        {
            //Arrange
            var productRepository = new ProductRepository(_context);
            var productService = new ProductService(null, productRepository, null, null);

            //Act
            var result = await productService.GetProduct();

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(5, result.Count);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public void TestUpdateProductQuantitiesInMemoryTestParametizedTestData(int quantity)
        {
            //Arrange
            var product = new Product
            {
                Id = 1,
                Name = "name",
                Quantity = 20
            };
            var options = TestDBContextOptionsBuilder();
            using (var context = new P3Referential(options))
            {
                context.Product.Add(product);
                context.SaveChanges();

                var cart = new Cart();
                cart.AddItem(product, quantity);
                int quantityLeft = product.Quantity - quantity;

                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);

                //Act
                productService.UpdateProductQuantities();
                var result = context.Product.First().Quantity;

                //Assert
                Assert.Equal(quantityLeft, result);

                context.Database.EnsureDeleted();
            }

            //worst case, quantity to remove is larger than the product quantity, its a negative
        }

        [Fact]
        public void TestCheckProductModelErrors()
        {
            //takes ProductViewModel as arguments - parametized
            //uses IStringLocalizer - create your own
            Assert.Equal(1, 2);
        }

        [Theory]
        [InlineData(1, "one", "oneDescription", "oneDetails", "10.00", "50")]
        [InlineData(2, "two", "twoDescription", "twoDetails", "20.00", "40")]
        [InlineData(3, "three", "threeDescription", "threeDetails", "30.00", "30")]
        [InlineData(4, "four", "fourDescription", "fourDetails", "40.00", "20")]
        [InlineData(5, "five", "fiveDescription", "fiveDetails", "50.00", "10")]
        public void TestSaveProductInMemoryTestParametizedTestData(int id, string name, string description, string details, string price, string stock)
        {
            //takes ProductViewModel as arguments - parametized
            //writes to productRepository = inmemory
            //Arrange
            var productViewModel = new ProductViewModel
            {
                Id = id,
                Name = name,
                Description = description,
                Details = details,
                Price = price,
                Stock = stock
            };
            var options = TestDBContextOptionsBuilder();
            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(null, productRepository, null, null);

                //Act
                productService.SaveProduct(productViewModel);
                var result = context.Product.ToList();

                //Assert
                Assert.Single(result);
                Assert.Equal(name, result.First().Name);

                context.Database.EnsureDeleted();
            }

            //worst case, saving null product, saving product with null properties
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void TestDeleteProductInMemoryTestParametizedDataTest(int id)
        {
            //Arrange
            var options = TestDBContextOptionsBuilder();
            SeedTestDb(options);

            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(null, productRepository, null, null);

                //Act
                productService.DeleteProduct(id);

                //Assert
                var result = context.Product.ToList();
                Assert.Equal(4, result.Count);

                context.Database.EnsureDeleted();
            }

            //worst case, entering the wrong id
            //need to check on the cart scenario
        }
    }
}