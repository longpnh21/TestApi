using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos.LostProperties;
using Project.Core.Entities;
using Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Project.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LostPropertiesController : ControllerBase
    {
        private readonly ILostPropertyService _lostPropertyService;
        private readonly IMapper _mapper;

        public LostPropertiesController(ILostPropertyService lostPropertyService, IMapper mapper)
        {
            _lostPropertyService = lostPropertyService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                IEnumerable<LostProperty> lostProperties = await _lostPropertyService.GetAllAsync();
                return Ok(lostProperties.Select(e => _mapper.Map<LostPropertyDto>(e)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}", Name = "GetLostPropertyById")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                LostProperty lostProperty = await _lostPropertyService.GetByIdAsync(id);
                if (lostProperty is null)
                {
                    return NotFound();
                }
                return Ok(_mapper.Map<LostPropertyDto>(lostProperty));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] CreateLostPropertyDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                LostProperty entity = _mapper.Map<LostProperty>(dto);

                await _lostPropertyService.AddAsync(entity);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = entity.Id }, dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateLostPropertyDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                if (id != dto.Id)
                {
                    return BadRequest();
                }

                LostProperty inDatabase = await _lostPropertyService.GetByIdAsync(id);
                if (inDatabase is null)
                {
                    return NotFound();
                }

                LostProperty entity = _mapper.Map<LostProperty>(dto);
                await _lostPropertyService.UpdateAsync(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                await _lostPropertyService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}/Delete")]
        public async Task<IActionResult> HardDeleteAsync(int id)
        {
            try
            {
                await _lostPropertyService.HardDeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
