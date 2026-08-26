using ahello_backend.Models.MeetingParticipant;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingRecordingController : ControllerBase
    {
        private readonly IMeetingRecordingService _service;

        public MeetingRecordingController(
            IMeetingRecordingService service)
        {
            _service = service;
        }


        // =========================================================
        // START RECORDING
        // POST: /api/MeetingRecording/start
        // =========================================================

        [HttpPost("start")]
        public async Task<IActionResult> Start(
            [FromBody] MeetingRecording model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Recording request is required."
                    });
                }

                if (model.MeetingId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid MeetingId."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.RoomId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "RoomId is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.RoomName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "RoomName is required."
                    });
                }

                var recordingId =
                    await _service.StartAsync(model);

                return Ok(new
                {
                    success = true,
                    recordingId = recordingId,
                    meetingId = model.MeetingId,
                    message = "Recording started successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[MeetingRecording Start Error] {ex}");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // =========================================================
        // STOP RECORDING
        // PUT: /api/MeetingRecording/stop/{meetingId}
        // =========================================================

        [HttpPut("stop/{meetingId}")]
        public async Task<IActionResult> Stop(
            int meetingId)
        {
            try
            {
                if (meetingId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid MeetingId."
                    });
                }

                var result =
                    await _service.StopAsync(meetingId);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Active recording not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    meetingId = meetingId,
                    message =
                        "Recording stop request sent successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[MeetingRecording Stop Error] {ex}");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // =========================================================
        // GET RECORDING BY MEETING
        // GET: /api/MeetingRecording/{meetingId}
        // =========================================================

        [HttpGet("{meetingId}")]
        public async Task<IActionResult> GetByMeetingId(
            int meetingId)
        {
            try
            {
                if (meetingId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid MeetingId."
                    });
                }

                var result =
                    await _service.GetByMeetingIdAsync(
                        meetingId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Recording not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    recording = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[MeetingRecording Get Error] {ex}");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // =========================================================
        // GET ALL RECORDINGS
        // GET: /api/MeetingRecording/all
        // =========================================================

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result =
                    await _service.GetAllAsync();

                return Ok(new
                {
                    success = true,
                    recordings = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[MeetingRecording GetAll Error] {ex}");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // =========================================================
        // 100MS WEBHOOK
        // POST: /api/MeetingRecording/webhook
        // =========================================================

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook(
    [FromBody] JsonElement payload)
        {
            try
            {
                Console.WriteLine("=================================================");
                Console.WriteLine("[100ms Webhook] Received");
                Console.WriteLine(payload.ToString());
                Console.WriteLine("=================================================");

                // -------------------------------------------------
                // EVENT TYPE
                // -------------------------------------------------

                string? eventType = null;

                if (payload.TryGetProperty("type", out var typeProperty) &&
                    typeProperty.ValueKind == JsonValueKind.String)
                {
                    eventType = typeProperty.GetString();
                }

                Console.WriteLine($"[100ms Webhook] Event Type: {eventType}");

                // -------------------------------------------------
                // DATA
                // -------------------------------------------------

                JsonElement data;

                if (payload.TryGetProperty("data", out var dataProperty))
                {
                    data = dataProperty;
                }
                else
                {
                    data = payload;
                }

                // -------------------------------------------------
                // ROOM ID
                // -------------------------------------------------

                string? roomId = null;

                if (data.TryGetProperty("room_id", out var roomIdProperty) &&
                    roomIdProperty.ValueKind == JsonValueKind.String)
                {
                    roomId = roomIdProperty.GetString();
                }

                Console.WriteLine($"[100ms Webhook] Room ID: {roomId}");

                // -------------------------------------------------
                // RECORDING ID
                // -------------------------------------------------

                string? recordingId = null;

                if (data.TryGetProperty("recording_id", out var recordingIdProperty) &&
                    recordingIdProperty.ValueKind == JsonValueKind.String)
                {
                    recordingId = recordingIdProperty.GetString();
                }

                if (string.IsNullOrWhiteSpace(recordingId))
                {
                    if (data.TryGetProperty("id", out var idProperty) &&
                        idProperty.ValueKind == JsonValueKind.String)
                    {
                        recordingId = idProperty.GetString();
                    }
                }

                Console.WriteLine(
                    $"[100ms Webhook] Recording ID: {recordingId}");

                // -------------------------------------------------
                // RECORDING URL
                // -------------------------------------------------

                string? recordingUrl = null;

                if (data.TryGetProperty(
                        "recording_presigned_url",
                        out var urlProperty) &&
                    urlProperty.ValueKind == JsonValueKind.String)
                {
                    recordingUrl = urlProperty.GetString();
                }

                // -------------------------------------------------
                // RECORDING ASSET ID
                // -------------------------------------------------

                string? recordingAssetId = null;

                if (data.TryGetProperty(
                        "asset_id",
                        out var assetIdProperty) &&
                    assetIdProperty.ValueKind == JsonValueKind.String)
                {
                    recordingAssetId = assetIdProperty.GetString();
                }

                Console.WriteLine(
                    $"[100ms Webhook] Recording Asset ID: {recordingAssetId}");

                // -------------------------------------------------
                // RECORDING PATH
                // -------------------------------------------------

                string? recordingPath = null;

                if (data.TryGetProperty(
                        "recording_path",
                        out var pathProperty) &&
                    pathProperty.ValueKind == JsonValueKind.String)
                {
                    recordingPath = pathProperty.GetString();
                }

                // If file name is not directly available,
                // use the recording path as fallback.
                string? fileName = null;

                if (data.TryGetProperty(
                        "file_name",
                        out var fileNameProperty) &&
                    fileNameProperty.ValueKind == JsonValueKind.String)
                {
                    fileName = fileNameProperty.GetString();
                }

                if (string.IsNullOrWhiteSpace(fileName) &&
                    !string.IsNullOrWhiteSpace(recordingPath))
                {
                    fileName = Path.GetFileName(recordingPath);
                }

                // -------------------------------------------------
                // DURATION
                // -------------------------------------------------

                int? durationSeconds = null;

                if (data.TryGetProperty(
                        "duration",
                        out var durationProperty) &&
                    durationProperty.ValueKind == JsonValueKind.Number)
                {
                    if (durationProperty.TryGetInt32(out var duration))
                    {
                        durationSeconds = duration;
                    }
                }

                // -------------------------------------------------
                // END TIME
                // -------------------------------------------------

                DateTime? endedAt = null;

                if (data.TryGetProperty(
                        "session_stopped_at",
                        out var sessionStoppedProperty) &&
                    sessionStoppedProperty.ValueKind == JsonValueKind.String)
                {
                    if (DateTime.TryParse(
                            sessionStoppedProperty.GetString(),
                            out var parsedDate))
                    {
                        endedAt = parsedDate;
                    }
                }

                if (endedAt == null &&
                    data.TryGetProperty(
                        "stopped_at",
                        out var stoppedAtProperty) &&
                    stoppedAtProperty.ValueKind == JsonValueKind.String)
                {
                    if (DateTime.TryParse(
                            stoppedAtProperty.GetString(),
                            out var parsedDate))
                    {
                        endedAt = parsedDate;
                    }
                }

                // -------------------------------------------------
                // STATUS
                // -------------------------------------------------

                string status;

                if (string.Equals(
                        eventType,
                        "recording.success",
                        StringComparison.OrdinalIgnoreCase))
                {
                    status = "Completed";
                }
                else if (string.Equals(
                             eventType,
                             "recording.failed",
                             StringComparison.OrdinalIgnoreCase))
                {
                    status = "Failed";
                }
                else
                {
                    status = "Processing";

                    if (data.TryGetProperty(
                            "status",
                            out var statusProperty) &&
                        statusProperty.ValueKind == JsonValueKind.String)
                    {
                        status =
                            statusProperty.GetString()
                            ?? "Processing";
                    }
                }

                Console.WriteLine($"[100ms Webhook] Status: {status}");
                Console.WriteLine(
                    $"[100ms Webhook] Recording URL: {recordingUrl}");
                Console.WriteLine(
                    $"[100ms Webhook] Duration: {durationSeconds}");
                Console.WriteLine(
                    $"[100ms Webhook] Ended At: {endedAt}");

                // -------------------------------------------------
                // ROOM ID IS REQUIRED
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(roomId))
                {
                    Console.WriteLine(
                        "[100ms Webhook] Room ID not found.");

                    return Ok(new
                    {
                        success = true,
                        message =
                            "Webhook received but room_id was not present.",
                        eventType
                    });
                }

                // -------------------------------------------------
                // FIND OUR RECORDING USING ROOM ID
                // -------------------------------------------------

                var recording =
                    await _service.GetByRoomIdAsync(roomId);

                if (recording == null)
                {
                    Console.WriteLine(
                        $"[100ms Webhook] No recording found for RoomId: {roomId}");

                    return Ok(new
                    {
                        success = true,
                        message =
                            "Webhook received but matching recording was not found.",
                        roomId
                    });
                }

                // -------------------------------------------------
                // USE OUR DATABASE HMS RECORDING ID
                // -------------------------------------------------

                var hmsRecordingId =
                    recording.HMSRecordingId;

                if (string.IsNullOrWhiteSpace(hmsRecordingId) &&
                    !string.IsNullOrWhiteSpace(recordingId))
                {
                    hmsRecordingId = recordingId;
                }

                if (string.IsNullOrWhiteSpace(hmsRecordingId))
                {
                    return Ok(new
                    {
                        success = true,
                        message =
                            "Webhook received but HMS recording ID was not available.",
                        roomId
                    });
                }

                // -------------------------------------------------
                // PROCESS WEBHOOK
                // -------------------------------------------------

                var result =
                    await _service.ProcessWebhookAsync(
                        hmsRecordingId,
                        recordingAssetId,
                        recordingUrl,
                        fileName,
                        endedAt,
                        durationSeconds,
                        status);

                Console.WriteLine(
                    $"[100ms Webhook] Process result: {result}");

                return Ok(new
                {
                    success = true,
                    message = "Webhook processed successfully.",
                    roomId,
                    recordingId = hmsRecordingId,
                    status
                });
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"[100ms Webhook JSON Error] {ex}");

                return BadRequest(new
                {
                    success = false,
                    message = "Invalid webhook JSON."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[100ms Webhook Error] {ex}");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // =========================================================
        // PLAY RECORDING
        // GET: /api/MeetingRecording/{recordingId}/play
        // =========================================================

        [HttpGet("{recordingId}/play")]
        public async Task<IActionResult> Play(
            int recordingId)
        {
            try
            {
                if (recordingId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid RecordingId."
                    });
                }

                var url =
                    await _service.GetPlayUrlAsync(
                        recordingId);

                if (string.IsNullOrWhiteSpace(url))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Recording not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    recordingId = recordingId,
                    recordingUrl = url
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[MeetingRecording Play Error] {ex}");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}