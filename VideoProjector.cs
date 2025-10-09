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
    [Ecopedia("Housing Objects", "Cultural", subPageName: "Video Projector Item")]
    public class VideoProjectorObject : WorldObject, IRepresentsItem
    {
        public virtual Type RepresentedItemType => typeof(VideoProjectorItem);
        public override LocString DisplayName => Localizer.DoStr("VideoProjector");
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        protected override void Initialize()
        {
            this.GetComponent<PowerConsumptionComponent>().Initialize(300);
            this.GetComponent<PowerGridComponent>().Initialize(10, new ElectricPower());
            this.GetComponent<HousingComponent>().HomeValue = VideoProjectorItem.homeValue;
            this.GetComponent<MinimapComponent>().SetCategory(Localizer.DoStr("Television"));
            this.GetComponent<PartsComponent>().Config(() => LocString.Empty, new PartInfo[] {
                new() { TypeName = nameof(LightBulbItem), Quantity = 1}
            });
            this.GetComponent<CinemaComponent>().Initialize(50, 16, 6);
        }

        static VideoProjectorObject()
        {
            WorldObject.AddOccupancy<VideoProjectorObject>(new List<BlockOccupancy>()
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
            });
        }
    }

    [Serialized]
    [LocDisplayName("VideoProjector")]
    [LocDescription("A video projector to play your favorite videos with your mates.")]
    [Ecopedia("Housing Objects", "Cultural", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(2000)]
    [Tag(nameof(SurfaceTags.CanBeOnSurface))]
    public class VideoProjectorItem : WorldObjectItem<VideoProjectorObject>, IPersistentData
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext( 0  | DirectionAxisFlags.Down , WorldObject.GetOccupancyInfo(this.WorldObjectType));
        public override HomeFurnishingValue HomeValue => homeValue;
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue()
        {
            ObjectName                              = typeof(VideoProjectorObject).UILink(),
            Category                                = HousingConfig.GetRoomCategory("Cultural"),
            BaseValue                               = 10,
            TypeForRoomLimit                        = Localizer.DoStr("Cultural"),
            DiminishingReturnMultiplier             = 0.1f
        };

        [NewTooltip(CacheAs.SubType, 7)] public static LocString PowerConsumptionTooltip() => Localizer.Do($"Consumes: {Text.Info(300)}w of {new ElectricPower().Name} power.");
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance, flags: TTFlags.AllowNonControllerTypeForChildren)] public object PersistentData { get; set; }
    }

    [RequiresSkill(typeof(ElectronicsSkill), 2)]
    [Ecopedia("Housing Objects", "Cultural", subPageName: "Video Projector Item")]
    public class VideoProjectorRecipe : RecipeFamily
    {
        public VideoProjectorRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "VideoProjector",  //noloc
                displayName: Localizer.DoStr("VideoProjector"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(SteelPlateItem), 12, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(BasicCircuitItem), 12, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(CopperWiringItem), 8, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(RadiatorItem), 3, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(GlassItem), 2, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(HeatSinkItem), 4, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(ScrewsItem), 8, typeof(ElectronicsSkill), typeof(ElectronicsLavishResourcesTalent)),
                    new IngredientElement(typeof(LightBulbItem), 1, true),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<VideoProjectorItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 15;

            this.LaborInCalories = CreateLaborInCaloriesValue(220, typeof(ElectronicsSkill));

            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(VideoProjectorRecipe), start: 24, skillType: typeof(ElectronicsSkill), typeof(ElectronicsFocusedSpeedTalent), typeof(ElectronicsParallelSpeedTalent));

            this.Initialize(displayText: Localizer.DoStr("VideoProjector"), recipeType: typeof(VideoProjectorRecipe));

            CraftingComponent.AddRecipe(tableType: typeof(ElectronicsAssemblyObject), recipeFamily: this);
        }
    }
}
