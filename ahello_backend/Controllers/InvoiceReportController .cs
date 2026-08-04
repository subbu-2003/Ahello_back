using ahello_backend.Models.Invoice_reports;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceReportController : ControllerBase
    {
        private readonly IInvoiceReportService _service;

        public InvoiceReportController(IInvoiceReportService service)
        {
            _service = service;
        }

        [HttpGet("datewise")]
        public async Task<IActionResult> GetDatewise([FromQuery] InvoiceReportRequestModel request)
        {
            var result = await _service.GetDatewiseAsync(request);
            return Ok(result);
        }

        [HttpGet("monthwise")]
        public async Task<IActionResult> GetMonthwise([FromQuery] InvoiceReportRequestModel request)
        {
            var result = await _service.GetMonthwiseAsync(request);
            return Ok(result);
        }

        [HttpGet("yearwise")]
        public async Task<IActionResult> GetYearwise([FromQuery] InvoiceReportRequestModel request)
        {
            var result = await _service.GetYearwiseAsync(request);
            return Ok(result);
        }

        [HttpGet("datewise/export")]
        public async Task<IActionResult> ExportDatewise([FromQuery] InvoiceReportRequestModel request)
        {
            var file = await _service.ExportDatewiseExcelAsync(request);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InvoiceReport_Datewise.xlsx");
        }

        [HttpGet("monthwise/export")]
        public async Task<IActionResult> ExportMonthwise([FromQuery] InvoiceReportRequestModel request)
        {
            var file = await _service.ExportMonthwiseExcelAsync(request);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InvoiceReport_Monthwise.xlsx");
        }

        [HttpGet("yearwise/export")]
        public async Task<IActionResult> ExportYearwise([FromQuery] InvoiceReportRequestModel request)
        {
            var file = await _service.ExportYearwiseExcelAsync(request);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InvoiceReport_Yearwise.xlsx");
        }
    }
}
