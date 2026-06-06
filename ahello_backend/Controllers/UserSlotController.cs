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

        // ── GET: api/UserSlot ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repo.GetAll());
        }

        // ── GET: api/UserSlot/user/1/service/5 ───────────────────────
        [HttpGet("user/{userId}/service/{serviceId}")]
        public async Task<IActionResult> GetByUserAndService(int userId, int serviceId)
        {
            return Ok(await _repo.GetByUserAndService(userId, serviceId));
        }

        // ── GET: api/UserSlot/week?userId=1&serviceId=5&weekStart=2026-06-01
        //    Existing endpoint — kept as-is
        [HttpGet("week")]
        public async Task<IActionResult> GetWeekSlots(
            [FromQuery] int userId,
            [FromQuery] int serviceId,
            [FromQuery] DateTime weekStart)
        {
            var service = await _serviceRepository.GetById(serviceId);
            if (service == null) return NotFound("Service not found.");

            var durationRaw = System.Text.RegularExpressions.Regex
                .Match(service.Duration ?? "", @"\d+").Value;

            if (!int.TryParse(durationRaw, out int durationMinutes) || durationMinutes <= 0)
                return BadRequest($"Invalid duration: '{service.Duration}'");

            var result = await _slotService.GetWeekSlots(
                userId, serviceId, durationMinutes, weekStart.Date);

            return Ok(result);
        }

        // ── GET: api/UserSlot/available-dates?userId=1&serviceId=5&year=2026&month=6
        //    PHASE 1 — returns only date strings with >= 1 free slot
        //    Call this on calendar month load to grey out unavailable dates (fast)
        [HttpGet("available-dates")]
        public async Task<IActionResult> GetAvailableDates(
            [FromQuery] int userId,
            [FromQuery] int serviceId,
            [FromQuery] int year,
            [FromQuery] int month)
        {
            if (year <= 0) return BadRequest("Invalid year.");
            if (month is < 1 or > 12) return BadRequest("Month must be 1–12.");

            var service = await _serviceRepository.GetById(serviceId);
            if (service == null) return NotFound("Service not found.");

            var durationRaw = System.Text.RegularExpressions.Regex
                .Match(service.Duration ?? "", @"\d+").Value;

            if (!int.TryParse(durationRaw, out int durationMinutes) || durationMinutes <= 0)
                return BadRequest($"Invalid duration: '{service.Duration}'");

            var dates = await _slotService.GetAvailableDates(
                userId, serviceId, durationMinutes, year, month);

            return Ok(dates); // ["2026-06-30", "2026-06-14", ...]
        }

        // ── GET: api/UserSlot/slots?userId=1&serviceId=5&date=2026-06-30
        //    PHASE 2 — returns all time slots for a single date
        //    Call this only when user clicks a date on the calendar (fast)
        [HttpGet("slots")]
        public async Task<IActionResult> GetDaySlots(
            [FromQuery] int userId,
            [FromQuery] int serviceId,
            [FromQuery] DateTime date)
        {
            if (date == default) return BadRequest("Invalid date.");

            var service = await _serviceRepository.GetById(serviceId);
            if (service == null) return NotFound("Service not found.");

            var durationRaw = System.Text.RegularExpressions.Regex
                .Match(service.Duration ?? "", @"\d+").Value;

            if (!int.TryParse(durationRaw, out int durationMinutes) || durationMinutes <= 0)
                return BadRequest($"Invalid duration: '{service.Duration}'");

            var slots = await _slotService.GetDaySlots(
                userId, serviceId, durationMinutes, date.Date);

            return Ok(slots);
        }

        // ── GET: api/UserSlot/5 ───────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var slot = await _repo.GetById(id);
            return slot == null ? NotFound() : Ok(slot);
        }

        // ── POST: api/UserSlot ────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserSlotPost model)
        {
            if (model.StartTime >= model.EndTime)
                return BadRequest("StartTime must be before EndTime.");

            var result = await _repo.Post(model);
            return result > 0 ? Ok("Slot created.") : StatusCode(500, "Failed to create slot.");
        }

        // ── POST: api/UserSlot/bulk ───────────────────────────────────
        [HttpPost("bulk")]
        public async Task<IActionResult> PostBulk([FromBody] List<UserSlotPost> slots)
        {
            if (slots == null || !slots.Any())
                return BadRequest("No slots provided.");

            if (slots.Any(s => s.StartTime >= s.EndTime))
                return BadRequest("All slots must have StartTime before EndTime.");

            var result = await _repo.PostBulk(slots);
            return result > 0 ? Ok($"{result} slot(s) created.") : StatusCode(500, "Failed.");
        }

        // ── PUT: api/UserSlot/5 ───────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserSlotPut model)
        {
            if (id != model.SlotId)
                return BadRequest("Route id and SlotId do not match.");

            if (model.StartTime >= model.EndTime)
                return BadRequest("StartTime must be before EndTime.");

            var result = await _repo.Put(model);
            return result > 0
                ? Ok("Slot updated.")
                : BadRequest("Slot not found or already booked.");
        }

        // ── DELETE: api/UserSlot/5 ────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.Delete(id);
            return result > 0
                ? Ok("Slot deleted.")
                : BadRequest("Slot not found or already booked.");
        }
    }
}