using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RotoMonster.Core;
using RotoMonster.Data;

namespace RotoMonster.Api
{
    [Route("api/admintasks")]
    [ApiController]
    public class AdminTasksController : ControllerBase
    {
        private readonly AdminTaskService _tasks;
        private readonly IConfiguration _config;

        public AdminTasksController(AdminTaskService tasks, IConfiguration config)
        {
            _tasks = tasks;
            _config = config;
        }

        public class AddTaskRequest
        {
            public string Text { get; set; }
            public string Url { get; set; }
            public string UrlText { get; set; }
            public bool IsAdminOnly { get; set; }
            public string Source { get; set; }
            public string DedupeKey { get; set; }
            public string ActionType { get; set; }
            public string ActionLabel { get; set; }
            public JsonElement? ActionData { get; set; }
        }

        public class DoneRequest
        {
            public string DedupeKey { get; set; }
            public string DoneBy { get; set; }
        }

        [HttpPost]
        public IActionResult Add([FromBody] AddTaskRequest request)
        {
            if (!IsAuthorized()) return Unauthorized();
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { error = "text is required" });

            var url = Trim(request.Url, 500);
            if (url != null && !(url.StartsWith("https://") || url.StartsWith("http://") || url.StartsWith("/")))
                return BadRequest(new { error = "url must start with http://, https:// or /" });

            var task = _tasks.Add(new AdminTask
            {
                Text = Trim(request.Text, 1000),
                Url = url,
                UrlText = Trim(request.UrlText, 50),
                IsAdminOnly = request.IsAdminOnly,
                Source = Trim(request.Source, 50),
                DedupeKey = Trim(request.DedupeKey, 200),
                ActionType = Trim(request.ActionType, 50),
                ActionLabel = Trim(request.ActionLabel, 30),
                ActionJson = request.ActionData.HasValue && request.ActionData.Value.ValueKind != JsonValueKind.Null
                    ? request.ActionData.Value.GetRawText()
                    : null
            });

            return Ok(new { id = task.Id, done = task.DoneAt != null });
        }

        [HttpPost("done")]
        public IActionResult Done([FromBody] DoneRequest request)
        {
            if (!IsAuthorized()) return Unauthorized();
            if (request == null || string.IsNullOrWhiteSpace(request.DedupeKey))
                return BadRequest(new { error = "dedupeKey is required" });

            var count = _tasks.MarkDoneByKey(request.DedupeKey, string.IsNullOrWhiteSpace(request.DoneBy) ? "Monitor" : Trim(request.DoneBy, 50));
            return Ok(new { done = count });
        }

        private bool IsAuthorized()
        {
            var apiKey = _config["AdminTasks:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) apiKey = _config["MonitorApiKey"];
            return !string.IsNullOrEmpty(apiKey) && Request.Headers["X-API-Key"] == apiKey;
        }

        private static string Trim(string value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();
            return value.Length > max ? value.Substring(0, max) : value;
        }
    }
}
