using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Crews.API.Data.Entities;
using Crews.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Crews.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IOptions<TokenOptions> _options;

    public AccountController(
        ILogger<AccountController> logger, 
        SignInManager<User> signInManager,
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        IAuthorizationService authorizationService,
        IOptions<TokenOptions> options)
    {
        _logger = logger;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _userManager = userManager;
        _authorizationService = authorizationService;
        _options = options;
    }

    [HttpPost("createtoken")]
    public async Task<IActionResult> CreateToken(LoginViewModel model)
    {
        try 
        {
            if (!ModelState.IsValid) return BadRequest("Failed to login");
            
            // sign-in with a cookie
            //var result =
            //    await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

            var user = await _signInManager.UserManager.FindByNameAsync(model.UserName);

            if (user is null) return Unauthorized();
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded) return Unauthorized();
            
            // Create the token
            var claims = await GetValidClaims(user);
                        
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _options.Value.Issuer,
                _options.Value.Audience, 
                claims, 
                signingCredentials: credentials,
                expires: DateTime.UtcNow.AddMinutes(2));

            return Created("", new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });

        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to login: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    private async Task<List<Claim>> GetValidClaims(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("Id", user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var userClaims = await _userManager.GetClaimsAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);
        claims.AddRange(userClaims);

        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
            var role = await _roleManager.FindByNameAsync(userRole);

            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);

                foreach (Claim roleClaim in roleClaims)
                {
                    claims.Add(roleClaim);
                }
            }
        }

        return claims;
    }

    [HttpPost("addrole")]
    public async Task<IActionResult> AddRole([FromBody] string roleName)
    {
        try
        {
            var role = await _roleManager.Roles.Where(r => r.Name == roleName).SingleOrDefaultAsync();

            if (role is not null) return BadRequest($"The role with name={roleName} already exist.");

            var newRole = new Role()
            {
                Name = roleName
            };

            var result = await _roleManager.CreateAsync(newRole);

            if (!result.Succeeded) return BadRequest($"Cannot add new role");

            return Created("", new
            {
                newRole
            });

        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to login: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("isadmin")]
    public async Task<IActionResult> IsAdmin()
    {
        try
        {
            var user = await _userManager.Users.Where(u => u.UserName == User.Identity.Name).SingleOrDefaultAsync();

            if (user is null) return NotFound($"The user does not exist.");

            var userRoles = await _userManager.GetRolesAsync(user);

            var adminRole = userRoles.SingleOrDefault(r => r == "Admin");

            if (adminRole is null)
            {
                return Ok("False");
            }

            // AuthorizeAsync checks policies and when find RoleRequired policy which is represented by
            // attribute RolesAuthorizationRequirement, it calls under the hood User.IsInRole(roleName)
            // so it can be alternative approach
            var isAdmin = await _authorizationService.AuthorizeAsync(this.User, "IsAdmin");
            
            if (isAdmin.Succeeded)
            {
                return Ok("True");
            }

            return BadRequest(isAdmin.Failure?.FailedRequirements.ToString());

            // alternatively
            // return Ok(User.IsInRole("Admin"));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to login: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("haspolicy")]
    public async Task<IActionResult> CheckPolicy(string policyName)
    {
        try
        {
            var hasPolicy = await _authorizationService.AuthorizeAsync(User, policyName);
            return Ok($"[{policyName}]={hasPolicy.Succeeded}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to login: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}