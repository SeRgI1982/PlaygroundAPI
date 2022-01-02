using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Crews.API.Data;
using Crews.API.Data.Entities;
using Crews.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Crews.API.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//[Route("api/v{version:apiVersion}/crews/{crewId:int}/[controller]")]
[Route("api/crews/{crewId:int}/[controller]")]
public class TrainingsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IOneCrewRepository _repository;
    private readonly ILogger<TrainingsController> _logger;
    private readonly LinkGenerator _linkGenerator;

    public TrainingsController(IOneCrewRepository repository, IMapper mapper, ILogger<TrainingsController> logger, LinkGenerator linkGenerator)
    {
        _mapper = mapper;
        _logger = logger;
        _repository = repository;
        _linkGenerator = linkGenerator;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TrainingViewModel>> GetTraining(int crewId, int id)
    {
        try
        {
            var training = await _repository.GetTrainingByCrewIdAsync(crewId, id);
            return training is not null ? Ok(_mapper.Map<TrainingViewModel>(training)) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to read training. {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingViewModel>>> GetCrewTrainings(int crewId)
    {
        try
        {
            var trainings = await _repository.GetTrainingsByCrewIdAsync(crewId);
            var viewModels = _mapper.Map<IEnumerable<TrainingViewModel>>(trainings);
            
            return viewModels.Any() ? Ok(viewModels) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get trainings: {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanAddTraining")]
    public async Task<IActionResult> Insert(int crewId, TrainingViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest("Can't Save");

            var crew = await _repository.GetCrewByIdAsync(crewId, includeTrainings: true);

            if (crew is null) return BadRequest("Crew does not exist");

            var training = crew.Trainings.FirstOrDefault(t => t.Id == vm.Id);

            if (training is null)
            {
                training = _mapper.Map<Training>(vm);
                training.Crew = crew;
                _repository.AddOrUpdate(training);
            }
            else
            {
                training = _mapper.Map(vm, training);
            }

            await _repository.SaveChangesAsync();
            var location = _linkGenerator.GetPathByAction(HttpContext, "GetTraining", values: new { crewId, id = training.Id });
            return Created(location, _mapper.Map<TrainingViewModel>(training));

        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to insert Training. {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanAddTraining")]
    public async Task<ActionResult<TrainingViewModel>> Update(int crewId, int id, TrainingViewModel model)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest("Can't Save");

            var crew = await _repository.GetCrewByIdAsync(crewId, includeTrainings: true);

            if (crew is null) return BadRequest("Crew does not exist");

            var training = crew.Trainings.FirstOrDefault(t => t.Id == model.Id);

            if (training is null)
            {
                return NotFound("The training does not exist.");
            }

            training = _mapper.Map(model, training);

            await _repository.SaveChangesAsync();

            return Ok(_mapper.Map<TrainingViewModel>(training));

        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update Training. {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        } 
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrainingViewModel>> Delete(int crewId, int id)
    {
        try
        {
            var training = await _repository.GetTrainingByCrewIdAsync(crewId, id);

            if (training is null)
            {
                return NotFound("The training does not exist.");
            }

            _repository.Delete(training);

            await _repository.SaveChangesAsync();

            return Ok();

        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete Training. {0}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}