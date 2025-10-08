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
        public override LocString DisplayName => Localizer.DoStr("RecordPlayer");
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
    [LocDisplayName("RecordPlayer")]
    [LocDescription("A record player to play your favorite songs with your mates.")]
    [Ecopedia("Housing Objects", "Living Room", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(2000)]
    [Tag(nameof(SurfaceTags.CanBeOnSurface))]
    public class RecordPlayerItem : WorldObjectItem<RecordPlayerObject>
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
                displayName: Localizer.DoStr("RecordPlayer"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("WoodBoard", 16, typeof(BasicEngineeringSkill), typeof(BasicEngineeringLavishResourcesTalent)),
                    new IngredientElement(typeof(IronBarItem), 8, typeof(BasicEngineeringSkill), typeof(BasicEngineeringLavishResourcesTalent)),
                    new IngredientElement(typeof(CopperBarItem), 4, typeof(BasicEngineeringSkill), typeof(BasicEngineeringLavishResourcesTalent)),
                    new IngredientElement(typeof(GoldBarItem), 2, typeof(BasicEngineeringSkill), typeof(BasicEngineeringLavishResourcesTalent)),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<RecordPlayerItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 6;

            this.LaborInCalories = CreateLaborInCaloriesValue(120, typeof(BasicEngineeringSkill));

            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(RecordPlayerRecipe), start: 10, skillType: typeof(BasicEngineeringSkill), typeof(BasicEngineeringFocusedSpeedTalent), typeof(BasicEngineeringParallelSpeedTalent));

            this.Initialize(displayText: Localizer.DoStr("RecordPlayer"), recipeType: typeof(RecordPlayerRecipe));

            CraftingComponent.AddRecipe(tableType: typeof(WainwrightTableObject), recipeFamily: this);
        }
    }
}
