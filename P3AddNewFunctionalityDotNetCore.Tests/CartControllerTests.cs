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
    public class CartControllerTests
    {
        /// <summary>
        /// INTEGRATION TEST DB SETUP
        /// </summary>
        /// 

        private P3Referential _context;

        public CartControllerTests()
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
                    Id = 1,
                    Name = "Echo Dot",
                    Description = "(2nd Generation) - Black",
                    Quantity = 10,
                    Price = 92.50
                },

                new Product
                {
                    Id = 2,
                    Name = "Anker 3ft / 0.9m Nylon Braided",
                    Description = "Tangle-Free Micro USB Cable",
                    Quantity = 20,
                    Price = 9.99
                },

                new Product
                {
                    Id = 3,
                    Name = "JVC HAFX8R Headphone",
                    Description = "Riptidz, In-Ear",
                    Quantity = 30,
                    Price = 69.99
                },

                new Product
                {
                    Id = 4,
                    Name = "VTech CS6114 DECT 6.0",
                    Description = "Cordless Phone",
                    Quantity = 40,
                    Price = 32.50
                },

                new Product
                {
                    Id = 5,
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
        /// CART CONTROLLER TESTS
        /// </summary>
        /// 

        [Fact]
        public void TestIndexEmptyCart()
        {
            //Arrange
            var cart = new Cart();
            var cartController = new CartController(cart, null);

            //Act
            var result = cartController.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var modal = Assert.IsType<Cart>(viewResult.Model);
            Assert.Empty(modal.Lines);
        }

        [Fact]
        public void TestIndexNonEmptyCart()
        {
            //Arrange
            var product1 = new Product { Id = 1, Name = "one", Quantity = 10 };
            var product2 = new Product { Id = 2, Name = "two", Quantity = 20 };

            var cart = new Cart();
            cart.AddItem(product1, 5);
            cart.AddItem(product2, 10);

            var cartController = new CartController(cart, null);

            //Act
            var result = cartController.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var modal = Assert.IsType<Cart>(viewResult.Model);
            Assert.Equal(2, modal.Lines.Count());
        }

        [Theory]
        [InlineData(1, "one")]
        [InlineData(2, "two")]
        [InlineData(3, "three")]
        [InlineData(4, "four")]
        public void TestAddItemToCartMockTestParametizedTestData(int id, string name)
        {
            //Arrange
            var product = new Product { Id = id, Name = name };

            var moqProductService = new Mock<IProductService>();
            moqProductService.Setup(x => x.GetProductById(id)).Returns(product);

            var cart = new Cart();

            var cartController = new CartController(cart, moqProductService.Object);

            //Act
            var result = cartController.AddToCart(id);

            //Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(name, cart.Lines.First().Product.Name);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void TestAddItemToCartIntegrationTestValidParametizedTestData(int id)
        {
            //Arrange
            var name = _context.Product.FirstOrDefault(p => p.Id == id).Name;
            var productRepository = new ProductRepository(_context);
            var cart = new Cart();
            var productService = new ProductService(null, productRepository, null, null);
            var cartController = new CartController(cart, productService);

            //Act
            var result = cartController.AddToCart(id);

            //Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(name, cart.Lines.First().Product.Name);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(10)]
        public void TestAddItemToCartInMemoryTestInvalidParametizedTestData(int id)
        {
            //Arrange
            var options = TestDBContextOptionsBuilder();
            SeedTestDb(options);

            using (var context = new P3Referential(options))
            {
                var product = context.Product.FirstOrDefault(p => p.Id == id);
                var productRepository = new ProductRepository(context);
                var cart = new Cart();
                var productService = new ProductService(null, productRepository, null, null);
                var cartController = new CartController(cart, productService);

                //Act
                var result = cartController.AddToCart(id);

                //Assert
                var viewResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Empty(cart.Lines);
            }

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void TestRemoveFromCartMockTestValidParametizedTestData(int id)
        {
            //Arrange
            var cart = new Cart();
            foreach (var item in _testProductList)
            {
                cart.AddItem(item, 2);
            }
            var moqProductService = new Mock<IProductService>();
            moqProductService.Setup(x => x.GetAllProducts()).Returns(_testProductList);
            
            var cartController = new CartController(cart, moqProductService.Object);

            //Act
            var result = cartController.RemoveFromCart(id);

            //Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(4, cart.Lines.Count());
            Assert.Null(cart.Lines.FirstOrDefault(p => p.Product.Id == id));
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(10)]
        public void TestRemoveFromCartMockTestInvalidParametizedTestData(int id)
        {
            //Arrange
            var cart = new Cart();
            foreach (var item in _testProductList)
            {
                cart.AddItem(item, 2);
            }
            var moqProductService = new Mock<IProductService>();
            moqProductService.Setup(x => x.GetAllProducts()).Returns(_testProductList);

            var cartController = new CartController(cart, moqProductService.Object);

            //Act
            var result = cartController.RemoveFromCart(id);

            //Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(5, cart.Lines.Count());
            Assert.Null(cart.Lines.FirstOrDefault(p => p.Product.Id == id));
        }

    }
}
