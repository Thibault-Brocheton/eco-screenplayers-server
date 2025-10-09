using Eco.Gameplay.Systems.EnvVars;
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

    [Serialized, HasIcon, CreateComponentTabLoc("Video And Audio", true), LocDescription("Customize video and audio settings.")]
    public class VideoBaseWithoutInteractionComponent : WorldObjectComponent
    {
        public override WorldObjectComponentClientAvailability Availability => WorldObjectComponentClientAvailability.Always;
        [Serialized, SyncToView, Notify, EnvVar] public bool VideoStarted { get; set; }
        [Serialized, SyncToView, Notify, EnvVar] public bool VideoPaused  { get; set; }

        [Serialized] protected string url = "";
        [Autogen, SyncToView, AutoRPC] public string Url
        {
            get => this.url;
            set
            {
                this.url = value;
				this.Parent.SetAnimatedState("URL", this.url);
            }
        }

        public virtual void Initialize(int volumeInit = 50, int maxDistanceInit = 16)
        {
            if (this.volume == 0)
            {
                this.volume = volumeInit;
            }

            if (this.maxDistance == 0)
            {
                this.maxDistance = maxDistanceInit;
            }

            this.Parent.SetAnimatedState("Volume", (float)this.volume / 100);
            this.Parent.SetAnimatedState("MaxDistance", this.maxDistance);
            this.Parent.SetAnimatedState("URL", this.url);

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

        [Serialized] protected int volume;
        [Autogen, SyncToView, AutoRPC] public int Volume
        {
            get => this.volume;
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

                this.volume = value;
                this.Parent.SetAnimatedState("Volume", (float)this.volume / 100);
            }
        }

        [Serialized] protected int maxDistance;
        [Autogen, SyncToView, AutoRPC] public int MaxDistance
        {
            get => this.maxDistance;
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

                this.maxDistance = value;
                this.Parent.SetAnimatedState("MaxDistance", this.maxDistance);
            }
        }

        protected void InternalStartStop(bool forceStop = false)
        {
            this.VideoStarted = !forceStop && !this.VideoStarted;
            this.Changed(nameof(this.VideoStarted));
            this.Parent.SetAnimatedState("VideoStarted", this.VideoStarted);

            this.VideoPaused = !this.VideoStarted;
            this.Changed(nameof(this.VideoPaused));
            this.Parent.SetAnimatedState("VideoPaused", this.VideoPaused);
        }

        protected void InternalPauseResume()
        {
            this.VideoPaused = !this.VideoPaused;
            this.Changed(nameof(this.VideoPaused));
            this.Parent.SetAnimatedState("VideoPaused", this.VideoPaused);
        }
    }

    [Serialized, HasIcon, CreateComponentTabLoc("Video And Audio", true), LocDescription("Customize video and audio settings.")]
    public class VideoBaseComponent : VideoBaseWithoutInteractionComponent
    {
        [Interaction(InteractionTrigger.RightClick, "Start/Stop", modifier: InteractionModifier.Shift, authRequired: AccessType.ConsumerAccess)]
        public void StartStop(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.InternalStartStop();
        }

        [Interaction(InteractionTrigger.RightClick, "Pause/Resume", authRequired: AccessType.ConsumerAccess)]
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
