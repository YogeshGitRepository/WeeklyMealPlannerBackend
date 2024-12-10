using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlannerBackend.Models
{
    public class RecipeIngredient
    {
        public string RecipeId { get; set; }
        public int Id { get; set; }
        public Recipe Recipe { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? Quantity { get; set; } 
        public string? Measurement { get; set; }
    }
}
