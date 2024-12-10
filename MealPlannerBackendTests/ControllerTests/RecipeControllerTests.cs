using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MealPlannerBackend.Controllers;
using MealPlannerBackend.Data;
using MealPlannerBackend.Models;
using MealPlannerBackend.MLModels;
using Moq;
using Newtonsoft.Json.Linq;

namespace MealPlannerBackendTests.ControllerTests
{
    [TestFixture]
    public class RecipeControllerTests
    {
        private RecipeController _controller;
        private Mock<ILogger<RecipeController>> _loggerMock;
        private ApplicationDbContext _dbContext;
        private Mock<IMemoryCache> _memoryCacheMock;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _memoryCacheMock = new Mock<IMemoryCache>();

            _loggerMock = new Mock<ILogger<RecipeController>>();

            _controller = new RecipeController(_dbContext, _memoryCacheMock.Object, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetRecipeRecommendation_ReturnsBadRequest_WhenNoIngredientsProvided()
        {
            var request = new RecipeSearchRequest
            {
                Ingredients = null, 
                Role = "user",
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _controller.GetRecipeRecommendation(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            dynamic errorResponse = badRequestResult.Value; 
            Assert.AreEqual("{ error = Please provide at least one ingredient. }", badRequestResult.Value.ToString());
        }      

        [Test]
        public void TrainAndSaveModel_ReturnsOk_WhenModelTrainedSuccessfully()
        {
            
            var result = _controller.TrainAndSaveModel();

            Assert.IsInstanceOf<ObjectResult>(result);
        }

    }
}
