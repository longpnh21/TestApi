﻿using AutoMapper;
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
                    switch (orderBy)
                    {
                        case "id_desc":
                            order = x => x.OrderByDescending(e => e.Id);
                            break;
                        case "name":
                            order = x => x.OrderBy(e => e.Name);
                            break;
                        case "name_desc":
                            order = x => x.OrderByDescending(e => e.Name);
                            break;
                        case "description":
                            order = x => x.OrderBy(e => e.Description);
                            break;
                        case "description_desc":
                            order = x => x.OrderByDescending(e => e.Description);
                            break;
                        case "status":
                            order = x => x.OrderBy(e => e.Status);
                            break;
                        case "status_desc":
                            order = x => x.OrderByDescending(e => e.Status);
                            break;
                        case "employeeId":
                            order = x => x.OrderBy(e => e.EmployeeId);
                            break;
                        case "employeeId_desc":
                            order = x => x.OrderByDescending(e => e.EmployeeId);
                            break;
                        case "locationId":
                            order = x => x.OrderBy(e => e.LocationId);
                            break;
                        case "locationId_desc":
                            order = x => x.OrderByDescending(e => e.LocationId);
                            break;
                        case "foundTime":
                            order = x => x.OrderBy(e => e.FoundTime);
                            break;
                        case "foundTime_desc":
                            order = x => x.OrderByDescending(e => e.LocationId);
                            break;
                        default:
                            order = x => x.OrderBy(e => e.Id);
                            break;
                    }
                }

                var lostProperties = await _lostPropertyService.GetAllAsync(pageIndex, pageSize, filter: filter, orderBy: order, include, isDelete);
                return Ok(lostProperties.Select(e => _mapper.Map<LostPropertyDto>(e)));
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
