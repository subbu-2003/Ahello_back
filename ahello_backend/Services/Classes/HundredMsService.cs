using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ahello_backend.Services.Classes
{
    public class HundredMsService
    {
        private readonly string _appAccessKey;
        private readonly string _appSecret;
        private readonly string _templateId;
        private readonly HttpClient _http;

        public HundredMsService(IConfiguration config, HttpClient http)
        {
            _appAccessKey = config["HundredMs:AppAccessKey"]!;
            _appSecret = config["HundredMs:AppSecret"]!;
            _templateId = config["HundredMs:TemplateId"]!;
            _http = http;
        }

        // ─────────────────────────────────────────
        // 1. Create a room via 100ms API
        //    Returns the room_id string
        // ─────────────────────────────────────────
        public async Task<string> CreateRoomAsync(int bookingId)
        {
            var managementToken = GenerateManagementToken();

            var uniqueId = Guid.NewGuid().ToString("N")[..8];

            var body = JsonSerializer.Serialize(new
            {
                name = $"ahllo-{bookingId}-{uniqueId}",
                description = $"Ahllo booking #{bookingId}",
                template_id = _templateId
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.100ms.live/v2/rooms");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managementToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[100ms] Status: {response.StatusCode}");
            Console.WriteLine($"[100ms] Response: {responseBody}");

            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("id").GetString()!;
        }

        // ─────────────────────────────────────────
        // 2. Generate an auth token for a peer
        //    role = "host" (expert) or "guest" (client)
        // ─────────────────────────────────────────
        public string GenerateAuthToken(string roomId, string role, string userId)
        {
            var now = DateTimeOffset.UtcNow;

            var payload = new JwtPayload
            {
                { "access_key", _appAccessKey },
                { "room_id",    roomId        },
                { "user_id",    userId        },
                { "role",       role          },
                { "type",       "app"         },
                { "version",    2             },
                { "iat",        now.ToUnixTimeSeconds()            },
                { "nbf",        now.ToUnixTimeSeconds()            },
                { "exp",        now.AddHours(6).ToUnixTimeSeconds()},
                { "jti",        Guid.NewGuid().ToString()          }
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(creds);

            var token = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ─────────────────────────────────────────
        // 3. Short-lived management token (for API calls)
        //    Not stored — generated per request
        // ─────────────────────────────────────────
        private string GenerateManagementToken()
        {
            var now = DateTimeOffset.UtcNow;

            var payload = new JwtPayload
            {
                { "access_key", _appAccessKey                      },
                { "type",       "management"                       },
                { "version",    2                                  },
                { "iat",        now.ToUnixTimeSeconds()            },
                { "nbf",        now.ToUnixTimeSeconds()            },
                { "exp",        now.AddMinutes(10).ToUnixTimeSeconds() },
                { "jti",        Guid.NewGuid().ToString()          }
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(creds);

            var token = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}