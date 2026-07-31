using ahello_backend.Models.Service_reports;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceReportController : ControllerBase
    {
        private readonly IServiceReportService _serviceReportService;

        public ServiceReportController(IServiceReportService serviceReportService)
        {
            _serviceReportService = serviceReportService;
        }

        // GET: api/ServiceReport/Datewise?fromDate=...&toDate=...&userId=...&serviceTitle=...&serviceCategoryName=...&search=...&pageNumber=1&pageSize=10
        [HttpGet("Datewise")]
        public async Task<IActionResult> GetDatewiseReport([FromQuery] ServiceReportRequestModel request)
        {
            try
            {
                var result = await _serviceReportService.GetServiceReportDatewiseAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong.", detail = ex.Message });
            }
        }

        // GET: api/ServiceReport/Monthwise?fromDate=...&toDate=...&userId=...&serviceTitle=...&serviceCategoryName=...&search=...&pageNumber=1&pageSize=10
        [HttpGet("Monthwise")]
        public async Task<IActionResult> GetMonthwiseReport([FromQuery] ServiceReportRequestModel request)
        {
            try
            {
                var result = await _serviceReportService.GetServiceReportMonthwiseAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong.", detail = ex.Message });
            }
        }

        // GET: api/ServiceReport/Yearwise?fromDate=...&toDate=...&userId=...&serviceTitle=...&serviceCategoryName=...&search=...&pageNumber=1&pageSize=10
        [HttpGet("Yearwise")]
        public async Task<IActionResult> GetYearwiseReport([FromQuery] ServiceReportRequestModel request)
        {
            try
            {
                var result = await _serviceReportService.GetServiceReportYearwiseAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong.", detail = ex.Message });
            }
        }
        [HttpGet("Datewise/Excel")]
        public async Task<IActionResult> ExportDatewiseExcel([FromQuery] ServiceReportRequestModel request)
        {
            var file = await _serviceReportService.ExportDatewiseExcelAsync(request);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DatewiseReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        [HttpGet("Monthwise/Excel")]
        public async Task<IActionResult> ExportMonthwiseExcel([FromQuery] ServiceReportRequestModel request)
        {
            var file = await _serviceReportService.ExportMonthwiseExcelAsync(request);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"MonthwiseReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        [HttpGet("Yearwise/Excel")]
        public async Task<IActionResult> ExportYearwiseExcel([FromQuery] ServiceReportRequestModel request)
        {
            var file = await _serviceReportService.ExportYearwiseExcelAsync(request);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"YearwiseReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }
}