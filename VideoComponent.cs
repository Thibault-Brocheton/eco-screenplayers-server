namespace ScreenPlayers
{
    using Eco.Core.Controller;
    using Eco.Gameplay.Interactions.Interactors;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Players;
    using Eco.Plugins.Networking;
    using Eco.Shared.Items;
    using Eco.Shared.Logging;
    using Eco.Shared.Networking;
    using Eco.Shared.Serialization;
    using Eco.Shared.SharedTypes;
    using Xabe.FFmpeg.Downloader;
    using YoutubeExplode.Converter;
    using YoutubeExplode;

    [Serialized, CreateComponentTabLoc, HasIcon]
    public class VideoComponent : WorldObjectComponent
    {
        public override WorldObjectComponentClientAvailability Availability => WorldObjectComponentClientAvailability.Always;
        private bool isPaused = false;
        private const string Folder = "WebClient/WebBin/Videos";
        private const string VideosFolder = "Videos";

        static VideoComponent()
        {
            try
            {
                Directory.CreateDirectory(Folder);
                FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.WriteLineLoc($"Error during download of ffmpeg !");
                Log.WriteWarningLineLocStr(ex.Message);
            }
        }

        [Serialized] private string internalUrl = "";
        [Serialized] private string url = "";
        [Autogen, SyncToView, AutoRPC] public string Url
        {
            get => this.url;
            set
            {
                Console.WriteLine($"Set URL {value}");

                this.url = value;

                if (value.Contains("youtube.com/watch"))
                {
                    this.DownloadYoutube(value).ConfigureAwait(false);
                }
                else
                {
                    this.Parent.SetAnimatedState("URL", this.url);
                }
            }
        }

        private async Task DownloadYoutube(string youtubeUrl)
        {
            try
            {
                Log.WriteLineLoc($"Downloading youtube video {youtubeUrl} ...");

                var id = youtubeUrl.Split('=').Last();

                var youtube = new YoutubeClient();
                await youtube.Videos.DownloadAsync(youtubeUrl, $"{Folder}/{id}.mp4");

                this.internalUrl = $"{NetworkManager.Config.WebServerUrl}/{VideosFolder}/{id}.mp4";
                this.Parent.SetAnimatedState("URL", this.internalUrl);

                Log.WriteLineLoc($"Youtube video {youtubeUrl} successfully downloaded !");
            }
            catch (Exception ex)
            {
                Log.WriteLineLoc($"Error during download of video {youtubeUrl} !");
                Log.WriteWarningLineLocStr(ex.Message);
            }
        }

        [Interaction(InteractionTrigger.RightClick, "Restart", authRequired: AccessType.ConsumerAccess)]
        public void Restart(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            Console.WriteLine("Restart");
            this.isPaused = false;
            this.Parent.SetAnimatedState("PauseOrResume", this.isPaused);
            this.Parent.TriggerAnimatedEvent("Restart");
        }

        [Interaction(InteractionTrigger.LeftClick, "PauseOrResume", authRequired: AccessType.ConsumerAccess)]
        public void PauseOrResume(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            Console.WriteLine("PauseOrResume");
            this.isPaused = !this.isPaused;
            this.Parent.SetAnimatedState("PauseOrResume", this.isPaused);
        }
    }
}
