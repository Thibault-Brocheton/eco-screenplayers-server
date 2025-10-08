using Eco.Gameplay.Objects;
using CavRn.ScreenPlayers;

namespace CavRnMods.HotWheels
{
    [RequireComponent(typeof(VehicleScreenComponent))]
    public partial class TeslaModel3Object: PhysicsWorldObject
    {
        protected override void PostInitialize()
        {
            base.PostInitialize();
            this.GetComponent<VehicleScreenComponent>().Initialize(50, 8);
        }
    }
}


