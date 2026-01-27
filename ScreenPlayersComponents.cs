using System.ComponentModel;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.EnvVars;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.Localization;

namespace CavRn.ScreenPlayers
{
    using Eco.Core.Controller;
    using Eco.Gameplay.Interactions.Interactors;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Players;
    using Eco.Shared.Items;
    using Eco.Shared.Networking;
    using Eco.Shared.Serialization;
    using Eco.Shared.SharedTypes;

    [Serialized]
    public class VideoBaseItemData : IController, INotifyPropertyChanged, IClearRequestHandler
    {
        #region IController
        public event PropertyChangedEventHandler? PropertyChanged;
        int            controllerID;
        public ref int ControllerID => ref this.controllerID;
        #endregion

        [Serialized, SyncToView] public string Url { get; set; } = "";
        [Serialized, SyncToView] public int Volume { get; set; } = -1;
        [Serialized, SyncToView] public int MaxDistance { get; set; } = -1;

        public bool HasDataThatCanBeCleared => Url != "";

        public VideoBaseWithoutInteractionComponent? Parent { get; set; }

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
        [Serialized, SyncToView, Notify, EnvVar] public bool VideoStarted { get; set; }
        [Serialized, SyncToView, Notify, EnvVar] public bool VideoPaused  { get; set; }
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance)] public VideoBaseItemData VideoBaseItemData { get; set; } = new();

        public object PersistentData { get => this.VideoBaseItemData; set => this.VideoBaseItemData = value as VideoBaseItemData ?? new VideoBaseItemData(); }

        [Autogen, SyncToView, AutoRPC] public string Url
        {
            get => this.VideoBaseItemData.Url;
            set
            {
                this.VideoBaseItemData.Url = value;
				this.Parent.SetAnimatedState("URL", value);
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
