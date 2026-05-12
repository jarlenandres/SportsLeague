using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly IMatchService _matchService;
    private readonly IMapper _mapper;

    public MatchController(IMatchService matchService, IMapper mapper)
    {
        _matchService = matchService;
        _mapper = mapper;
    }

    [HttpGet("tournament/{tournamentId}")]
    public async Task<ActionResult<IEnumerable<MatchResponseDTO>>> GetByTournament(int tournamentId)
    {
        try
        {
            var matches = await _matchService.GetAllByTournamentAsync(tournamentId);
            var matchDTOs = _mapper.Map<IEnumerable<MatchResponseDTO>>(matches);
            return Ok(matchDTOs);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mesasge = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener los partidos: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MatchResponseDTO>> GetById(int id)
    {
        try
        {
            var match = await _matchService.GetByIdAsync(id);
            if (match == null)
            {
                return NotFound(new { message = $"No se encontró el partido con ID {id}." });
            }
            var matchDTO = _mapper.Map<MatchResponseDTO>(match);
            return Ok(matchDTO);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener el partido: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<MatchResponseDTO>> Create(MatchRequestDTO dto)
    {
        try
        {
            var match = _mapper.Map<Match>(dto);
            var created = await _matchService.CreateAsync(match);
            var matchWithDetails = await _matchService.GetByIdAsync(created.Id);
            var responseDTO = _mapper.Map<MatchResponseDTO>(matchWithDetails);
            return CreatedAtAction(nameof(GetById), new { id = responseDTO.Id }, responseDTO);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el partido: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, MatchRequestDTO dto)
    {
        try
        {
            var match = _mapper.Map<Match>(dto);
            await _matchService.UpdateAsync(id, match);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el partido: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _matchService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al eliminar el partido: {ex.Message}");
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, UpdateMatchStatusDTO dto)
    {
        try
        {
            await _matchService.UpdateStatusAsync(id, dto.Status);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el estado del partido: {ex.Message}");
        }
    }
}
