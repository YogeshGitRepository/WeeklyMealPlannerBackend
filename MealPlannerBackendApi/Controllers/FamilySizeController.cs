using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealPlannerBackend.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[Route("api/familysize")]
[Authorize]
[ApiController]
public class FamilySizeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FamilySizeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<int>> GetFamilySizes()
    {
       
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized("User not authenticated.");
        }

        var familySize = await _context.Users
                             .Where(c => c.Email == userName)
                             .Select(x => x.FamilySize)
                             .FirstOrDefaultAsync();

        if (familySize == 0) 
        {
            return NotFound("Family size not found for the user.");
        }

        return familySize;
    }
}
