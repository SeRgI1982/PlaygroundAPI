using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Crews.API.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crews.API.Controllers;

[ApiController]
[Route("api/users/{userId:int}/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<RolesController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly LinkGenerator _linkGenerator;

    public RolesController(IMapper mapper, ILogger<RolesController> logger, UserManager<User> userManager, RoleManager<Role> roleManager, LinkGenerator linkGenerator)
    {
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        _linkGenerator = linkGenerator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetRoles(int userId)
    {
        try
        {
            var user = await _userManager.Users.Where(u => u.Id == userId).SingleOrDefaultAsync();

            if (user is null) return NotFound($"The user with id={userId} does not exist.");

            return Ok(await _userManager.GetRolesAsync(user));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get trainings: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddUserToRole(int userId, [FromBody] string roleName)
    {
        try
        {
            var user = await _userManager.Users.Where(u => u.Id == userId).SingleOrDefaultAsync();

            if (user is null) return NotFound($"The user with id={userId} does not exist.");

            var role = await _roleManager.Roles.Where(r => r.Name == roleName).SingleOrDefaultAsync();

            if (role is null) return NotFound($"The role with name={roleName} does not exist.");

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                var location = _linkGenerator.GetPathByAction(HttpContext, "GetRoles", values: new { userId });
                if (location != null) 
                    return Created(location, userId);
            }

            return BadRequest($"Could not add user ({user.UserName}) to role={roleName}.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get trainings: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}