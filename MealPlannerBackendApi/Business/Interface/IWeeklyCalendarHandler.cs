using MealPlannerBackend.Models;

namespace MealPlannerBackend.Business.Interface
{
    public interface IWeeklyCalendarHandler
    {
        Task AddCalendar(WeeklyCalendarViewModal weeklyCalendar);
        Task AddRecipe(RecipeData receipe);
    }
}