using Eco.Core.Plugins.Interfaces;
using Eco.Shared.Localization;

namespace CavRn.ScreenPlayers
{
    public class ScreenPlayersMod: IModInit
    {
        public static ModRegistration Register() => new()
        {
            ModName = "ScreenPlayers",
            ModDescription = "ScreenPlayers introduces televisions and music players. Play your favorite songs and videos with your friends!",
            ModDisplayName = "Screen Players"
        };
    }
}


