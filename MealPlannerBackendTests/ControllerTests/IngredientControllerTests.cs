
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealPlannerBackend.Data;
using MealPlannerBackend.Models;

namespace MealPlannerBackendTests.ControllerTests;

[TestFixture]
public class IngredientControllerTests
{
    private IngredientController _controller;
    private ApplicationDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _dbContext.Ingredients.AddRange(new List<Ingredient>
        {
            new Ingredient { Id = 1, Name = "Sugar", Quantity = 150, Measurement = "cup" },
            new Ingredient { Id = 2, Name = "Salt", Quantity = 500, Measurement = "teaspoon" },
        });
        _dbContext.SaveChanges();

        _controller = new IngredientController(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task GetIngredients_ReturnsAllIngredients()
    {
        var result = await _controller.GetIngredients();

        var ingredients = result?.Value;
        Assert.IsNotNull(ingredients);
        Assert.That(ingredients.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task PostIngredient_AddsNewIngredient()
    {
        var newIngredient = new Ingredient { Id = 3, Name = "Flour", Quantity = 200, Measurement = "cups" };

        var result = await _controller.PostIngredient(newIngredient);

        Assert.IsInstanceOf<ActionResult<Ingredient>>(result);
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var addedIngredient = okResult.Value as Ingredient;
        Assert.IsNotNull(addedIngredient);
        Assert.That(newIngredient.Name, Is.EqualTo(addedIngredient.Name));


        var ingredientInDb = await _dbContext.Ingredients.FindAsync(3);
        Assert.IsNotNull(ingredientInDb);
        Assert.That(newIngredient.Name, Is.EqualTo(addedIngredient.Name));
    }
}

