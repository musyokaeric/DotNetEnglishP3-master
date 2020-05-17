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

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class OrderServiceTest
    {
        /// <summary>
        /// IN-MEMORY SET UP
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

        /// <summary>
        /// ORDER SERVICE TESTS
        /// </summary>
        /// 
        [Theory]
        [InlineData(1, "one")]
        [InlineData(2, "two")]
        [InlineData(3, "three")]
        [InlineData(4, "four")]
        public async Task TestGetOrderByIdAsyncInMemoryTestParametizedTestData(int id, string name)
        {
            //Arrange
            var option = TestDBContextOptionsBuilder();
            SeedOrdersTestDb(option);

            using (var context = new P3Referential(option))
            {
                var orderRepository = new OrderRepository(context);
                var orderService = new OrderService(null, orderRepository, null);

                //Act
                var result = await orderService.GetOrder(id);

                //Assert
                Assert.NotNull(result);
                Assert.IsType<Order>(result);
                Assert.Equal(name, result.Name);

                context.Database.EnsureDeleted();
            }
        }

        [Theory]
        [InlineData(-5)]
        [InlineData(0)]
        [InlineData(26)]
        public async Task TestGetOrderBuInvalidIdAsyncInMemoryTestParametizedTestData(int id)
        {
            //Arrange
            var option = TestDBContextOptionsBuilder();
            SeedOrdersTestDb(option);

            using (var context = new P3Referential(option))
            {
                var orderRepository = new OrderRepository(context);
                var orderService = new OrderService(null, orderRepository, null);

                //Act
                var result = await orderService.GetOrder(id);

                //Assert
                Assert.Null(result);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TestGetOrdersAsync()
        {
            //Arrange
            var option = TestDBContextOptionsBuilder();
            SeedOrdersTestDb(option);

            using (var context = new P3Referential(option))
            {
                var orderRepository = new OrderRepository(context);
                var orderService = new OrderService(null, orderRepository, null);

                //Act
                var result = await orderService.GetOrders();

                //Assert
                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.IsType<List<Order>>(result);
                Assert.Equal(4, result.Count);
                Assert.NotNull(result.FirstOrDefault(o => o.Name == "three"));
            }
        }

        [Fact]
        public void TestSaveOrderInMemoryTest()
        {
            //Arrange
            var option = TestDBContextOptionsBuilder();
            SeedProductsTestDb(option);

            Product product1;
            Product product2;

            using (var context = new P3Referential(option))
            {
                product1 = context.Product.FirstOrDefault(p => p.Id == 1);
                product2 = context.Product.FirstOrDefault(p => p.Id == 2);
            }
            OrderViewModel _testOrderViewModel = new OrderViewModel
            {
                OrderId = 1,
                Name = "one",
                Address = "oneAddress",
                City = "oneCity",
                Zip = "oneZip",
                Country = "oneCountry",
                Lines = new List<CartLine>
                {
                    new CartLine{ OrderLineId = 1, Product = product1, Quantity = 3},
                    new CartLine{ OrderLineId = 1, Product = product2 , Quantity = 8}
                }
            };
            
            using (var context = new P3Referential(option))
            {
                Cart cart = new Cart();
                foreach (var line in _testOrderViewModel.Lines)
                {
                    cart.AddItem(line.Product, line.Quantity);
                }
                var orderReopsitory = new OrderRepository(context);
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, orderReopsitory, null);

                var orderService = new OrderService(cart, orderReopsitory, productService);

                //Act
                orderService.SaveOrder(_testOrderViewModel);

                //Assert
                var result = context.Order.ToList();
                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.Single(result);
                Assert.Equal("oneAddress", result.First().Address);
                Assert.Equal(2, result.First().OrderLine.Count);

                Assert.Equal(12, context.Product.FirstOrDefault(p => p.Id == 1).Quantity);

                context.Database.EnsureDeleted();
            }
        }
    }
}
