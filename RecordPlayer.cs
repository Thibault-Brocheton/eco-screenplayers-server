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
    [Ecopedia("Housing Objects", "Living Room", subPageName: "RecordPlayer Item")]
    public class RecordPlayerObject : WorldObject, IRepresentsItem
    {
        public virtual Type RepresentedItemType => typeof(RecordPlayerItem);
        public override LocString DisplayName => Localizer.DoStr("Record Player");
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        protected override void Initialize()
        {
            this.GetComponent<PowerConsumptionComponent>().Initialize(10);
            this.GetComponent<PowerGridComponent>().Initialize(10, new MechanicalPower());
            this.GetComponent<HousingComponent>().HomeValue = RecordPlayerItem.homeValue;
            this.GetComponent<MusicComponent>().Initialize(50, 10);
        }

        static RecordPlayerObject()
        {
            WorldObject.AddOccupancy<RecordPlayerObject>(new List<BlockOccupancy>()
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
            });
        }
    }

    [Serialized]
    [LocDisplayName("Record Player")]
    [LocDescription("A record player to play your favorite songs with your mates.")]
    [Ecopedia("Housing Objects", "Living Room", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(2000)]
    [SalvageCost(typeof(CopperScrap), 0.4f, typeof(GoldScrap), 0.2f, typeof(IronScrap), 0.8f, typeof(WoodScrap), 3.2f)]
    [Tag(nameof(SurfaceTags.CanBeOnSurface))]
    public class RecordPlayerItem : WorldObjectItem<RecordPlayerObject>, IPersistentData
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext( 0  | DirectionAxisFlags.Down , WorldObject.GetOccupancyInfo(this.WorldObjectType));
        public override HomeFurnishingValue HomeValue => homeValue;
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue()
        {
            ObjectName                              = typeof(RecordPlayerObject).UILink(),
            Category                                = HousingConfig.GetRoomCategory("Living Room"),
            BaseValue                               = 3,
            TypeForRoomLimit                        = Localizer.DoStr("Music"),
            DiminishingReturnMultiplier             = 0.1f
        };
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance, flags: TTFlags.AllowNonControllerTypeForChildren)] public object? PersistentData { get; set; }
        [NewTooltip(CacheAs.SubType, 7)] public static LocString PowerConsumptionTooltip() => Localizer.Do($"Consumes: {Text.Info(10)}w of {new MechanicalPower().Name} power.");
    }

    [RequiresSkill(typeof(BasicEngineeringSkill), 2)]
    [Ecopedia("Housing Objects", "Living Room", subPageName: "RecordPlayer Item")]
    public class RecordPlayerRecipe : RecipeFamily
    {
        public RecordPlayerRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "RecordPlayer",  //noloc
                displayName: Localizer.DoStr("Record Player"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("WoodBoard", 8, typeof(BasicEngineeringSkill)),
                    new IngredientElement(typeof(IronBarItem), 4, typeof(BasicEngineeringSkill)),
                    new IngredientElement(typeof(CopperBarItem), 2, typeof(BasicEngineeringSkill)),
                    new IngredientElement(typeof(GoldBarItem), 1, typeof(BasicEngineeringSkill)),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<RecordPlayerItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 6;

            this.LaborInCalories = CreateLaborInCaloriesValue(120, typeof(BasicEngineeringSkill));

            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(RecordPlayerRecipe), start: 5, skillType: typeof(BasicEngineeringSkill));

            this.Initialize(displayText: Localizer.DoStr("Record Player"), recipeType: typeof(RecordPlayerRecipe));

            CraftingComponent.AddRecipe(tableType: typeof(WainwrightTableObject), recipeFamily: this);
        }
    }
}
