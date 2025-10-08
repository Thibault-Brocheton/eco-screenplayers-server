namespace CavRn.ScreenPlayers
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
    [RequireComponent(typeof(VideoComponent))]
    [RequireComponent(typeof(MinimapComponent))]
    [RequireComponent(typeof(RoomRequirementsComponent))]
    [RequireRoomContainment]
    [RequireRoomVolume(8)]
    [Tag("Usable")]
    [Ecopedia("Housing Objects", "Living Room", subPageName: "Flat Screen Television Item")]
    public class FlatScreenTelevisionObject : WorldObject, IRepresentsItem
    {
        public virtual Type RepresentedItemType => typeof(FlatScreenTelevisionItem);
        public override LocString DisplayName => Localizer.DoStr("FlatScreenTelevision");
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        protected override void Initialize()
        {
            this.GetComponent<PowerConsumptionComponent>().Initialize(100);
            this.GetComponent<PowerGridComponent>().Initialize(10, new ElectricPower());
            this.GetComponent<HousingComponent>().HomeValue = FlatScreenTelevisionItem.homeValue;
            this.GetComponent<MinimapComponent>().SetCategory(Localizer.DoStr("Television"));
            this.GetComponent<VideoComponent>().Initialize(50, 8);
        }

        static FlatScreenTelevisionObject()
        {
            WorldObject.AddOccupancy<FlatScreenTelevisionObject>(new List<BlockOccupancy>()
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(1, 0, 0)),
            });
        }
    }

    [Serialized]
    [LocDisplayName("FlatScreenTelevision")]
    [LocDescription("A flat screen television to play your favorite videos with your mates.")]
    [Ecopedia("Housing Objects", "Living Room", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(2000)]
    [Tag(nameof(SurfaceTags.CanBeOnSurface))]
    public class FlatScreenTelevisionItem : WorldObjectItem<FlatScreenTelevisionObject>
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext( 0  | DirectionAxisFlags.Down , WorldObject.GetOccupancyInfo(this.WorldObjectType));
        public override HomeFurnishingValue HomeValue => homeValue;
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue()
        {
            ObjectName                              = typeof(FlatScreenTelevisionObject).UILink(),
            Category                                = HousingConfig.GetRoomCategory("Living Room"),
            BaseValue                               = 8,
            TypeForRoomLimit                        = Localizer.DoStr("FlatScreenTelevision"),
            DiminishingReturnMultiplier             = 0.1f
        };

        [NewTooltip(CacheAs.SubType, 7)] public static LocString PowerConsumptionTooltip() => Localizer.Do($"Consumes: {Text.Info(100)}w of {new ElectricPower().Name} power.");
    }

    [RequiresSkill(typeof(ElectronicsSkill), 2)]
    [Ecopedia("Housing Objects", "Living Room", subPageName: "Flat Screen Television Item")]
    public class FlatScreenTelevisionRecipe : RecipeFamily
    {
        public FlatScreenTelevisionRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "FlatScreenTelevision",  //noloc
                displayName: Localizer.DoStr("FlatScreenTelevision"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(SteelPlateItem), 8, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(BasicCircuitItem), 4, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(RadiatorItem), 1, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(GlassItem), 4, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<FlatScreenTelevisionItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 6;

            this.LaborInCalories = CreateLaborInCaloriesValue(120, typeof(ElectronicsSkill));

            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(FlatScreenTelevisionRecipe), start: 12, skillType: typeof(ElectronicsSkill), typeof(ElectronicsFocusedSpeedTalent), typeof(ElectronicsParallelSpeedTalent));

            this.Initialize(displayText: Localizer.DoStr("FlatScreenTelevision"), recipeType: typeof(FlatScreenTelevisionRecipe));

            CraftingComponent.AddRecipe(tableType: typeof(ElectronicsAssemblyObject), recipeFamily: this);
        }
    }
}
