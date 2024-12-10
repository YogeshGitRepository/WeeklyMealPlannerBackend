namespace MealPlannerBackend.Models
{
    public class WeeklyCalendar
    {
        public int Id { get; set; }
        public int DayOfWeek { get; set; } 
        public int SlotId { get; set; }    
        public string RecipeId { get; set; } 
        public Recipe Recipe { get; set; }  
    }

    public class WeeklyCalendarViewModal
    {
        public int Id { get; set; }
        public int DayOfWeek { get; set; }
        public int SlotId { get; set; }    
        public string RecipeId { get; set; }  
        public RecipeData Recipe { get; set; }  
    }
}
