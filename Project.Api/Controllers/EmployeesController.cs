using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos.Employee;
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
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;

        public EmployeesController(IEmployeeService employeeService, IMapper mapper)
        {
            _employeeService = employeeService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] int pageIndex = 1, int pageSize = 10, string searchValue = null, string orderBy = null, bool isDelete = false)
        {
            try
            {
                Expression<Func<Employee, bool>> filter = null;
                Func<IQueryable<Employee>, IOrderedQueryable<Employee>> order = null;
                string include = string.Empty;

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    searchValue = searchValue.ToUpper();
                    filter = e => e.Id.Contains(searchValue)
                            || e.FirstName.ToUpper().Contains(searchValue)
                            || e.LastName.ToUpper().Contains(searchValue)
                            || e.Phone.ToUpper().Contains(searchValue)
                            || e.Email.ToUpper().Contains(searchValue);
                }

                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    order = orderBy switch
                    {
                        "id_desc" => x => x.OrderByDescending(e => e.Id),
                        "firstName" => x => x.OrderBy(e => e.FirstName),
                        "firstName_desc" => x => x.OrderByDescending(e => e.FirstName),
                        "lastName" => x => x.OrderBy(e => e.LastName),
                        "lastName_desc" => x => x.OrderByDescending(e => e.LastName),
                        "phone" => x => x.OrderBy(e => e.Phone),
                        "phone_desc" => x => x.OrderByDescending(e => e.Phone),
                        "dob" => x => x.OrderBy(e => e.DateOfBirth),
                        "dob_desc" => x => x.OrderByDescending(e => e.DateOfBirth),
                        _ => x => x.OrderBy(e => e.Id),
                    };
                }

                var employees = await _employeeService.GetAsync(pageIndex, pageSize, filter: filter, orderBy: order, include, isDelete);
                employees.Result.Select(e => _mapper.Map<EmployeeDto>(e));
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}", Name = "GetEmployeeById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)
                    || !Guid.TryParse(id, out _))
                {
                    return BadRequest();
                }

                var employee = await _employeeService.GetByIdAsync(id);
                return employee is null
                    ? NotFound()
                    : Ok(_mapper.Map<EmployeeDto>(employee));
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
        public async Task<IActionResult> AddAsync([FromBody] CreateEmployeeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var entity = _mapper.Map<Employee>(dto);

                await _employeeService.AddAsync(entity);
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
        public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateEmployeeDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)
                    || !Guid.TryParse(id, out _)
                    || !ModelState.IsValid
                    || !(id == dto.Id))
                {
                    return BadRequest();
                }

                var inDatabase = await _employeeService.GetByIdAsync(id);
                if (inDatabase is null)
                {
                    return NotFound();
                }

                await _employeeService.UpdateAsync(_mapper.Map<Employee>(dto));

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
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)
                    || !Guid.TryParse(id, out _))
                {
                    return BadRequest();
                }

                await _employeeService.DeleteAsync(id);
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
        public async Task<IActionResult> HardDeleteAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)
                    || !Guid.TryParse(id, out _))
                {
                    return BadRequest();
                }

                await _employeeService.HardDeleteAsync(id);
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
