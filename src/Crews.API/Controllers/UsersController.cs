 using System;
using System.Collections.Generic;
using System.Linq;
 using System.Security.Claims;
 using System.Threading.Tasks;
using AutoMapper;
using Crews.API.Data.Entities;
using Crews.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crews.API.Controllers;

[ApiController]
//[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly LinkGenerator _linkGenerator;

    public UsersController(
        IMapper mapper, 
        ILogger<UsersController> logger, 
        UserManager<User> userManager, 
        LinkGenerator linkGenerator)
    {
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
        _linkGenerator = linkGenerator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserViewModel>>> GetUsers()
    {
        try
        {
            var users = await _userManager.Users.ToArrayAsync();
            var models = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get crews: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserViewModel>> GetUser(int id)
    {
        try
        {
            var user = await _userManager.Users.Where(u => u.Id == id).SingleOrDefaultAsync();

            if (user is null) return NotFound($"The user with id={id} does not exist."); 
            var model = _mapper.Map<UserViewModel>(user);
            var claims = await _userManager.GetClaimsAsync(user);

            var favoriteColor = claims.SingleOrDefault(c =>
                string.Equals(c.Type, nameof(model.FavoriteColor), StringComparison.InvariantCultureIgnoreCase));

            if (favoriteColor != null)
            {
                model.FavoriteColor = favoriteColor.Value;
            }

            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get crews: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("{id:int}/claims")]
    public async Task<ActionResult<UserViewModel>> GetUserClaims(int id)
    {
        try
        {
            var user = await _userManager.Users.Where(u => u.Id == id).SingleOrDefaultAsync();

            if (user is null) return NotFound($"The user with id={id} does not exist.");
            var claims = await _userManager.GetClaimsAsync(user);
            return Ok(claims.Select(c => new { Name = c.Type, Value = c.Value }).ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get crews: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserViewModel>> Register(UserViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(string.Join('|',
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

            var user = _mapper.Map<User>(model);
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return BadRequest($"Could not register user: {result}");

            var location = _linkGenerator.GetPathByAction(nameof(UsersController.GetUser), "Users", new { id = user.Id });

            if (string.IsNullOrWhiteSpace(location))
            {
                return BadRequest("Could not use current user id");
            }

            var claim = new Claim(nameof(UserViewModel.FavoriteColor), model.FavoriteColor);
            var claimResult = await _userManager.AddClaimAsync(user, claim);

            if (!claimResult.Succeeded) return BadRequest($"Could not add claim: [{claim.Type}]={claim.Value}");
            
            return Created(location, _mapper.Map<UserViewModel>(user));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get crews: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}