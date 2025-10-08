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
        [Serialized] public bool isRunning;

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

            if (this.isRunning)
            {
                this.Parent.SetAnimatedState("Start", this.isRunning);
            }
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

        protected void InternalStop()
        {
            this.isRunning = false;
            this.Parent.SetAnimatedState("Start", this.isRunning);
            this.Parent.TriggerAnimatedEvent("Stop");
        }

        protected void InternalStart()
        {
            this.isRunning = !this.isRunning;
            this.Parent.SetAnimatedState("Start", this.isRunning);
        }
    }

    [Serialized, HasIcon, CreateComponentTabLoc("Video And Audio", true), LocDescription("Customize video and audio settings.")]
    public class VideoBaseComponent : VideoBaseWithoutInteractionComponent
    {
        [Interaction(InteractionTrigger.RightClick, "Stop", modifier: InteractionModifier.Shift, authRequired: AccessType.ConsumerAccess)]
        public void Stop(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.InternalStop();
        }

        [Interaction(InteractionTrigger.RightClick, "Start", authRequired: AccessType.ConsumerAccess)]
        public void Start(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.InternalStart();
        }
    }

    [Serialized, HasIcon, CreateComponentTabLoc("Vehicle Screen", true), LocDescription("Customize video and audio settings.")]
    public class VehicleScreenComponent : VideoBaseWithoutInteractionComponent
    {
        [RPC]
        public void Stop(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.InternalStop();
        }

        [RPC]
        public void Start(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.InternalStart();
        }
    }

    [Serialized, HasIcon, CreateComponentTabLoc("Video", true), LocDescription("Customize video and audio settings.")]
    public class VideoComponent : VideoBaseComponent
    {
    }

    [Serialized, HasIcon, CreateComponentTabLoc("Cinema", true), LocDescription("Customize video and audio settings.")]
    public class CinemaComponent : VideoBaseComponent
    {
        public override void Initialize(int volumeInit = 50, int maxDistanceInit = 16)
        {
            this.Parent.SetAnimatedState("Mode", this.projectorDistance);
            base.Initialize(volumeInit, maxDistanceInit);
        }

        [Serialized] private int projectorDistance = 0;
        [Autogen, SyncToView, AutoRPC] public int ProjectorDistance
        {
            get => this.projectorDistance;
            set
            {
                if (value > 7)
                {
                    value = 7;
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
