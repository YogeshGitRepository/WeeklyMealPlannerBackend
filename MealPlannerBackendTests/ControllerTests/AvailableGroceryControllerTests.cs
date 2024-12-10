using MealPlannerBackend.Controllers;
using MealPlannerBackend.Data;
using MealPlannerBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace MealPlannerBackendTests.ControllerTests;

[TestFixture]
public class AvailableGroceryControllerTests
{
    private ApplicationDbContext _dbContext;
    private AvailableGroceryController _controller;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _controller = new AvailableGroceryController(_dbContext);

        _dbContext.Ingredients.AddRange(
            new Ingredient { Id = 1, Name = "Milk", Quantity = 2 },
            new Ingredient { Id = 2, Name = "Eggs", Quantity = 12 }
        );
        _dbContext.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task GetGroceries_ValidToken_ReturnsOkWithIngredients()
    {
        var token = GenerateValidJwtToken();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request = { Headers = { ["Authorization"] = $"Bearer {token}" } }
            }
        };

        var result = await _controller.GetGroceries();

        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var groceries = okResult.Value as List<Ingredient>;
        Assert.AreEqual(2, groceries?.Count);
    }

    [Test]
    public async Task GetGroceries_InvalidToken_ReturnsUnauthorized()
    {
        var invalidToken = "invalidToken";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request = { Headers = { ["Authorization"] = $"Bearer {invalidToken}" } }
            }
        };

        var result = await _controller.GetGroceries();

        Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
    }

    [Test]
    public async Task AddGrocery_ValidIngredient_ReturnsOk()
    {
        var newIngredient = new Ingredient { Id = 3, Name = "Sugar", Quantity = 1 };

        var result = await _controller.AddGrocery(newIngredient);

        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var ingredient = okResult.Value as Ingredient;
        Assert.AreEqual("Sugar", ingredient?.Name);
    }

    [Test]
    public async Task AddGrocery_NullIngredient_ReturnsBadRequest()
    {
        var result = await _controller.AddGrocery(null);

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    private string GenerateValidJwtToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddMinutes(5)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
