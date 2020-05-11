using Moq;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductServiceTests
    {
        /// <summary>
        /// Take this test method as a template to write your test method.
        /// A test method must check if a definite method does its job:
        /// returns an expected value from a particular set of parameters
        /// </summary>
        [Fact]
        public void ExampleMethod()
        {
            // Arrange

            // Act


            // Assert
            Assert.Equal(1, 1);
        }

        /// <summary>
        /// Take this test method as a template to write your test method.
        /// A test method must check if a definite method does its job:
        /// returns an expected value from a particular set of parameters
        /// </summary>
        [Fact]
        public async Task TestGetProductByIdAsyncMockTest()
        {
            // Arrange
            Product product = new Product
            {
                Id = 1,
                Description = "one",
                Details = "onedetails",

            };

            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(x => x.GetProduct(1)).Returns(Task.FromResult(product));
            // Act
            var productService = new ProductService(null, mockProductRepository.Object, null, null);
            var result = await productService.GetProduct(1);
            // Assert

            Assert.NotNull(result);
            Assert.Equal("one", result.Description);
        }


        //parameterized test
        [Theory]
        [InlineData(1, "one", "oneDetails")]
        [InlineData(2, "two", "twoDetails")]
        [InlineData(3, "three", "threeDetails")]
        public async Task TestGetProductByIdAsyncMockTestParameterizedTestData(int id, string description, string details)
        {


            Product product = new Product
            {
                Id = id,
                Description = description,
                Details = details,

            };

            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(x => x.GetProduct(id)).Returns(Task.FromResult(product));
            // Act
            var productService = new ProductService(null, mockProductRepository.Object, null, null);
            var result = await productService.GetProduct(id);
            // Assert

            Assert.NotNull(result);
            Assert.Equal(description, result.Description);
        }
        // TODO write test methods to ensure a correct coverage of all possibilities
    }
}