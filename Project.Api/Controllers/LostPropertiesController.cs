using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos.LostProperty;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync(int pageIndex = 1, int pageSize = 10, string searchValue = null, string orderBy = null, bool isDelete = false)
        {
            try
            {
                Expression<Func<LostProperty, bool>> filter = null;
                Func<IQueryable<LostProperty>, IOrderedQueryable<LostProperty>> order = null;
                string include = string.Empty;

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    filter = e => e.Id.ToString().Equals(searchValue)
                            || e.Name.Contains(searchValue)
                            || e.Description.Contains(searchValue)
                            || e.Status.Equals(searchValue)
                            || searchValue.Equals(e.LocationId)
                            || searchValue.Equals(e.EmployeeId)
                            || searchValue.Equals(e.FoundTime);
                }
                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    order = orderBy switch
                    {
                        "id_desc" => x => x.OrderByDescending(e => e.Id),
                        "name" => x => x.OrderBy(e => e.Name),
                        "name_desc" => x => x.OrderByDescending(e => e.Name),
                        "description" => x => x.OrderBy(e => e.Description),
                        "description_desc" => x => x.OrderByDescending(e => e.Description),
                        "status" => x => x.OrderBy(e => e.Status),
                        "status_desc" => x => x.OrderByDescending(e => e.Status),
                        "employeeId" => x => x.OrderBy(e => e.EmployeeId),
                        "employeeId_desc" => x => x.OrderByDescending(e => e.EmployeeId),
                        "locationId" => x => x.OrderBy(e => e.LocationId),
                        "locationId_desc" => x => x.OrderByDescending(e => e.LocationId),
                        "foundTime" => x => x.OrderBy(e => e.FoundTime),
                        "foundTime_desc" => x => x.OrderByDescending(e => e.LocationId),
                        _ => x => x.OrderBy(e => e.Id),
                    };
                }

                var lostProperties = await _lostPropertyService.GetAllAsync(pageIndex, pageSize, filter: filter, orderBy: order, include, isDelete);
                return Ok(lostProperties.Result.Select(e => _mapper.Map<LostPropertyDto>(e)));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}", Name = "GetLostPropertyById")]
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

                var lostProperty = await _lostPropertyService.GetByIdAsync(id);
                return lostProperty is null
                    ? NotFound()
                    : Ok(_mapper.Map<LostPropertyDto>(lostProperty));
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
        public async Task<IActionResult> AddAsync([FromBody] CreateLostPropertyDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var entity = _mapper.Map<LostProperty>(dto);

                await _lostPropertyService.AddAsync(entity);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = entity.Id }, dto);
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(ex.Message);
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
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateLostPropertyDto dto)
        {
            try
            {
                if (id < 0
                    || !ModelState.IsValid
                    || id != dto.Id)
                {
                    return BadRequest();
                }

                var inDatabase = await _lostPropertyService.GetByIdAsync(id);
                if (inDatabase is null)
                {
                    return NotFound();
                }

                var entity = _mapper.Map<LostProperty>(dto);
                await _lostPropertyService.UpdateAsync(entity);

                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(ex.Message);
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

                await _lostPropertyService.DeleteAsync(id);
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

                await _lostPropertyService.HardDeleteAsync(id);
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
    }
}
