using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos.Employee;
using Project.Core.Entities;
using Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                IEnumerable<Employee> employees = await _employeeService.GetAllAsync();
                return Ok(employees.Select(e => _mapper.Map<EmployeeDto>(e)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}", Name = "GetEmployeeById")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            try
            {
                Employee employee = await _employeeService.GetByIdAsync(id);
                if (employee is null)
                {
                    return NotFound();
                }
                return Ok(_mapper.Map<EmployeeDto>(employee));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] CreateEmployeeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                Employee entity = _mapper.Map<Employee>(dto);

                await _employeeService.AddAsync(entity);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = entity.Id }, dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateEmployeeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                if (!id.Equals(dto.Id))
                {
                    return BadRequest();
                }

                Employee inDatabase = await _employeeService.GetByIdAsync(id);
                if (inDatabase is null)
                {
                    return NotFound();
                }

                await _employeeService.UpdateAsync(_mapper.Map<Employee>(dto));

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                await _employeeService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}/Delete")]
        public async Task<IActionResult> HardDeleteAsync(string id)
        {
            try
            {
                await _employeeService.HardDeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
