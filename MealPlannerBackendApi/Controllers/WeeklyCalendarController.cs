using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealPlannerBackend.Models;
using MealPlannerBackend.Data;
using MealPlannerBackend.Business.Interface;

namespace MealPlannerBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeeklyCalendarController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWeeklyCalendarHandler _weeklyCalendarHandler;

        public WeeklyCalendarController(ApplicationDbContext dbContext, IWeeklyCalendarHandler weeklyCalendarHandler)
        {
            _dbContext = dbContext;
            _weeklyCalendarHandler = weeklyCalendarHandler;
        }

        [HttpPost]
        public async Task<IActionResult> SaveWeeklyCalendar([FromBody] WeeklyCalendarViewModal weeklyCalendar)
        {
            if (weeklyCalendar == null)
            {
                return BadRequest("Invalid weekly calendar data.");
            }

          
            await _weeklyCalendarHandler.AddRecipe(weeklyCalendar.Recipe);

            
            await _weeklyCalendarHandler.AddCalendar(weeklyCalendar);

            return Ok("Weekly calendar saved successfully.");
        }

       
        [HttpGet]
        public async Task<IActionResult> GetWeeklyCalendar()
        {
            var weeklyCalendars = await _dbContext.WeeklyCalendars.Include(w => w.Recipe).ToListAsync();
            return Ok(weeklyCalendars);
        }
    }
}
