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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> moqUserManager;
        private readonly Mock<SignInManager<IdentityUser>> moqSignInManager;

        public AccountControllerTests()
        {
            moqUserManager = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object
                );

            moqSignInManager = new Mock<SignInManager<IdentityUser>>(MockBehavior.Loose,
                moqUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<IdentityUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<IdentityUser>>().Object
                );
        }

        LoginModel _testLoginModel = new LoginModel
        {
            Name = "one",
            Password = "onePassword",
            ReturnUrl = "/oneUrl"
        };

        List<IdentityUser> testUsersList = new List<IdentityUser>{
                new IdentityUser { UserName = "one" },
                new IdentityUser { UserName = "two" },
                new IdentityUser { UserName = "three" }
            };


        [Theory]
        [InlineData("")]
        [InlineData("testUrl")]
        [InlineData("numbersUrl")]
        [InlineData(" whitespaceUrl ")]
        public void TestLoginRedirectParametizedTestData(string url)
        {
            // Arrange
            var accountController = new AccountController(null, null);

            // Act
            var result = accountController.Login(url);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LoginModel>(viewResult.Model);
            Assert.Equal(url, model.ReturnUrl);
        }

        [Fact]
        public async Task TestValidLoginMockTestAsync()
        {
            //Arrange
            moqUserManager.Setup(x => x.FindByNameAsync(_testLoginModel.Name)).Returns(Task.FromResult(new IdentityUser(_testLoginModel.Name)));
            
            moqSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

            var accountController = new AccountController(moqUserManager.Object, moqSignInManager.Object);

            //Act
            var result = await accountController.Login(_testLoginModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(_testLoginModel.ReturnUrl, redirectResult.Url);
        }

        [Fact]
        public async Task TestInValidLoginAsyncMockTest()
        {
            //Arrange
            moqUserManager.Setup(x => x.FindByNameAsync(_testLoginModel.Name)).Returns(Task.FromResult(new IdentityUser(_testLoginModel.Name)));

            moqSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

            var accountController = new AccountController(moqUserManager.Object, moqSignInManager.Object);

            //Act
            var result = await accountController.Login(_testLoginModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LoginModel>(viewResult.Model);
            Assert.Equal("/oneUrl", model.ReturnUrl);
        }

        [Fact]
        public async Task TestLogout()
        {
            //Arrange
            string returnUrl = "/Product";

            moqUserManager.Setup(x => x.FindByNameAsync(_testLoginModel.Name)).Returns(Task.FromResult(new IdentityUser(_testLoginModel.Name)));

            moqSignInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

            var accountController = new AccountController(moqUserManager.Object, moqSignInManager.Object);

            //Act
            var result = await accountController.Logout();

            //Assert
            var actionResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(returnUrl, actionResult.Url);
        }
    }
}
