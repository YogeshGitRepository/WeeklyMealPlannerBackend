using Microsoft.ML.Data;
using System.ComponentModel.DataAnnotations;

namespace MealPlannerBackend.Models
{
    public class RecipeData
    {
        [Key]
        [LoadColumn(0)]
        public string Id { get; set; }

        [LoadColumn(1)]
        public string RecipeName { get; set; } 

        [LoadColumn(2)]
        public string? ImageURL { get; set; } 

        [LoadColumn(3)]
        public string[] Ingredients { get; set; } 

        [LoadColumn(4)]
        public string Instructions { get; set; }

    }
}
