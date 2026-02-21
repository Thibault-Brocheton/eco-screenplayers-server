// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace CavRn.ScreenPlayers
{
    using Eco.Core.Controller;
    using Eco.Core.Utils;
    using Eco.Gameplay.Interactions.Interactors;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Systems.EnvVars;
    using Eco.Gameplay.Systems.NewTooltip;
    using Eco.Shared.Items;
    using Eco.Shared.Localization;
    using Eco.Shared.Networking;
    using Eco.Shared.Serialization;
    using Eco.Shared.SharedTypes;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System;

    public interface IScreenPlayersService
    {
        bool EnableWebUploader { get; }
        bool AllowOnlyLocalUrl { get; }
        string GetWebServerBaseUrl();
        string GetUploaderUrl();
        string GetPublicUrl(Guid id);
        ScreenPlayersFileInfo[] GetValidatedFiles();
    }

    public static class ScreenPlayersRegistry
    {
        public static IScreenPlayersService Obj;
    }

    public class ScreenPlayersFileInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string CreatorName { get; set; } = "";
    }

    [Serialized]
    public class VideoBaseItemData : IController, INotifyPropertyChanged, IClearRequestHandler
    {
        #region IController
        #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
        int            controllerID;
        public ref int ControllerID => ref this.controllerID;
        #endregion

        [Serialized, SyncToView] public string Url { get; set; } = "";
        [Serialized, SyncToView] public int Volume { get; set; } = -1;
        [Serialized, SyncToView] public int MaxDistance { get; set; } = -1;

        public bool HasDataThatCanBeCleared => this.Url != "";

        public VideoBaseWithoutInteractionComponent Parent { get; set; }

        public Result TryHandleClearRequest(Player player)
        {
            this.Url = "";
            return Result.Succeeded;
        }
    }

    [Serialized, HasIcon("ModulesComponent"), CreateComponentTabLoc("Video And Audio", true), LocDescription("Customize video and audio settings.")]
    public class VideoBaseWithoutInteractionComponent : WorldObjectComponent, IPersistentData
    {
        public override WorldObjectComponentClientAvailability Availability => WorldObjectComponentClientAvailability.Always;
        [SyncToView] public override string IconName => "ModulesComponent";

        [Serialized, SyncToView, Notify, EnvVar] public bool VideoStarted { get; set; }
        [Serialized, SyncToView, Notify, EnvVar] public bool VideoPaused  { get; set; }
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance)] public VideoBaseItemData VideoBaseItemData { get; set; } = new();

        public object PersistentData { get => this.VideoBaseItemData; set => this.VideoBaseItemData = value as VideoBaseItemData ?? new VideoBaseItemData(); }

        [Autogen, RPC, UITypeName("BigButton"), LocDescription("📤 Open Web Uploader")]
        public void OpenUploader(Player player)
        {
            var url = ScreenPlayersRegistry.Obj?.GetUploaderUrl();
            if (string.IsNullOrWhiteSpace(url))
            {
                player.MsgLoc($"Web uploader is not available on this server.");
                return;
            }

            player.User.OpenWebpage(url);
        }

        [Serialized] private string urlValidationError = "";
        [SyncToView, Autogen, PropReadOnly, UITypeName("StringDisplay")]
        public LocString UrlValidationError => Localizer.NotLocalizedStr(this.urlValidationError);

        [Autogen, SyncToView, AutoRPC, LocDescription("Enter the URL of your video or audio file.")]
        public string Url
        {
            get => this.VideoBaseItemData.Url;
            set
            {
                var nextValue = value ?? string.Empty;

                //Clear previous error
                this.urlValidationError = "";

                //Validate URL if AllowOnlyLocalUrl is enabled
                if (ScreenPlayersRegistry.Obj is { } service && service.AllowOnlyLocalUrl && nextValue.Length > 0)
                {
                    var baseUrl = service.GetWebServerBaseUrl();
                    if (!string.IsNullOrWhiteSpace(baseUrl) &&
                        !nextValue.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        this.urlValidationError = $"❌ Only local URLs are allowed on this server. Expected: {baseUrl}...";
                        this.Changed(nameof(this.UrlValidationError));
                        return;
                    }
                }

                this.VideoBaseItemData.Url = nextValue;
				this.Parent.SetAnimatedState("URL", nextValue);
            }
        }



        public virtual void Initialize(int volumeInit = 50, int maxDistanceInit = 16)
        {
            this.VideoBaseItemData ??= new VideoBaseItemData();
            this.VideoBaseItemData.Parent = this;

            if (this.Volume == -1)
            {
                this.Volume = volumeInit;
            }

            if (this.MaxDistance == -1)
            {
                this.MaxDistance = maxDistanceInit;
            }

            this.Parent.SetAnimatedState("Volume", (float)this.Volume / 100);
            this.Parent.SetAnimatedState("MaxDistance", this.MaxDistance);
            this.Parent.SetAnimatedState("URL", this.Url);

            this.Parent.SetAnimatedState("VideoStarted", this.VideoStarted);
            this.Parent.SetAnimatedState("VideoPaused", this.VideoPaused);

            this.Parent.OnEnableChange.Add(() =>
            {
                if (!this.Parent.Enabled)
                {
                    this.InternalStartStop(true);
                }
            });
        }

        [Autogen, SyncToView, AutoRPC] public int Volume
        {
            get => this.VideoBaseItemData.Volume;
            set
            {
                if (value > 100)
                {
                    value = 100;
                }

                if (value <= 0)
                {
                    value = 0;
                }

                this.VideoBaseItemData.Volume = value;
                this.Parent.SetAnimatedState("Volume", (float)value / 100);
            }
        }

        [Autogen, SyncToView, AutoRPC] public int MaxDistance
        {
            get => this.VideoBaseItemData.MaxDistance;
            set
            {
                if (value > 32)
                {
                    value = 32;
                }

                if (value <= 0)
                {
                    value = 0;
                }

                this.VideoBaseItemData.MaxDistance = value;
                this.Parent.SetAnimatedState("MaxDistance", value);
            }
        }

        protected void InternalStartStop(bool forceStop = false)
        {
            if ((!forceStop || !this.VideoStarted) && !this.Parent.Enabled)
            {
                return;
            }

            this.VideoStarted = !forceStop && !this.VideoStarted;
            this.Changed(nameof(this.VideoStarted));
            this.Parent.SetAnimatedState("VideoStarted", this.VideoStarted);

            this.VideoPaused = !this.VideoStarted;
            this.Changed(nameof(this.VideoPaused));
            this.Parent.SetAnimatedState("VideoPaused", this.VideoPaused);
        }

        protected void InternalPauseResume()
        {
            if (!this.VideoPaused && !this.Parent.Enabled)
            {
                return;
            }

            this.VideoPaused = !this.VideoPaused;
            this.Changed(nameof(this.VideoPaused));
            this.Parent.SetAnimatedState("VideoPaused", this.VideoPaused);
        }
    }

    [Serialized, HasIcon, CreateComponentTabLoc("Video And Audio", true), LocDescription("Customize video and audio settings.")]
    public class VideoBaseComponent : VideoBaseWithoutInteractionComponent
    {
        [Interaction(InteractionTrigger.RightClick, "Start/Stop", modifier: InteractionModifier.Shift, authRequired: AccessType.FullAccess)]
        public void StartStop(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.InternalStartStop();
        }

        [Interaction(InteractionTrigger.RightClick, "Pause/Resume", authRequired: AccessType.FullAccess)]
        public void PauseResume(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.InternalPauseResume();
        }
    }

    [Serialized, HasIcon, CreateComponentTabLoc("Vehicle Screen", true), LocDescription("Customize video and audio settings.")]
    public class VehicleScreenComponent : VideoBaseWithoutInteractionComponent
    {
        [RPC]
        public void StartStop(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.InternalStartStop();
        }

        [RPC]
        public void PauseResume(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.InternalPauseResume();
        }
    }

    [Serialized, HasIcon, CreateComponentTabLoc("Video", true), LocDescription("Customize video and audio settings.")]
    public class VideoComponent : VideoBaseComponent
    {
    }

    [Serialized, HasIcon, CreateComponentTabLoc("Cinema", true), LocDescription("Customize video and audio settings.")]
    public class CinemaComponent : VideoBaseComponent
    {
        private int maxMode = 10;

        public void Initialize(int volumeInit = 50, int maxDistanceInit = 16, int maxModeInit = 6)
        {
            this.maxMode = maxModeInit;
            this.Parent.SetAnimatedState("Mode", this.projectorDistance);
            base.Initialize(volumeInit, maxDistanceInit);
        }

        [Serialized] private int projectorDistance = 0;
        [Autogen, SyncToView, AutoRPC] public int ProjectorDistance
        {
            get => this.projectorDistance;
            set
            {
                if (value > this.maxMode)
                {
                    value = this.maxMode;
                }

                if (value < 0)
                {
                    value = 0;
                }

                this.projectorDistance = value;
                this.Parent.SetAnimatedState("Mode", this.projectorDistance);
            }
        }
    }

    [Serialized, HasIcon, CreateComponentTabLoc("Music", true), LocDescription("Customize audio settings.")]
    public class MusicComponent : VideoBaseComponent
    {
        public override void Initialize(int volumeInit = 50, int maxDistanceInit = 16)
        {
            base.Initialize(volumeInit, maxDistanceInit);
        }
    }
}
