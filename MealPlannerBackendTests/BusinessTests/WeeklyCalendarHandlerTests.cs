using MealPlannerBackend.Business;
using MealPlannerBackend.Data;
using MealPlannerBackend.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace MealPlannerBackendTests.BusinessTests;

[TestFixture]
public class WeeklyCalendarHandlerTests
{
    private WeeklyCalendarHandler _weeklyCalendarHandler;
    private ApplicationDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
       
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();

      
        _weeklyCalendarHandler = new WeeklyCalendarHandler(_dbContext);
    }

    [TearDown]
    public void Teardown()
    {
       
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
    [Test]
    public async Task AddRecipe_ShouldAddNewRecipe_WhenRecipeDoesNotExist()
    {

       
        var recipeData = new RecipeData
        {
            Id = "recipe_1",
            RecipeName = "Test Recipe",
            Instructions = "Test Instructions",
            Ingredients = ["1 cup flour", "2 eggs"]
        };

        await _weeklyCalendarHandler.AddRecipe(recipeData);

        var addedRecipe = await _dbContext.Recipe.FirstOrDefaultAsync(r => r.Id == recipeData.Id);

        Assert.IsNotNull(addedRecipe, "Recipe was not added to the database.");
        Assert.AreEqual(recipeData.RecipeName, addedRecipe.Name, "Recipe name mismatch.");
        Assert.AreEqual(recipeData.Instructions, addedRecipe.Instructions, "Recipe instructions mismatch.");
    }

    [Test]
    public async Task AddRecipe_ShouldNotAddRecipe_WhenRecipeAlreadyExists()
    {
        var existingRecipe = new RecipeData
        {
            Id = "recipe",
            RecipeName = "Existing Recipe",
            Instructions = "Already exists",
            Ingredients = ["1 cup flour", "2 eggs"]
        };


        await _weeklyCalendarHandler.AddRecipe(existingRecipe);

        await _dbContext.Recipes.AddAsync(existingRecipe);
        await _dbContext.SaveChangesAsync();

        var recipeData = new RecipeData
        {
            Id = "recipe", 
            RecipeName = "Updated Recipe",
            Instructions = "Updated instructions",
            Ingredients = ["New ingredients"]
        };

        await _weeklyCalendarHandler.AddRecipe(recipeData);

        var recipeInDb = await _dbContext.Recipes.FirstOrDefaultAsync(r => r.Id == recipeData.Id);

        Assert.IsNotNull(recipeInDb);
        Assert.AreEqual("Existing Recipe", recipeInDb.RecipeName);
    }

    [Test]
    public async Task AddRecipe_ShouldAddIngredients_WhenIngredientsDoNotExist()
    {
        var recipeData = new RecipeData
        {
            Id = "recipe_3",
            RecipeName = "Ingredient Test Recipe",
            Instructions = "Mix ingredients",
            Ingredients = ["2 tablespoons unsalted butter, softened", "4 or 5 slices brioche, or good quality white bread (I like Pepperidge Farm)","1/4 inch thick, crusts removed", "3 extra-large eggs", "2 extra-large egg yolks", "1/4 cup brown sugar", "1 1/2 cups heavy cream", "1 1/4 cups whole milk", "1 teaspoon pure vanilla extract", "1/2 teaspoon ground cinnamon", "1/4 teaspoon freshly grated nutmeg", "1/4 teaspoon kosher salt", "3/4 cup chopped bittersweet chocolate", "1 tablespoon granulated sugar, for caramelizing the top"]
        };

        await _weeklyCalendarHandler.AddRecipe(recipeData);

        var ingredientsInDb = await _dbContext.RecipeIngredients
            .Where(i => i.RecipeId == recipeData.Id)
            .ToListAsync();

        Assert.IsNotEmpty(ingredientsInDb);
        Assert.AreEqual(18, ingredientsInDb.Count); 
    }
}