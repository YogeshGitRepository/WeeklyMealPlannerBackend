namespace MealPlannerBackend.Models
{
    public class SearchLog
    {
        public int Id { get; set; } 
        public DateTime SearchDate { get; set; } 
        public List<SearchIngredient> SearchIngredients { get; set; }  
        public ICollection<RecipeName> RecipeNames { get; set; }  
        public string Role { get; set; }  
       
    }
    public class RecipeName
    {
        public int Id { get; set; } 
        public string Name { get; set; } 
        public int SearchLogId { get; set; }  
        public SearchLog SearchLog { get; set; }  
    }
}
