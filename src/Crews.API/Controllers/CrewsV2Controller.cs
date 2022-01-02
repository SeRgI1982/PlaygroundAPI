using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Crews.API.Data;
using Crews.API.Data.Entities;
using Crews.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Crews.API.Controllers;

[ApiVersion("2.0")]
[ApiController]
//[Route("api/v{version:apiVersion}/crews")]
[Route("api/crews")]
public class CrewsV2Controller : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IOneCrewRepository _repository;
    private readonly ILogger<CrewsV2Controller> _logger;
    private readonly LinkGenerator _linkGenerator;

    public CrewsV2Controller(IOneCrewRepository repository, IMapper mapper, ILogger<CrewsV2Controller> logger, LinkGenerator linkGenerator)
    {
        _mapper = mapper;
            
        _logger = logger;
        _linkGenerator = linkGenerator;
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CrewViewModel>>> GetCrews2(bool includeTrainings = false)
    {
        try
        {
            var crews = await _repository.GetCrewsAsync(includeTrainings);
            var viewModels = _mapper.Map<IEnumerable<CrewViewModel>>(crews);

            foreach (var viewModel in viewModels)
            {
                viewModel.Country = "Ver. 2.0";
            }

            return Ok(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get crews: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CrewViewModel>> GetCrew(int id)
    {
        try
        {
            var crew = await _repository.GetCrewByIdAsync(id);
            return crew is not null ? Ok(_mapper.Map<CrewViewModel>(crew)) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to read event. {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Insert(CrewViewModel vm)
    {
        try
        {
            var crew = _mapper.Map<Crew>(vm);
            _repository.AddOrUpdate(crew);
            await _repository.SaveChangesAsync();

            // The same like: /api/crews/{crew.Id}
            var location = _linkGenerator.GetPathByAction(nameof(CrewsController.GetCrew), "Crews", new { id = crew.Id });

            if (string.IsNullOrWhiteSpace(location))
            {
                return BadRequest("Could not use current crew id");
            }

            return Created(location, _mapper.Map<CrewViewModel>(crew));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to insert Crew. {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CrewViewModel>> Put(int id, CrewViewModel vm)
    {
        try
        {
            var oldCrew = await _repository.GetCrewByIdAsync(id);

            if (oldCrew == null) return NotFound(($"Could not find crew with id {id}"));

            _mapper.Map(vm, oldCrew);

            await _repository.SaveChangesAsync();

            return _mapper.Map<CrewViewModel>(oldCrew);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update Crew. {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        } 
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var oldCrew = await _repository.GetCrewByIdAsync(id);

            if (oldCrew == null) return NotFound(($"Could not find crew with id {id}"));

            _repository.Delete(oldCrew);
            await _repository.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete Crew. {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}