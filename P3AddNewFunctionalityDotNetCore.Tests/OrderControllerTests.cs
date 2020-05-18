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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class OrderControllerTests
    {
        /// <summary>
        /// IN-MEMORY SETUP
        /// </summary>
        /// 

        private static List<Order> _testOrderList => GenerateOrderData();
        private static List<Product> _testProductList => GenerateProductData();

        private static List<Order> GenerateOrderData()
        {
            List<Order> testOrdersList = new List<Order>
            {
                new Order { Id = 1, Name = "one", OrderLine = new List<OrderLine>() },
                new Order { Id = 2, Name = "two", OrderLine = new List<OrderLine>()},
                new Order { Id = 3, Name = "three", OrderLine = new List<OrderLine>()},
                new Order { Id = 4, Name = "four", OrderLine = new List<OrderLine>()},
            };
            return testOrdersList;
        }

        private static List<Product> GenerateProductData()
        {
            var testProductsList = new List<Product>
            {
                new Product { Id = 1, Name = "one", Quantity = 15 },
                new Product { Id = 2, Name = "two", Quantity = 20 }
            };
            return testProductsList;
        }

        private DbContextOptions<P3Referential> TestDBContextOptionsBuilder()
        {
            return new DbContextOptionsBuilder<P3Referential>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private void SeedOrdersTestDb(DbContextOptions<P3Referential> options)
        {
            using (var context = new P3Referential(options))
            {
                foreach (var item in _testOrderList)
                {
                    context.Order.Add(item);
                }
                context.SaveChanges();
            }
        }

        private void SeedProductsTestDb(DbContextOptions<P3Referential> options)
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



        //IstringLocalizer setup
        List<LocalizedString> localizedStrings = new List<LocalizedString>
            {
                new LocalizedString("ErrorMissingName","ErrorMissingName"),
                new LocalizedString("ErrorMissingAddress","ErrorMissingAddress"),
                new LocalizedString("ErrorMissingCity","ErrorMissingCity"),
                new LocalizedString("ErrorMissingZipCode","ErrorMissingZipCode"),
                new LocalizedString("ErrorMissingCountry","ErrorMissingCountry"),
                new LocalizedString("CartEmpty","CartEmpty"),
            };

        [Fact]
        public void TestIndex()
        {
            //Arrange
            var orderController = new OrderController(null, null, null);

            //Assert
            var result = orderController.Index();

            //Assert
            Assert.IsType<ViewResult>(result);
            Assert.IsType<OrderViewModel>(result.Model);
        }

        [Fact]
        public void TestSaveOrderValidEntryInMemoryTest()
        {
            //Arrange
            OrderViewModel _testOrderViewModel = new OrderViewModel
            {
                OrderId = 1,
                Name = "one",
                Address = "oneAddress",
                City = "oneCity",
                Zip = "oneZip",
                Country = "oneCountry",
            };
            var cart = new Cart();
            cart.AddItem(_testProductList[0], 3);
            cart.AddItem(_testProductList[1], 8);

            var moqStringLocaliser = new Mock<IStringLocalizer<OrderController>>();
            foreach (var item in localizedStrings)
            {
                moqStringLocaliser.Setup(x => x[item.Name]).Returns(item);
            }

            var option = TestDBContextOptionsBuilder();
            SeedProductsTestDb(option);

            using (var context = new P3Referential(option))
            {
                var productRepository = new ProductRepository(context);
                var orderRepository = new OrderRepository(context);
                var productService = new ProductService(cart, productRepository, orderRepository, null);
                var orderService = new OrderService(cart, orderRepository, productService);
                
                var orderController = new OrderController(cart, orderService, moqStringLocaliser.Object);

                //Act
                var result = orderController.Index(_testOrderViewModel);

                //Assert
                Assert.IsType<RedirectToActionResult>(result);
                Assert.Single(context.Order.ToList());
                Assert.Equal("one", context.Product.FirstOrDefault(p => p.Id == 1).Name);
                Assert.Equal(12, context.Product.FirstOrDefault(p => p.Id == 2).Quantity);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public void TestSaveOrderEmptyCartMockTest()
        {
            //Arrange
            OrderViewModel _testOrderViewModel = new OrderViewModel
            {
                OrderId = 1,
                Name = "one",
                Address = "oneAddress",
                City = "oneCity",
                Zip = "oneZip",
                Country = "oneCountry",
            };
            var cart = new Cart();

            var moqStringLocaliser = new Mock<IStringLocalizer<OrderController>>();
            foreach (var item in localizedStrings)
            {
                moqStringLocaliser.Setup(x => x[item.Name]).Returns(item);
            }

            var orderController = new OrderController(cart, null, moqStringLocaliser.Object);

            //Act
            var result = orderController.Index(_testOrderViewModel);

            //Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(orderController.ModelState.IsValid);
        }

        [Fact]
        public void TestCompled()
        {
            //Arrange
            var cart = new Cart();
            cart.AddItem(_testProductList[0], 3);
            cart.AddItem(_testProductList[1], 8);

            var orderController = new OrderController(cart, null, null);

            //Act
            var result = orderController.Completed();

            //Assert
            Assert.IsType<ViewResult>(result);
            Assert.Empty(cart.Lines);
        }
    }
}
