namespace MealPlannerBackend.Models
{
    public class Recipe
    {
        public string Id { get; set; }
        public string Name { get; set; } 
        public string Instructions { get; set; }

       
        public ICollection<WeeklyCalendar> WeeklyCalendars { get; set; } = new List<WeeklyCalendar>();

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

    }
}
