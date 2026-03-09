namespace CavRn.ScreenPlayers
{
    using Eco.Core.Plugins.Interfaces;
    using Eco.Core.Plugins;
    using Eco.Core.Utils;
    using Eco.Gameplay.Players;
    using Eco.Shared.Localization;
    using Eco.Shared.Logging;
    using Eco.Shared.Utils;
    using Eco.WebServer.Web.Authentication;
    using Eco.WebServer.Web.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System;

    public class ScreenPlayersConfig
    {
        public bool EnableWebUploader { get; set; } = true;
        public bool RequireValidation { get; set; } = false;
        public bool AllowOnlyLocalUrl { get; set; } = false;
        public int MaxUploadPerUser { get; set; } = 5;
        public int MaxFileSizeInMB { get; set; } = 15;
    }

    public class ScreenPlayersPlugin : Singleton<ScreenPlayersPlugin>, IWebPlugin, IModKitPlugin, IInitializablePlugin, IShutdownablePlugin, IConfigurablePlugin
    {
        static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        const string DataFileName = "files.json";

        public static List<VideoAudioFile> Files = [];

        public static ThreadSafeAction OnSettingsChanged = new();
        public IPluginConfig PluginConfig => this.config;
        private readonly PluginConfig<ScreenPlayersConfig> config;
        public ScreenPlayersConfig Config => this.config.Config;
        public ThreadSafeAction<object, string> ParamChanged { get; set; } = new();

        public ScreenPlayersPlugin()
        {
            this.config = new PluginConfig<ScreenPlayersConfig>("ScreenPlayers");
            ScreenPlayersRegistry.Obj = new ScreenPlayersService();
            this.SaveConfig();
        }

        public LocString GetMenuTitle()
        {
            return new LocString("Audio Video Uploader");
        }

        public string? GetPluginIndexUrl()
        {
            return "ScreenPlayersPlugin/index.html";
        }

        public string? GetFontAwesomeIcon()
        {
            return "fa-solid fa-cloud-arrow-up";
        }

        public string? GetStaticFilesPath()
        {
            return null;
        }

        public string? GetEmbeddedResourceNamespace()
        {
            return this.Config.EnableWebUploader ? "ScreenPlayers.Assets" : null;
        }

        public string GetStatus()
        {
            return "OK";
        }

        public string GetCategory()
        {
            return "Mods";
        }

        public Task ShutdownAsync()
        {
            SaveFiles();

            return Task.CompletedTask;
        }

        public void Initialize(TimedTask timer)
        {
            LoadFiles();
        }

        public object GetEditObject() => this.config.Config;
        public void OnEditObjectChanged(object o, string param) { this.SaveConfig(); }

        static string DataFilePath => Path.Combine(Directory.GetCurrentDirectory(), "Storage", "ScreenPlayers", DataFileName);
        internal static string ValidatedDir => Path.Combine(Directory.GetCurrentDirectory(), "WebClient", "WebBin", "ScreenPlayers");
        internal static string NonValidatedDir => Path.Combine(Directory.GetCurrentDirectory(), "Storage", "ScreenPlayers", "NonValidated");

        internal static string GetStoredPath(Guid id, bool validated) => Path.Combine(validated ? ValidatedDir : NonValidatedDir, $"{id}.mp4");

        internal static string? GetWebServerBaseUrl()
        {
            //Priority 1: Use WebServerUrl if configured (this is what clients use)
            var webUrl = Eco.Plugins.Networking.NetworkManager.Config.WebServerUrl;
            if (!string.IsNullOrWhiteSpace(webUrl))
                return webUrl.Trim().TrimEnd('/');

            //Priority 2: Build URL from RemoteAddress and WebServerPort
            var port = Eco.Plugins.Networking.NetworkManager.Config.WebServerPort;

            var remoteAddress = Eco.Plugins.Networking.NetworkManager.Config.RemoteAddress;
            if (!string.IsNullOrWhiteSpace(remoteAddress) && remoteAddress != "*" && remoteAddress != "0.0.0.0" && remoteAddress != "Any")
            {
                //If it already has a protocol, use it as-is
                if (remoteAddress.Contains("://", StringComparison.Ordinal))
                    return remoteAddress.Trim().TrimEnd('/');

                //Otherwise build http://address:port
                return $"http://{remoteAddress}:{port}";
            }

            //Priority 3: Build URL from IPAddress and WebServerPort
            var ipAddress = Eco.Plugins.Networking.NetworkManager.Config.IPAddress;
            if (!string.IsNullOrWhiteSpace(ipAddress) && ipAddress != "*" && ipAddress != "0.0.0.0" && ipAddress != "Any")
            {
                return $"http://{ipAddress}:{port}";
            }

            //Priority 4: Use localhost as fallback
            return $"http://localhost:{port}";
        }

        internal static string? GetUploaderUrl()
        {
            if (!Obj.Config.EnableWebUploader)
                return null;

            return $"{GetWebServerBaseUrl()}/plugin/ScreenPlayersPlugin";
        }

        internal static string GetPublicUrl(Guid id)
        {
            return $"{GetWebServerBaseUrl()}/ScreenPlayers/{id}.mp4";
        }

        internal static VideoAudioFileDto ToDto(VideoAudioFile file) =>
            new()
            {
                Id = file.Id.ToString(),
                CreatorName = file.CreatorName,
                Name = file.Name,
                UploadedAt = new DateTimeOffset(file.UploadedAt).ToUnixTimeMilliseconds(),
                Validated = file.Validated,
                Url = file.Validated ? GetPublicUrl(file.Id) : null
            };

        internal static VideoAudioFile FromDto(VideoAudioFileDto dto)
        {
            var id = Guid.TryParse(dto.Id, out var parsed) ? parsed : Guid.NewGuid();
            var uploadedAt = DateTimeOffset.FromUnixTimeMilliseconds(dto.UploadedAt).UtcDateTime;
            return new VideoAudioFile(dto.CreatorName, dto.Name, uploadedAt, dto.Validated)
            {
                Id = id,
                Url = dto.Validated ? GetPublicUrl(id) : null
            };
        }

        static void SaveFiles()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DataFilePath)!);

            Log.WriteLineLoc($"Saving {Files.Count} files to {DataFilePath}");

            var json = JsonSerializer.Serialize(Files.Select(ToDto).ToList(), JsonOptions);
            File.WriteAllText(DataFilePath, json);
        }

        static void LoadFiles()
        {
            if (!File.Exists(DataFilePath))
                return;

            try
            {
                var json = File.ReadAllText(DataFilePath);
                var dtoRecords = JsonSerializer.Deserialize<List<VideoAudioFileDto>>(json, JsonOptions);
                if (dtoRecords != null)
                {
                    Files = dtoRecords.Select(FromDto).ToList();
                    return;
                }

                var legacyRecords = JsonSerializer.Deserialize<List<VideoAudioFile>>(json, JsonOptions) ?? new List<VideoAudioFile>();
                foreach (var file in legacyRecords)
                {
                    if (file.Validated)
                        file.Url ??= GetPublicUrl(file.Id);
                    else
                        file.Url = null;
                }

                Files = legacyRecords;
            }
            catch
            {
                Files = new List<VideoAudioFile>();
            }
        }
    }

    public class VideoAudioFile(string creatorName, string name, DateTime uploadedAt, bool validated)
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CreatorName { get; set; } = creatorName;
        public string Name { get; set; } = name;
        public DateTime UploadedAt { get; set; } = uploadedAt;
        public bool Validated { get; set; } = validated;
        public string? Url { get; set; }
    }

    public class VideoAudioFileDto
    {
        public string Id { get; set; } = "";
        public string CreatorName { get; set; } = "";
        public string Name { get; set; } = "";
        public long UploadedAt { get; set; }
        public bool Validated { get; set; }
        public string? Url { get; set; }
    }

    public sealed class ScreenPlayersService : IScreenPlayersService
    {
        public bool EnableWebUploader => ScreenPlayersPlugin.Obj.Config.EnableWebUploader;
        public bool AllowOnlyLocalUrl => ScreenPlayersPlugin.Obj.Config.AllowOnlyLocalUrl;

        public string? GetWebServerBaseUrl() => ScreenPlayersPlugin.GetWebServerBaseUrl();
        public string GetPublicUrl(Guid id) => ScreenPlayersPlugin.GetPublicUrl(id);
    }

    [Route("api/v1/plugins/screenplayers")]
    [Controller]
    [Authorize(Policy = PolicyNames.RequiresEcoUser)]
    public class ScreenPlayersController : Controller
    {
        private User GetUserFromContext() => (this.HttpContext.User.Identity as EcoUserIdentity)!.User;

        [HttpGet("config")]
        public ActionResult GetConfig()
        {
            return this.Ok(new { maxFileSizeInMB = ScreenPlayersPlugin.Obj.Config.MaxFileSizeInMB });
        }

        [HttpGet("allFiles")]
        [Authorize(Policy = PolicyNames.RequireAdmin)]
        public ActionResult<List<VideoAudioFileDto>> GetAllFiles()
        {
            return ScreenPlayersPlugin.Files.Select(ScreenPlayersPlugin.ToDto).ToList();
        }

        [HttpGet("DeleteFile")]
        public ActionResult<List<VideoAudioFileDto>> DeleteFile(Guid fileId)
        {
            var user = this.GetUserFromContext();
            var file = ScreenPlayersPlugin.Files.FirstOrDefault(f => f.Id == fileId);
            if (file == null)
                return this.NotFound();

            if (!user.IsAdmin && user.Name != file.CreatorName) return this.Unauthorized();

            ScreenPlayersPlugin.Files.Remove(file);
            var validatedPath = ScreenPlayersPlugin.GetStoredPath(file.Id, validated: true);
            var nonValidatedPath = ScreenPlayersPlugin.GetStoredPath(file.Id, validated: false);
            if (System.IO.File.Exists(validatedPath))
                System.IO.File.Delete(validatedPath);
            if (System.IO.File.Exists(nonValidatedPath))
                System.IO.File.Delete(nonValidatedPath);

            return ScreenPlayersPlugin.Files.Select(ScreenPlayersPlugin.ToDto).ToList();
        }

        [HttpGet("ValidateFile")]
        [Authorize(Policy = PolicyNames.RequireAdmin)]
        public ActionResult<List<VideoAudioFileDto>> ValidateFile(Guid fileId)
        {
            var user = this.GetUserFromContext();
            var file = ScreenPlayersPlugin.Files.FirstOrDefault(f => f.Id == fileId);
            if (file == null)
                return this.NotFound();

            var sourcePath = ScreenPlayersPlugin.GetStoredPath(file.Id, validated: false);
            var targetPath = ScreenPlayersPlugin.GetStoredPath(file.Id, validated: true);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            if (System.IO.File.Exists(sourcePath))
            {
                System.IO.File.Move(sourcePath, targetPath, overwrite: true);
            }
            else if (!System.IO.File.Exists(targetPath))
            {
                return this.NotFound("File not found on disk.");
            }

            file.Validated = true;
            file.Url = ScreenPlayersPlugin.GetPublicUrl(file.Id);

            return ScreenPlayersPlugin.Files.Select(ScreenPlayersPlugin.ToDto).ToList();
        }

        [HttpGet("myFiles")]
        public ActionResult<List<VideoAudioFileDto>> GetMyFiles()
        {
            var user = this.GetUserFromContext();

            return ScreenPlayersPlugin.Files
                .Where(f => f.CreatorName == user.Name)
                .Select(ScreenPlayersPlugin.ToDto)
                .ToList();
        }

        [HttpPost("uploadFile")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = Int32.MaxValue)]
        public ActionResult<VideoAudioFileDto> UploadFile([FromForm] IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return this.BadRequest("Missing file.");

            long maxBytes = ScreenPlayersPlugin.Obj.Config.MaxFileSizeInMB * 1024 * 1024;
            if (file.Length > maxBytes)
                return this.BadRequest($"File too large (max {ScreenPlayersPlugin.Obj.Config.MaxFileSizeInMB} MB).");

            var user = this.GetUserFromContext();

            var existingFiles = ScreenPlayersPlugin.Files.Where(f => f.CreatorName == user.Name).ToList();

            if (existingFiles.Count >= ScreenPlayersPlugin.Obj.Config.MaxUploadPerUser)
            {
                return this.BadRequest($"You already have uploaded {ScreenPlayersPlugin.Obj.Config.MaxUploadPerUser} files. Delete some to upload more.");
            }

            var originalName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalName).ToLowerInvariant();
            if (extension != ".mp4" && extension != ".mp3")
                return this.BadRequest("Only .mp4 or .mp3 are allowed.");

            var requireValidation = ScreenPlayersPlugin.Obj.Config.RequireValidation;
            var baseDir = requireValidation ? ScreenPlayersPlugin.NonValidatedDir : ScreenPlayersPlugin.ValidatedDir;
            Directory.CreateDirectory(baseDir);

            var id = Guid.NewGuid();
            var targetPath = Path.Combine(baseDir, $"{id}.mp4");

            if (extension == ".mp4")
            {
                using var output = System.IO.File.Create(targetPath);
                file.CopyTo(output);
            }
            else
            {
                var tempMp3 = Path.Combine(baseDir, $"{id}.mp3");
                using (var output = System.IO.File.Create(tempMp3))
                    file.CopyTo(output);

                var args = string.Join(" ",
                    "-y",
                    "-f lavfi -i color=c=black:s=1920x1080",
                    $"-i \"{tempMp3}\"",
                    "-map 0:v:0 -map 1:a:0",
                    "-c:v libx264 -preset veryfast -tune stillimage",
                    "-vf format=yuv420p",
                    "-c:a aac -b:a 192k",
                    "-shortest -pix_fmt yuv420p -movflags +faststart",
                    $"\"{targetPath}\""
                );

                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments = args,
                            CreateNoWindow = true,
                            UseShellExecute = false
                        }
                    };

                    process.Start();
                    process.WaitForExit(30_000);
                    if (process.ExitCode != 0 || !System.IO.File.Exists(targetPath))
                        return this.BadRequest("MP3 conversion failed (ffmpeg not available or error).");
                }
                finally
                {
                    if (System.IO.File.Exists(tempMp3))
                        System.IO.File.Delete(tempMp3);
                }
            }

            var entry = new VideoAudioFile(
                user.Name,
                extension == ".mp3" ? Path.ChangeExtension(originalName, ".mp4") : originalName,
                DateTime.UtcNow,
                validated: !requireValidation
            );
            entry.Id = id;
            entry.Url = entry.Validated ? ScreenPlayersPlugin.GetPublicUrl(entry.Id) : null;

            ScreenPlayersPlugin.Files.Add(entry);
            return ScreenPlayersPlugin.ToDto(entry);
        }
    }
}
