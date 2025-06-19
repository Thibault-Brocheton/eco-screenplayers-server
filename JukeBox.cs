namespace ScreenPlayers
{
    using Eco.Core.Items;
    using Eco.Gameplay.Components.Auth;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.Housing.PropertyValues;
    using Eco.Gameplay.Housing;
    using Eco.Gameplay.Items.Recipes;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Occupancy;
    using Eco.Gameplay.Property;
    using Eco.Gameplay.Skills;
    using Eco.Gameplay.Systems.NewTooltip;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Mods.TechTree;
    using Eco.Shared.Items;
    using Eco.Shared.Localization;
    using Eco.Shared.Math;
    using Eco.Shared.Serialization;
    using Eco.Shared.Utils;
    using System.Collections.Generic;
    using System;

    [Serialized]
    [RequireComponent(typeof(OnOffComponent))]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(PowerGridComponent))]
    [RequireComponent(typeof(PowerConsumptionComponent))]
    [RequireComponent(typeof(HousingComponent))]
    [RequireComponent(typeof(OccupancyRequirementComponent))]
    [RequireComponent(typeof(ForSaleComponent))]
    [RequireComponent(typeof(MusicComponent))]
    [RequireComponent(typeof(RoomRequirementsComponent))]
    [RequireRoomContainment]
    [RequireRoomVolume(4)]
    [Tag("Usable")]
    [Ecopedia("Housing Objects", "Living Room", subPageName: "JukeBox Item")]
    public class JukeBoxObject : WorldObject, IRepresentsItem
    {
        public virtual Type RepresentedItemType => typeof(JukeBoxItem);
        public override LocString DisplayName => Localizer.DoStr("JukeBox");
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        protected override void Initialize()
        {
            this.GetComponent<PowerConsumptionComponent>().Initialize(200);
            this.GetComponent<PowerGridComponent>().Initialize(10, new MechanicalPower());
            this.GetComponent<HousingComponent>().HomeValue = JukeBoxItem.homeValue;
            this.GetComponent<MusicComponent>().Initialize(50, 12);
        }

        static JukeBoxObject()
        {
            WorldObject.AddOccupancy<JukeBoxObject>(new List<BlockOccupancy>()
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(1, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 1, 0)),
                new BlockOccupancy(new Vector3i(1, 1, 0)),
            });
        }
    }

    [Serialized]
    [LocDisplayName("JukeBox")]
    [LocDescription("A jukebox to play your favorite songs with your mates.")]
    [Ecopedia("Housing Objects", "Living Room", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(2000)]
    [Tag(nameof(SurfaceTags.CanBeOnRug))]
    public class JukeBoxItem : WorldObjectItem<JukeBoxObject>
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext( 0  | DirectionAxisFlags.Down , WorldObject.GetOccupancyInfo(this.WorldObjectType));
        public override HomeFurnishingValue HomeValue => homeValue;
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue()
        {
            ObjectName                              = typeof(JukeBoxObject).UILink(),
            Category                                = HousingConfig.GetRoomCategory("Living Room"),
            BaseValue                               = 6,
            TypeForRoomLimit                        = Localizer.DoStr("Music"),
            DiminishingReturnMultiplier             = 0.1f
        };

        [NewTooltip(CacheAs.SubType, 7)] public static LocString PowerConsumptionTooltip() => Localizer.Do($"Consumes: {Text.Info(200)}w of {new MechanicalPower().Name} power.");
    }

    [RequiresSkill(typeof(MechanicsSkill), 2)]
    [Ecopedia("Housing Objects", "Living Room", subPageName: "JukeBox Item")]
    public class JukeBoxRecipe : RecipeFamily
    {
        public JukeBoxRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "JukeBox",  //noloc
                displayName: Localizer.DoStr("JukeBox"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("Lumber", 16, typeof(MechanicsSkill), typeof(MechanicsLavishResourcesTalent)),
                    new IngredientElement(typeof(CopperWiringItem), 24, typeof(MechanicsSkill), typeof(MechanicsLavishResourcesTalent)),
                    new IngredientElement(typeof(ScrewsItem), 32, typeof(MechanicsSkill), typeof(MechanicsLavishResourcesTalent)),
                    new IngredientElement(typeof(IronPlateItem), 12, typeof(MechanicsSkill), typeof(MechanicsLavishResourcesTalent)),
                    new IngredientElement(typeof(GlassItem), 6, typeof(MechanicsSkill), typeof(MechanicsLavishResourcesTalent)),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<JukeBoxItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 6;

            this.LaborInCalories = CreateLaborInCaloriesValue(240, typeof(MechanicsSkill));

            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(JukeBoxRecipe), start: 16, skillType: typeof(MechanicsSkill), typeof(MechanicsFocusedSpeedTalent), typeof(MechanicsParallelSpeedTalent));

            this.Initialize(displayText: Localizer.DoStr("JukeBox"), recipeType: typeof(JukeBoxRecipe));

            CraftingComponent.AddRecipe(tableType: typeof(AssemblyLineObject), recipeFamily: this);
        }
    }
}
