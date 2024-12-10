using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlannerBackend.Models
{
    public class SearchIngredient
    {
        public int Id { get; set; }
        public string IngredientName { get; set; }
        public int SearchCount { get; set; }
        public string Role { get; set; }

        
        public int SearchLogId { get; set; }

      
        [ForeignKey("SearchLogId")]
        public SearchLog SearchLog { get; set; }
    }
}
