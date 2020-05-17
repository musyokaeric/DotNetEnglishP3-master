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
    public class OrderRepositoryTests
    {
        /// <summary>
        /// IN-MEMORY TEST SEUP
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
        /// ORDER REPOSITORY TESTS
        /// </summary>
        /// 

        [Fact]
        public void TestSaveOrderInMemoryTest()
        {
            //Arrange
            var order = new Order { Id = 1, Name = "one", OrderLine = new List<OrderLine>() };
            var options = TestDBContextOptionsBuilder();

            using (var context = new P3Referential(options))
            {
                var orderRepository = new OrderRepository(context);

                //Act
                orderRepository.Save(order);

                //Assert
                var orders = context.Order.ToList();
                Assert.Single(orders);
                Assert.Equal("one", orders.First().Name);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public void TestSaveNullOrderInMemoryTest()
        {
            //Arrange
            Order order = null;
            var options = TestDBContextOptionsBuilder();

            using (var context = new P3Referential(options))
            {
                var orderRepository = new OrderRepository(context);

                //Act
                orderRepository.Save(order);

                //Assert
                var orders = context.Order.ToList();
                Assert.Empty(orders);

                context.Database.EnsureDeleted();
            }
        }

        [Theory]
        [InlineData(1, "one")]
        [InlineData(2, "two")]
        [InlineData(3, "three")]
        [InlineData(4, "four")]
        public async Task TestGetOrderByIdAsyncInMemoryTestParametizedTestData(int id, string name)
        {
            //Arrange
            var options = TestDBContextOptionsBuilder();
            SeedTestDb(options);

            using (var context = new P3Referential(options))
            {
                var orderRepository = new OrderRepository(context);

                //Act
                var result = await orderRepository.GetOrder(id);

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
        [InlineData(16)]
        public async Task TestGetOrderByInvalidIdAsyncInMemoryTestParametizedTestData(int id)
        {
            //Arrange
            var options = TestDBContextOptionsBuilder();
            SeedTestDb(options);

            using (var context = new P3Referential(options))
            {
                var orderRepository = new OrderRepository(context);

                //Act
                var result = await orderRepository.GetOrder(id);

                //Assert
                Assert.Null(result);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TestGetOrdersAsync()
        {
            //Arrange
            var options = TestDBContextOptionsBuilder();
            SeedTestDb(options);

            using (var context = new P3Referential(options))
            {
                var orderRepository = new OrderRepository(context);

                //Act
                var result = await orderRepository.GetOrders();

                //Assert
                Assert.NotNull(result);
                Assert.IsType<List<Order>>(result);
                Assert.Equal(4, result.Count);
                Assert.NotNull(result.FirstOrDefault(o => o.Name == "three"));
            }
        }
    }
}
