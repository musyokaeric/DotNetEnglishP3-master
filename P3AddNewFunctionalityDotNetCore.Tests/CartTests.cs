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
    public class CartTests
    {
        [Fact]
        public void TestAddItem()
        {
            //Arrange
            var product1 = new Product { Id = 1, Name = "one", Description = "oneDescription" };
            var product2 = new Product { Id = 2, Name = "two", Description = "twoDescription" };
            var cart = new Cart();

            //Act
            cart.AddItem(product1, 1);
            cart.AddItem(product2, 1);
            cart.AddItem(product1, 1);

            //Assert
            Assert.NotEmpty(cart.Lines);
            Assert.Equal(2, cart.Lines.Count());
            Assert.Equal(2, cart.Lines.FirstOrDefault(c => c.Product.Id == 1).Quantity);

        }

        [Fact]
        public void TestRemoveLine()
        {
            //Arrange
            var product1 = new Product { Id = 1, Name = "one", Description = "oneDescription" };
            var product2 = new Product { Id = 2, Name = "two", Description = "twoDescription" };

            var cart = new Cart();
            cart.AddItem(product1, 1);
            cart.AddItem(product2, 1);
            cart.AddItem(product1, 1);

            //Act
            cart.RemoveLine(product2);

            //Assert
            Assert.Single(cart.Lines);
            Assert.Equal(1, cart.Lines.First().Product.Id);
        }

        [Fact]
        public void TestGetAverageValue()
        {
            //Arrange
            var product1 = new Product { Id = 1, Name = "one", Price = 12, Quantity = 3 };
            var product2 = new Product { Id = 2, Name = "two", Price = 20, Quantity = 3 };

            var cart = new Cart();
            cart.AddItem(product1, 2);
            cart.AddItem(product2, 3);

            //Act
            double expectedValue = ((12 * 2) + (20 * 3)) / 5;
            var result = cart.GetAverageValue();

            //Assert
            Assert.IsType<double>(result);
            Assert.Equal(expectedValue, result);

            //worst case, quantity is zero
        }

        [Fact]
        public void TestClearCart()
        {
            //Arrange
            var product1 = new Product { Id = 1, Name = "one", Description = "oneDescription" };
            var product2 = new Product { Id = 2, Name = "two", Description = "twoDescription" };

            var cart = new Cart();
            cart.AddItem(product1, 1);
            cart.AddItem(product2, 1);
            cart.AddItem(product1, 1);

            //Act
            cart.Clear();

            //Assert
            Assert.Empty(cart.Lines);
        }
    }
}
