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
    [Tag("Usable")]
    [Ecopedia("Housing Objects", "Living Room", subPageName: "Radio Item")]
    public class RadioObject : WorldObject, IRepresentsItem
    {
        public virtual Type RepresentedItemType => typeof(RadioItem);
        public override LocString DisplayName => Localizer.DoStr("Radio");
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        protected override void Initialize()
        {
            this.GetComponent<PowerConsumptionComponent>().Initialize(10);
            this.GetComponent<PowerGridComponent>().Initialize(10, new MechanicalPower());
            this.GetComponent<MusicComponent>().Initialize(50, 18);
        }

        static RadioObject()
        {
            WorldObject.AddOccupancy<RadioObject>(new List<BlockOccupancy>()
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
            });
        }
    }

    [Serialized]
    [LocDisplayName("Radio")]
    [LocDescription("A radio to play your favorite songs with your mates.")]
    [Weight(500)]
    [Tag(nameof(SurfaceTags.CanBeOnRug))]
    public class RadioItem : WorldObjectItem<RadioObject>
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext( 0  | DirectionAxisFlags.Down , WorldObject.GetOccupancyInfo(this.WorldObjectType));

        [NewTooltip(CacheAs.SubType, 7)] public static LocString PowerConsumptionTooltip() => Localizer.Do($"Consumes: {Text.Info(10)}w of {new MechanicalPower().Name} power.");
    }

    [RequiresSkill(typeof(BasicEngineeringSkill), 2)]
    [Ecopedia("Housing Objects", "Living Room", subPageName: "Radio Item")]
    public class RadioRecipe : RecipeFamily
    {
        public RadioRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Radio",  //noloc
                displayName: Localizer.DoStr("Radio"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("WoodBoard", 8, typeof(BasicEngineeringSkill), typeof(BasicEngineeringLavishResourcesTalent)),
                    new IngredientElement(typeof(IronBarItem), 4, typeof(BasicEngineeringSkill), typeof(BasicEngineeringLavishResourcesTalent)),
                    new IngredientElement(typeof(IronPipeItem), 4, typeof(BasicEngineeringSkill), typeof(BasicEngineeringLavishResourcesTalent)),
                    new IngredientElement(typeof(CopperBarItem), 2, typeof(BasicEngineeringSkill), typeof(BasicEngineeringLavishResourcesTalent)),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<RadioItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 3;

            this.LaborInCalories = CreateLaborInCaloriesValue(80, typeof(BasicEngineeringSkill));

            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(RadioRecipe), start: 4, skillType: typeof(BasicEngineeringSkill), typeof(BasicEngineeringFocusedSpeedTalent), typeof(BasicEngineeringParallelSpeedTalent));

            this.Initialize(displayText: Localizer.DoStr("Radio"), recipeType: typeof(RadioRecipe));

            CraftingComponent.AddRecipe(tableType: typeof(WainwrightTableObject), recipeFamily: this);
        }
    }
}
