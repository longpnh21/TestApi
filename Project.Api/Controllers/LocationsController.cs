using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos.Location;
using Project.Core.Entities;
using Project.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly IMapper _mapper;

        public LocationsController(ILocationService locationService, IMapper mapper)
        {
            _locationService = locationService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] int pageIndex = 1, int pageSize = 10, string searchValue = null, string orderBy = null, bool isDelete = false)
        {
            try
            {
                Expression<Func<Location, bool>> filter = null;
                Func<IQueryable<Location>, IOrderedQueryable<Location>> order = null;
                string include = string.Empty;

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    filter = e => e.Id.ToString().Equals(searchValue)
                            || e.Floor.ToString().Equals(searchValue)
                            || e.Cube.Contains(searchValue);
                }
                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    switch (orderBy)
                    {
                        case "id_desc":
                            order = x => x.OrderByDescending(e => e.Id);
                            break;
                        case "floor":
                            order = x => x.OrderBy(e => e.Floor);
                            break;
                        case "floor_desc":
                            order = x => x.OrderByDescending(e => e.Floor);
                            break;
                        case "cube":
                            order = x => x.OrderBy(e => e.Cube);
                            break;
                        case "cube_desc":
                            order = x => x.OrderByDescending(e => e.Cube);
                            break;
                        default:
                            order = x => x.OrderBy(e => e.Id);
                            break;
                    }
                }

                var locations = await _locationService.GetAllAsync(pageIndex, pageSize, filter: filter, orderBy: order, include, isDelete);
                return Ok(locations.Select(e => _mapper.Map<LocationDto>(e)));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}", Name = "GetLocationById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                var location = await _locationService.GetByIdAsync(id);
                return location is null
                    ? NotFound()
                    : Ok(_mapper.Map<LocationDto>(location));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddAsync([FromBody] CreateLocationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var entity = _mapper.Map<Location>(dto);

                await _locationService.AddAsync(entity);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = entity.Id }, dto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateLocationDto dto)
        {
            try
            {
                if (id < 0
                    || !ModelState.IsValid
                    || id != dto.Id)
                {
                    return BadRequest();
                }

                var inDatabase = await _locationService.GetByIdAsync(id);
                if (inDatabase is null)
                {
                    return NotFound();
                }

                var entity = _mapper.Map<Location>(dto);
                await _locationService.UpdateAsync(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                await _locationService.DeleteAsync(id);
                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}/Delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HardDeleteAsync(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                await _locationService.HardDeleteAsync(id);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
