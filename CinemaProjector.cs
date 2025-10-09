using Eco.Core.Controller;

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
    using static Eco.Gameplay.Components.PartsComponent;

    [Serialized]
    [RequireComponent(typeof(OnOffComponent))]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(PowerGridComponent))]
    [RequireComponent(typeof(PowerConsumptionComponent))]
    [RequireComponent(typeof(HousingComponent))]
    [RequireComponent(typeof(OccupancyRequirementComponent))]
    [RequireComponent(typeof(ForSaleComponent))]
    [RequireComponent(typeof(MinimapComponent))]
    [RequireComponent(typeof(CinemaComponent))]
    [RequireComponent(typeof(PartsComponent))]
    [Tag("Usable")]
    [Ecopedia("Housing Objects", "Cultural", subPageName: "Cinema Projector Item")]
    public class CinemaProjectorObject : WorldObject, IRepresentsItem
    {
        public virtual Type RepresentedItemType => typeof(CinemaProjectorItem);
        public override LocString DisplayName => Localizer.DoStr("CinemaProjector");
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        protected override void Initialize()
        {
            this.GetComponent<PowerConsumptionComponent>().Initialize(600);
            this.GetComponent<PowerGridComponent>().Initialize(10, new ElectricPower());
            this.GetComponent<HousingComponent>().HomeValue = CinemaProjectorItem.homeValue;
            this.GetComponent<MinimapComponent>().SetCategory(Localizer.DoStr("Television"));
            this.GetComponent<PartsComponent>().Config(() => LocString.Empty, new PartInfo[] {
                new() { TypeName = nameof(LightBulbItem), Quantity = 2}
            });
            this.GetComponent<CinemaComponent>().Initialize(50, 24, 10);
        }

        static CinemaProjectorObject()
        {
            WorldObject.AddOccupancy<CinemaProjectorObject>(new List<BlockOccupancy>()
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
            });
        }
    }

    [Serialized]
    [LocDisplayName("CinemaProjector")]
    [LocDescription("A cinema projector to play your favorite films with your mates.")]
    [Ecopedia("Housing Objects", "Cultural", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(2500)]
    [Tag(nameof(SurfaceTags.CanBeOnSurface))]
    public class CinemaProjectorItem : WorldObjectItem<CinemaProjectorObject>, IPersistentData
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext( 0  | DirectionAxisFlags.Up , WorldObject.GetOccupancyInfo(this.WorldObjectType));
        public override HomeFurnishingValue HomeValue => homeValue;
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue()
        {
            ObjectName                              = typeof(CinemaProjectorObject).UILink(),
            Category                                = HousingConfig.GetRoomCategory("Cultural"),
            BaseValue                               = 12,
            TypeForRoomLimit                        = Localizer.DoStr("Cultural"),
            DiminishingReturnMultiplier             = 0.1f
        };

        [NewTooltip(CacheAs.SubType, 7)] public static LocString PowerConsumptionTooltip() => Localizer.Do($"Consumes: {Text.Info(600)}w of {new ElectricPower().Name} power.");
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance, flags: TTFlags.AllowNonControllerTypeForChildren)] public object PersistentData { get; set; }
    }

    [RequiresSkill(typeof(ElectronicsSkill), 3)]
    [Ecopedia("Housing Objects", "Cultural", subPageName: "Cinema Projector Item")]
    public class CinemaProjectorRecipe : RecipeFamily
    {
        public CinemaProjectorRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "CinemaProjector",  //noloc
                displayName: Localizer.DoStr("CinemaProjector"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(SteelPlateItem), 12, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(BasicCircuitItem), 16, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(CopperWiringItem), 12, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(RadiatorItem), 6, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(GlassItem), 4, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(HeatSinkItem), 8, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(ScrewsItem), 12, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(LightBulbItem), 2, true),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<CinemaProjectorItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 18;

            this.LaborInCalories = CreateLaborInCaloriesValue(300, typeof(ElectronicsSkill));

            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(CinemaProjectorRecipe), start: 30, skillType: typeof(ElectronicsSkill), typeof(ElectronicsFocusedSpeedTalent), typeof(ElectronicsParallelSpeedTalent));

            this.Initialize(displayText: Localizer.DoStr("CinemaProjector"), recipeType: typeof(CinemaProjectorRecipe));

            CraftingComponent.AddRecipe(tableType: typeof(ElectronicsAssemblyObject), recipeFamily: this);
        }
    }
}
