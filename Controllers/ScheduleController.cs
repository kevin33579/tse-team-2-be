using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleApi.Data;
using ScheduleApi.Models;

namespace ScheduleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // => /api/schedule
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepository _repository;

        public ScheduleController(IScheduleRepository repository)
        {
            _repository = repository;
        }

        // GET: /api/schedule
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetAllAsync()
        {
            var schedules = await _repository.GetAllAsync();
            return Ok(schedules);                       // 200
        }
        [HttpGet("Admin")]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetAllAdminAsync()
        {
            var schedules = await _repository.GetAllAdminAsync();
            return Ok(schedules);                       // 200
        }

        [HttpPut("deactivate/{id:int}")]
        public async Task<IActionResult> DeactivateSchedule(int id)
        {
            var success = await _repository.DeactivateAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }

        [HttpPut("activate/{id:int}")]
        public async Task<IActionResult> ActivateSchedule(int id)
        {
            var success = await _repository.ActivateAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }


        // GET: /api/schedule/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Schedule>> GetByIdAsync(int id)
        {
            var schedule = await _repository.GetByIdAsync(id);
            if (schedule is null) return NotFound();    // 404
            return Ok(schedule);                        // 200
        }

        // POST: /api/schedule
        [HttpPost]
        public async Task<ActionResult<Schedule>> CreateAsync([FromBody] Schedule schedule)
        {
            if (schedule is null || !ModelState.IsValid)
                return BadRequest("Schedule payload is invalid.");

            int newId = await _repository.CreateAsync(schedule);
            schedule.Id = newId;

            // 201 Created + Location header
            return Ok(new { Message = "Schedule created", Id = newId });

        }
        // GET: /api/schedule/user
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetUserScheduleAsync()
        {
            var schedules = await _repository.GetUserScheduleAsync();
            return Ok(schedules); // 200
        }


        // PUT: /api/schedule/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] Schedule schedule)
        {
            if (id != schedule.Id) return BadRequest("Id mismatch");

            var exists = await _repository.GetByIdAsync(id);
            if (exists is null) return NotFound();      // 404

            bool updated = await _repository.UpdateAsync(schedule);
            if (!updated)
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  "Failed to update schedule");

            return NoContent();                         // 204
        }

        // DELETE: /api/schedule/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var exists = await _repository.GetByIdAsync(id);
            if (exists is null) return NotFound();      // 404

            bool deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  "Failed to delete schedule");

            return NoContent();                         // 204
        }
    }
}
