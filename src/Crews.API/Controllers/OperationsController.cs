using System;
using System.Collections.Generic;
using System.Linq;
using Crews.API.Data;
using Crews.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crews.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperationsController : ControllerBase
{
    private readonly IOneCrewRepository _repository;
    private readonly IEnumerable<IModule> _modules;
    private readonly ILogger<OperationsController> _logger;
    private readonly IConfiguration _config;

    public OperationsController(
        IOneCrewRepository repository, 
        IEnumerable<IModule> modules, 
        ILogger<OperationsController> logger,
        IConfiguration config)
    {
        _repository = repository;
        _modules = modules;
        _logger = logger;
        _config = config;
    }

    [HttpOptions("reset")]
    public IActionResult ResetDatabase()
    {
        try
        {
            _repository.Reset();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not reset database: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpOptions("modules")]
    public IActionResult RegisteredModules()
    {
        try
        {
            return Ok(_modules.Select(module => module.Name).ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not list registered modules: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpOptions("roles")]
    public IActionResult Roles()
    {
        try
        {
            var pairs = _config.GetSection("Roles")
                               .GetChildren()
                               .Where(x => x.Key.Contains('|') && x.Key.IndexOf('|') != x.Key.Length - 1)
                               .Select(x => $"[{x.Key[(x.Key.IndexOf('|') + 1)..]}] {x.Value}").ToArray();
            return Ok(pairs);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not list registered modules: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}