using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealPlannerBackend.Data;
using MealPlannerBackend.Models;
using Microsoft.AspNetCore.Authorization;

[Route("api/ingredients")]
[ApiController]
[Authorize] 
public class IngredientController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public IngredientController(ApplicationDbContext context)
    {
        _context = context;
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ingredient>>> GetIngredients()
    {
        return await _context.Ingredients.ToListAsync();
    }

   
    [HttpPost]
    public async Task<ActionResult<Ingredient>> PostIngredient(Ingredient ingredient)
    {
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();
        return Ok(ingredient); 
    }
}
