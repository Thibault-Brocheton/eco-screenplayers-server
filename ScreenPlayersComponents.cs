using Eco.Shared.Localization;

namespace ScreenPlayers
{
    using Eco.Core.Controller;
    using Eco.Gameplay.Interactions.Interactors;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Players;
    using Eco.Shared.Items;
    using Eco.Shared.Networking;
    using Eco.Shared.Serialization;
    using Eco.Shared.SharedTypes;

    [Serialized, CreateComponentTabLoc("Video And Sound", true), HasIcon, LocDescription("Customize videos and audio settings.")]
    public class VideoComponent : WorldObjectComponent
    {
        public override WorldObjectComponentClientAvailability Availability => WorldObjectComponentClientAvailability.Always;
        [Serialized] private bool isRunning;

        [Serialized] private string url = "";
        [Autogen, SyncToView, AutoRPC] public string Url
        {
            get => this.url;
            set
            {
                this.url = value;
				this.Parent.SetAnimatedState("URL", this.url);
            }
        }

        [Interaction(InteractionTrigger.RightClick, "Restart", modifier: InteractionModifier.Shift, authRequired: AccessType.ConsumerAccess)]
        public void Restart(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.isRunning = true;
            this.Parent.SetAnimatedState("URL", this.url);
            this.Parent.TriggerAnimatedEvent("Restart");
        }

        [Interaction(InteractionTrigger.RightClick, "Resume", authRequired: AccessType.ConsumerAccess, DisallowedEnvVars = new[] { nameof(isRunning) })]
        [Interaction(InteractionTrigger.RightClick, "Pause", authRequired: AccessType.ConsumerAccess, RequiredEnvVars = new[] { nameof(isRunning) })]
        public void PauseOrResume(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
        {
            this.isRunning = !this.isRunning;
            this.Parent.SetAnimatedState("IsRunning", this.isRunning);
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
        }

        [Serialized] private int volume;
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

        [Serialized] private int maxDistance;
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
    }

    [Serialized, CreateComponentTabLoc("Cinema", true), HasIcon, LocDescription("Customize video and audio settings.")]
    public class CinemaComponent : VideoComponent
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

    [Serialized, CreateComponentTabLoc("Music", true), HasIcon, LocDescription("Customize audio settings.")]
    public class MusicComponent : VideoComponent
    {
        public override void Initialize(int volumeInit = 50, int maxDistanceInit = 16)
        {
            base.Initialize(volumeInit, maxDistanceInit);
        }
    }
}
