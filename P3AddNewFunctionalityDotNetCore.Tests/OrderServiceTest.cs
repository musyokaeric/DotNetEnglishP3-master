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
                foreach (var item in _testOrderList)
                {
                    context.Order.Add(item);
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
        public async Task TestGetOrderByIdAsyncParametizedTestData(int id, string name)
        {
            //Arrange
            var option = TestDBContextOptionsBuilder();
            SeedTestDb(option);

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
            }

            //worst case, entering invalid id
        }

        [Fact]
        public async Task TestGetOrdersAsync()
        {
            //Arrange
            var option = TestDBContextOptionsBuilder();
            SeedTestDb(option);

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
        public void TestSaveOrder()
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
                Lines = new List<CartLine>
                {
                    new CartLine{ OrderLineId = 1, Product = new Product{}, Quantity = 1},
                    new CartLine{ OrderLineId = 1, Product = new Product{}, Quantity = 1}
                }
            };
            var option = TestDBContextOptionsBuilder();
            using (var context = new P3Referential(option))
            {
                Cart cart = new Cart();
                var orderReopsitory = new OrderRepository(context);
                var moqProductService = new Mock<IProductService>();
                moqProductService.Setup(x => x.UpdateProductQuantities());

                var orderService = new OrderService(cart, orderReopsitory, moqProductService.Object);

                //Act
                orderService.SaveOrder(_testOrderViewModel);

                //Assert
                var result = context.Order.ToList();
                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.Single(result);
                Assert.Equal("oneAddress", result.First().Address);
                Assert.Equal(2, result.First().OrderLine.Count);
            }
        }
    }
}
