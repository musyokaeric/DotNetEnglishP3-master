using Microsoft.EntityFrameworkCore;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class IntegrationDbTests
    {
        private P3Referential _context;
        public IntegrationDbTests()
        {
            var options = new DbContextOptionsBuilder<P3Referential>().
                       UseSqlServer("Server=.\\SQLEXPRESS;Database=P3Referential-2f561d3b-493f-46fd-83c9-6e2643e7bd0a;Trusted_Connection=True;MultipleActiveResultSets=true")
                       .Options;

            _context = new P3Referential(options);
        }

        [Fact]
        public async Task TestGetProductByIdIntegrationTestAsync()
        {
            ProductRepository productRepository = new ProductRepository(_context);
            var result = await productRepository.GetProduct(5);
            Assert.NotNull(result);
            Assert.Equal("Cell Phone", result.Description);
        }
    }
}