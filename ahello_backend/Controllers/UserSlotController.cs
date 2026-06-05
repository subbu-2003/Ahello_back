using ahello_backend.Models.UserSlots;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSlotController : ControllerBase
    {
        private readonly IUserSlotRepository _repo;
        private readonly IServiceRepository _serviceRepository;
        private readonly ISlotService _slotService;

        public UserSlotController(
            IUserSlotRepository repo,
            IServiceRepository serviceRepository,
            ISlotService slotService)
        {
            _repo = repo;
            _serviceRepository = serviceRepository;
            _slotService = slotService;
        }

        // GET: api/UserSlot
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var slots = await _repo.GetAll();

                return Ok(slots);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        // GET: api/UserSlot/user/1/service/5
        [HttpGet("user/{userId}/service/{serviceId}")]
        public async Task<IActionResult> GetByUserAndService(
            int userId,
            int serviceId)
        {
            try
            {
                var slots = await _repo.GetByUserAndService(
                    userId,
                    serviceId);

                return Ok(slots);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        // GET: api/UserSlot/week
        [HttpGet("week")]
        public async Task<IActionResult> GetWeekSlots(
            [FromQuery] int userId,
            [FromQuery] int serviceId,
            [FromQuery] DateTime weekStart)
        {
            try
            {
                var service =
                    await _serviceRepository.GetById(serviceId);

                if (service == null)
                {
                    return NotFound(new
                    {
                        Message = "Service not found"
                    });
                }

                var durationRaw =
                    System.Text.RegularExpressions.Regex.Match(
                        service.Duration ?? "",
                        @"\d+").Value;

                if (!int.TryParse(
                        durationRaw,
                        out int durationMinutes) ||
                    durationMinutes <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "Invalid service duration"
                    });
                }

                var result =
                    await _slotService.GetWeekSlots(
                        userId,
                        serviceId,
                        durationMinutes,
                        weekStart.Date);

                return Ok(result);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        // GET: api/UserSlot/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var slot = await _repo.GetById(id);

                if (slot == null)
                {
                    return NotFound(new
                    {
                        Message = "Slot not found"
                    });
                }

                return Ok(slot);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        // POST: api/UserSlot
        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] UserSlotPost model)
        {
            try
            {
                if (model.StartTime >= model.EndTime)
                {
                    return BadRequest(new
                    {
                        Message = "StartTime must be before EndTime"
                    });
                }

                var result = await _repo.Post(model);

                return result > 0
                    ? Ok(new
                    {
                        Message = "Slot created successfully"
                    })
                    : BadRequest(new
                    {
                        Message = "Failed to create slot"
                    });
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Invalid data"
                });
            }
        }

        // POST: api/UserSlot/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> PostBulk(
            [FromBody] List<UserSlotPost> slots)
        {
            try
            {
                if (slots == null || !slots.Any())
                {
                    return BadRequest(new
                    {
                        Message = "No slots provided"
                    });
                }

                if (slots.Any(s => s.StartTime >= s.EndTime))
                {
                    return BadRequest(new
                    {
                        Message = "All slots must have StartTime before EndTime"
                    });
                }

                var result = await _repo.PostBulk(slots);

                return result > 0
                    ? Ok(new
                    {
                        Message = $"{result} slot(s) created"
                    })
                    : BadRequest(new
                    {
                        Message = "Failed to create slots"
                    });
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Invalid data"
                });
            }
        }

        // PUT: api/UserSlot/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(
            int id,
            [FromBody] UserSlotPut model)
        {
            try
            {
                if (id != model.SlotId)
                {
                    return BadRequest(new
                    {
                        Message = "Route id and SlotId do not match"
                    });
                }

                if (model.StartTime >= model.EndTime)
                {
                    return BadRequest(new
                    {
                        Message = "StartTime must be before EndTime"
                    });
                }

                var result = await _repo.Put(model);

                return result > 0
                    ? Ok(new
                    {
                        Message = "Slot updated successfully"
                    })
                    : BadRequest(new
                    {
                        Message = "Slot not found or already booked"
                    });
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Invalid data"
                });
            }
        }

        // DELETE: api/UserSlot/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _repo.Delete(id);

                return result > 0
                    ? Ok(new
                    {
                        Message = "Slot deleted successfully"
                    })
                    : BadRequest(new
                    {
                        Message = "Slot not found or already booked"
                    });
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }
    }
}