using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealPlannerBackend.Controllers;
using MealPlannerBackend.Models;
using MealPlannerBackend.Data;
using MealPlannerBackend.Business.Interface;
using Moq;

namespace MealPlannerBackendTests.ControllerTests
{
    [TestFixture]
    public class WeeklyCalendarControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private Mock<IWeeklyCalendarHandler> _mockWeeklyCalendarHandler;

        [SetUp]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _mockWeeklyCalendarHandler = new Mock<IWeeklyCalendarHandler>();
        }

        private ApplicationDbContext CreateDbContext()
        {
            return new ApplicationDbContext(_dbContextOptions);
        }

        [Test]
        public async Task SaveWeeklyCalendar_ReturnsOk_WhenValidDataProvided()
        {
            var controller = new WeeklyCalendarController(CreateDbContext(), _mockWeeklyCalendarHandler.Object);
            var weeklyCalendar = new WeeklyCalendarViewModal
            {
                Recipe = new RecipeData { RecipeName = "Pasta" },
            };

            _mockWeeklyCalendarHandler
                .Setup(h => h.AddRecipe(It.IsAny<RecipeData>()))
                .Returns(Task.CompletedTask);

            _mockWeeklyCalendarHandler
                .Setup(h => h.AddCalendar(It.IsAny<WeeklyCalendarViewModal>()))
                .Returns(Task.CompletedTask);

            var result = await controller.SaveWeeklyCalendar(weeklyCalendar);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo("Weekly calendar saved successfully."));
        }

        [Test]
        public async Task GetWeeklyCalendar_ReturnsOk_WithListOfCalendars()
        {
            var dbContext = CreateDbContext();
            var controller = new WeeklyCalendarController(dbContext, _mockWeeklyCalendarHandler.Object);

            var recipe = new Recipe {Id = "receipe", Name = "Salad", Instructions= "instructions" };
            dbContext.Recipes.Add( new RecipeData() {Id = "receipe",  RecipeName = recipe.Name, Ingredients = ["ingredients"], Instructions = "instructions" });
            dbContext.WeeklyCalendars.Add(new WeeklyCalendar
            {
                Id = 1,
                RecipeId = recipe.Id,
                Recipe = recipe,
            });
            await dbContext.SaveChangesAsync();

            var result = await controller.GetWeeklyCalendar();

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var calendars = okResult.Value as List<WeeklyCalendar>;
            Assert.That(calendars, Is.Not.Null);
            Assert.That(calendars.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task SaveWeeklyCalendar_ReturnsBadRequest_WhenInvalidDataProvided()
        {
            var controller = new WeeklyCalendarController(CreateDbContext(), _mockWeeklyCalendarHandler.Object);
            WeeklyCalendarViewModal weeklyCalendar = null;

            var result = await controller.SaveWeeklyCalendar(weeklyCalendar);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid weekly calendar data."));
        }
    }
}
