using MealPlannerBackend.Data;
using Microsoft.AspNetCore.Mvc;
using MealPlannerBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace MealPlannerBackend.Controllers
{
   
    [Route("api/AvailableGrocery")]
    [ApiController]
    public class AvailableGroceryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AvailableGroceryController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetGroceries()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Unauthorized("Token is missing.");
                }

                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    if (jwtToken.ValidTo < DateTime.UtcNow)
                    {
                        return Unauthorized("Token is expired.");
                    }
                }
                else
                {
                    return Unauthorized("Invalid token.");
                }
                var groceries = await _context.Ingredients.ToListAsync();
                return Ok(groceries);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database: " + ex.Message);
            }
        }

      
        [HttpPost]
        public async Task<IActionResult> AddGrocery([FromBody] Ingredient ingredient)
        {
            if (ingredient == null)
            {
                return BadRequest("Invalid ingredient data.");
            }

            try
            {
                _context.Ingredients.Add(ingredient);
               await _context.SaveChangesAsync();
                return Ok(ingredient);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving data to the database: " + ex.Message);
            }
        }
    }
}
