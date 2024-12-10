using MealPlannerBackend.Business.Interface;
using MealPlannerBackend.Data;
using MealPlannerBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MealPlannerBackend.Business
{
    public class WeeklyCalendarHandler : IWeeklyCalendarHandler
    {
        private readonly ApplicationDbContext _dbContext;

        public WeeklyCalendarHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddCalendar(WeeklyCalendarViewModal weeklyCalendar)
        {
            
            var existingCalendar = await _dbContext.WeeklyCalendars
                .FirstOrDefaultAsync(s => s.DayOfWeek == weeklyCalendar.DayOfWeek && s.SlotId == weeklyCalendar.SlotId);

            if (existingCalendar != null)
            {
              
                existingCalendar.SlotId = weeklyCalendar.SlotId;
                existingCalendar.DayOfWeek = weeklyCalendar.DayOfWeek;
                existingCalendar.RecipeId = weeklyCalendar.RecipeId;
                _dbContext.WeeklyCalendars.Update(existingCalendar);
            }
            else
            {
               
                var addCalendar = new WeeklyCalendar()
                {
                    RecipeId = weeklyCalendar.RecipeId,
                    DayOfWeek = weeklyCalendar.DayOfWeek,
                    SlotId = weeklyCalendar.SlotId
                };
               
                _dbContext.WeeklyCalendars.Add(addCalendar);
            }

          
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddRecipe(RecipeData recipeData)
        {
           
            var existingRecipe = await _dbContext.Recipe
                .FirstOrDefaultAsync(s => s.Id == recipeData.Id);
            if (existingRecipe == null)
            {
               
                var addRecipe = new Recipe()
                {
                    Id = recipeData.Id,
                    Name = recipeData.RecipeName,
                    Instructions = recipeData.Instructions                    
                };
                _dbContext.Recipe.Add(addRecipe);

                if (!await _dbContext.RecipeIngredients
                .AnyAsync(s => s.RecipeId == recipeData.Id))
                {                    
                    HandlesIngredients(recipeData.Ingredients, recipeData.Id);
                }

              
                await _dbContext.SaveChangesAsync();
            }
        }

        private void HandlesIngredients(string[] ingredients, string recipeId)
        {
          
            foreach (var ingredient in ingredients)
            {
                if (ingredient != null)
                {
                    RecipeIngredient? addIngred = ConvertIngredientsIntoReadableFormat(ingredient);

                    if (addIngred != null)
                    {
                        addIngred.RecipeId = recipeId;
                        
                        _dbContext.RecipeIngredients.Add(addIngred);
                    }
                }

            }
        }
        
        private static RecipeIngredient ConvertIngredientsIntoReadableFormat(string ingredient)
        {
           
            var regex = new Regex(@"^(?<quantity>[\d./\s]+)?\s*(?<measurement>[a-zA-Z]+)?\s*(?<name>.+)$");
            var match = regex.Match(ingredient);

            if (!match.Success)
                return null;

            var quantityStr = match.Groups["quantity"].Value.Trim();
            var measurement = match.Groups["measurement"].Value.Trim();
            var name = match.Groups["name"].Value.Trim();

          
            decimal? quantity = null;
            if (!string.IsNullOrEmpty(quantityStr))
            {
                try
                {
                    quantity = ParseQuantity(quantityStr);
                }
                catch (FormatException)
                {
                    
                }
            }

            return new RecipeIngredient
            {
                Name = name,
                Quantity = quantity,
                Measurement = string.IsNullOrEmpty(measurement) ? null : measurement
            };
        }
        private static decimal ParseQuantity(string quantityStr)
        {
            
            if (quantityStr.Contains('/'))
            {
                var parts = quantityStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1)
                {
                    return FractionToDecimal(parts[0]);
                }
                else if (parts.Length == 2)
                {
                    return decimal.Parse(parts[0]) + FractionToDecimal(parts[1]);
                }
            }

           
            return decimal.Parse(quantityStr, CultureInfo.InvariantCulture);
        }
        private static decimal FractionToDecimal(string fraction)
        {
            var fractionParts = fraction.Split('/');
            if (fractionParts.Length == 2)
            {
                return decimal.Parse(fractionParts[0]) / decimal.Parse(fractionParts[1]);
            }

            throw new FormatException("Invalid fraction format.");
        }
    }
}
