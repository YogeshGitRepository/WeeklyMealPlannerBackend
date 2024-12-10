using MealPlannerBackend.Data;
using MealPlannerBackend.MLModels;
using MealPlannerBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MealPlannerBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RecipeController> _logger;

        public RecipeController(ApplicationDbContext context, IMemoryCache cache, ILogger<RecipeController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

       
        [HttpPost("train")]
        public IActionResult TrainAndSaveModel()
        {
            try
            {
                RecipeRecommender.TrainAndSaveModel();
                return Ok(new { message = "Model trained and saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("recommend")]
        public async Task<IActionResult> GetRecipeRecommendation([FromBody] RecipeSearchRequest request)
        {
           

            if (request?.Ingredients == null || request.Ingredients.Count == 0)
            {
                return BadRequest(new { error = "Please provide at least one ingredient." });
            }

            try
            {
                var ingredients = request.Ingredients.ToArray();
                var cacheKey = $"{request.Role}_recommendation_{string.Join("_", ingredients)}";

               
                if (!_cache.TryGetValue(cacheKey, out List<RecipeData> recommendedRecipes))
                {
                
                    await LogSearchIngredients(ingredients, request.Role);
                    recommendedRecipes = RecipeRecommender.PredictRecipe(ingredients);
                  

                    if (recommendedRecipes == null || recommendedRecipes.Count == 0)
                    {
                        return NotFound(new { error = "No recipe recommendation found." });
                    }

                  
                    _logger.LogInformation($"Cache miss for ingredients: {string.Join(", ", ingredients)}. Caching results.");

                  
                    _cache.Set(cacheKey, recommendedRecipes, TimeSpan.FromMinutes(30));
                }
                else
                {
                  
                    _logger.LogInformation($"Cache hit for ingredients: {string.Join(", ", ingredients)}.");
                }

               
                var pagedRecipes = recommendedRecipes
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                return Ok(new { Recipes = pagedRecipes, TotalCount = recommendedRecipes.Count });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { error = "Model file not found. Please train the model first." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during recipe recommendation");
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        private async Task LogSearchIngredients(string[] ingredients, string role)
        {
           
            role = string.IsNullOrEmpty(role) ? "anonymous" : role;

           
            var log = new SearchLog
            {
                SearchDate = DateTime.Today,
                SearchIngredients = new List<SearchIngredient>(),
               
                Role = role
            };

            foreach (var ingredient in ingredients)
            {
                var searchIngredient = log.SearchIngredients
                    .FirstOrDefault(si => si.IngredientName == ingredient);

                if (searchIngredient == null)
                {
                    log.SearchIngredients.Add(new SearchIngredient
                    {
                        IngredientName = ingredient,
                        SearchCount = 1,
                        Role = role
                    });
                }
                else
                {
                    searchIngredient.SearchCount++;
                }
            }

            var recommendedRecipes = RecipeRecommender.PredictRecipe(ingredients);

            if (recommendedRecipes != null && recommendedRecipes.Count > 0)
            {
                log.RecipeNames = recommendedRecipes.Select(r => new RecipeName{Name = r.RecipeName}).ToList();
            }

            _context.SearchLogs.Add(log);
            await _context.SaveChangesAsync();
        }


        [HttpGet("aggregateddata")]
        public async Task<IActionResult> GetAggregatedData()
        {
            try
            {
               
                var aggregatedIngredientCount = await _context.SearchLogs
                    .AsNoTracking()
                    .SelectMany(log => log.SearchIngredients)
                    .GroupBy(ingredient => ingredient.IngredientName.ToLower())
                    .Select(group => new
                    {
                        Ingredient = group.Key,
                        SearchCount = group.Sum(g => g.SearchCount)
                    })
                    .ToListAsync();


                var groupedRecipeData = await _context.RecipeNames
     .GroupBy(r => r.Name)
     .Select(group => new
     {
         Recipe = group.Key,
         Count = (int)(group.Count() * 0.1) 
     })
     .OrderByDescending(recipe => recipe.Count)
     .ToListAsync();



                return Ok(new { AggregatedIngredientCount = aggregatedIngredientCount, RecipeData = groupedRecipeData });
            }
            catch (Exception ex)
            {
               
                _logger.LogError(ex, "Error occurred while fetching aggregated data");
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpPost("recommendByIngredients")]
        public async Task<IActionResult> GetRecipeRecommendationsByIngredients([FromBody] RecipeSearchRequest request)
        {
            if (request?.Ingredients == null || request.Ingredients.Count == 0)
            {
                return BadRequest(new { error = "Please provide at least one ingredient." });
            }

            try
            {
                var ingredients = request.Ingredients.ToArray();
                var cacheKey = $"{request.Role}_recommendation_{string.Join("_", ingredients)}";

               
                if (!_cache.TryGetValue(cacheKey, out List<RecipeData> recommendedRecipes))
                {
                    
                    recommendedRecipes = RecipeRecommender.PredictRecipe(ingredients);

                    if (recommendedRecipes == null || recommendedRecipes.Count == 0)
                    {
                        return NotFound(new { error = "No recipe recommendation found." });
                    }

                  
                    _cache.Set(cacheKey, recommendedRecipes, TimeSpan.FromMinutes(30));
                }

               
                var pagedRecipes = recommendedRecipes
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                return Ok(new { Recipes = pagedRecipes, TotalCount = recommendedRecipes.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during recipe recommendation");
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("remaining-ingredients")]
        public async Task<IActionResult> GetRemainingIngredients()
        {
           
            var ingredients = await _context.Ingredients.ToListAsync();

            if (ingredients == null || ingredients.Count == 0)
            {
                return NotFound("No ingredients found.");
            }

            
            var remainingIngredients = new List<object>();

            foreach (var ingredient in ingredients)
            {
                
                var totalUsedQuantity = await _context.RecipeIngredients
      .Where(ri => EF.Functions.Like(ri.Name.ToLower(), $"%{ingredient.Name.ToLower()}%"))
      .SumAsync(ri => ri.Quantity ?? 0);


               
                decimal totalQuantity = ingredient.Quantity; 
                decimal remainingQuantity = totalQuantity - totalUsedQuantity; 
                decimal usedQuantity = totalUsedQuantity;

           
                remainingIngredients.Add(new
                {
                    IngredientName = ingredient.Name,
                    TotalQuantity = totalQuantity,
                    RemainingQuantity = remainingQuantity,
                    UsedQuantity = usedQuantity,
                    Measurement = ingredient.Measurement
                });
            }

            return Ok(remainingIngredients);
        }


    }
}
