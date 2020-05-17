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
using System.Collections.Generic;
using System;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using P3AddNewFunctionalityDotNetCore.Models;
using Microsoft.Extensions.Localization;
using P3AddNewFunctionalityDotNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductControllerTests
    {
        /// <summary>
        /// INTEGRATION TEST DB SETUP
        /// </summary>
        /// 

        private P3Referential _context;

        public ProductControllerTests()
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
        private static List<ProductViewModel> _testProductViewModelList => GenerateProducViewModeltData();

        private static List<ProductViewModel> GenerateProducViewModeltData()
        {
            List<ProductViewModel> testProductsList = new List<ProductViewModel>
            {
                new ProductViewModel
                {
                    Name = "Echo Dot",
                    Description = "(2nd Generation) - Black",
                    Stock = "10",
                    Price = "92.50"
                },

                new ProductViewModel
                {
                    Name = "Anker 3ft / 0.9m Nylon Braided",
                    Description = "Tangle-Free Micro USB Cable",
                    Stock = "20",
                    Price = "9.99"
                },

                new ProductViewModel
                {
                    Name = "JVC HAFX8R Headphone",
                    Description = "Riptidz, In-Ear",
                    Stock = "30",
                    Price = "69.99"
                },

                new ProductViewModel
                {
                    Name = "VTech CS6114 DECT 6.0",
                    Description = "Cordless Phone",
                    Stock = "40",
                    Price = "32.50"
                },

                new ProductViewModel
                {
                    Name = "NOKIA OEM BL-5J",
                    Description = "Cell Phone",
                    Stock = "50",
                    Price = "895.00"
                }
            };
            return testProductsList;
        }

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
        public void TestIndexMockTest()
        {
            //Arrange
            var moqProductService = new Mock<IProductService>();
            moqProductService.Setup(x => x.GetAllProductsViewModel()).Returns(_testProductViewModelList);

            var productController = new ProductController(moqProductService.Object, null);

            //Act
            var result = productController.Index();

            //Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            moqProductService.Verify(x => x.GetAllProductsViewModel(), Times.Once);
        }

        [Fact]
        public void TestAdminIntegrationTest()
        {
            //Arrange
            var productRepository = new ProductRepository(_context);
            var productService = new ProductService(null, productRepository, null, null);

            var productController = new ProductController(productService, null);

            //Act
            var result = productController.Admin();

            //Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            var modals = Assert.IsAssignableFrom<IEnumerable<ProductViewModel>>(viewResult.ViewData.Model);
            Assert.Equal(5, modals.Count());
        }

        [Fact]
        public void TestCreateProductInMeMoryTestValidInput()
        {
            //Arrange
            var options = TestDBContextOptionsBuilder();
            var product = _testProductViewModelList.First();

            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(null, productRepository, null, null);
                var productController = new ProductController(productService, null);

                //Act
                var result = productController.Create(product);

                //Assert
                Assert.NotNull(result);
                Assert.IsType<RedirectToActionResult>(result);

                var savedProduct = context.Product.ToList();

                Assert.NotNull(savedProduct);
                Assert.NotEmpty(savedProduct);
                Assert.Single(savedProduct);
                Assert.Equal("Echo Dot", savedProduct.First().Name);
            }
        }

        [Fact]
        public void TestCreateProductInMeMoryTestInvalidInput()
        {
            //Arrange
            List<LocalizedString> localizedStrings = new List<LocalizedString>
            {
                new LocalizedString("MissingName","MissingName"),
                new LocalizedString("MissingPrice","MissingPrice"),
                new LocalizedString("PriceNotANumber","PriceNotANumber"),
                new LocalizedString("PriceNotGreaterThanZero","PriceNotGreaterThanZero"),
                new LocalizedString("MissingQuantity","MissingQuantity"),
                new LocalizedString("StockNotAnInteger","StockNotAnInteger"),
                new LocalizedString("StockNotGreaterThanZero","StockNotGreaterThanZero"),
            };
            var moqStringLocaliser = new Mock<IStringLocalizer<ProductService>>();
            foreach (var item in localizedStrings)
            {
                moqStringLocaliser.Setup(x => x[item.Name]).Returns(item);
            }

            var options = TestDBContextOptionsBuilder();
            var product = new ProductViewModel { };

            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(null, productRepository, null, moqStringLocaliser.Object);
                var productController = new ProductController(productService, null);

                //Act
                var result = productController.Create(product);

                //Assert
                Assert.NotNull(result);
                Assert.IsType<ViewResult>(result);

                var savedProduct = context.Product.ToList();

                Assert.Empty(savedProduct);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void TestDeleteProductInMemoryTestParametizedTestData(int id)
        {
            //Arrange
            var options = TestDBContextOptionsBuilder();
            SeedTestDb(options);

            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(null, productRepository, null, null);
                var productController = new ProductController(productService, null);

                //Act
                var result = productController.DeleteProduct(id);

                //Assert
                Assert.NotNull(result);
                Assert.IsType<RedirectToActionResult>(result);

                var reminingProducts = context.Product.ToList();

                Assert.NotNull(reminingProducts);
                Assert.Equal(4, reminingProducts.Count);
                Assert.Null(reminingProducts.FirstOrDefault(p => p.Id == id));
            }
        }

        [Theory]
        [InlineData(-5)]
        [InlineData(0)]
        [InlineData(26)]
        public void TestDeleteProductInvalidIdInMemoryTestParametizedTestData(int id)
        {
            //Arrange
            var options = TestDBContextOptionsBuilder();
            SeedTestDb(options);

            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(null, productRepository, null, null);
                var productController = new ProductController(productService, null);

                //Act
                var exception = Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    productController.DeleteProduct(id);
                });

                //Assert
                Assert.Contains("Invalid id", exception.Message);
                Assert.Equal(5, context.Product.Count());
            }
        }
    }
}
