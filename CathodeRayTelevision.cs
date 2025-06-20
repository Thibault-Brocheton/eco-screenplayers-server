﻿namespace ScreenPlayers
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
    [RequireRoomVolume(4)]
    [Tag("Usable")]
    [Ecopedia("Housing Objects", "Living Room", subPageName: "Cathode Ray Television Item")]
    public class CathodeRayTelevisionObject : WorldObject, IRepresentsItem
    {
        public virtual Type RepresentedItemType => typeof(CathodeRayTelevisionItem);
        public override LocString DisplayName => Localizer.DoStr("CathodeRayTelevision");
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        protected override void Initialize()
        {
            this.GetComponent<PowerConsumptionComponent>().Initialize(300);
            this.GetComponent<PowerGridComponent>().Initialize(10, new MechanicalPower());
            this.GetComponent<HousingComponent>().HomeValue = CathodeRayTelevisionItem.homeValue;
            this.GetComponent<MinimapComponent>().SetCategory(Localizer.DoStr("Television"));
            this.GetComponent<VideoComponent>().Initialize(50, 6);
        }

        static CathodeRayTelevisionObject()
        {
            WorldObject.AddOccupancy<CathodeRayTelevisionObject>(new List<BlockOccupancy>()
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
            });
        }
    }

    [Serialized]
    [LocDisplayName("CathodeRayTelevision")]
    [LocDescription("A cathode ray television to play your favorite videos with your mates.")]
    [Ecopedia("Housing Objects", "Living Room", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(2000)]
    [Tag(nameof(SurfaceTags.CanBeOnRug))]
    public class CathodeRayTelevisionItem : WorldObjectItem<CathodeRayTelevisionObject>
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext( 0  | DirectionAxisFlags.Down , WorldObject.GetOccupancyInfo(this.WorldObjectType));
        public override HomeFurnishingValue HomeValue => homeValue;
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue()
        {
            ObjectName                              = typeof(CathodeRayTelevisionObject).UILink(),
            Category                                = HousingConfig.GetRoomCategory("Living Room"),
            BaseValue                               = 5,
            TypeForRoomLimit                        = Localizer.DoStr("CathodeRayTelevision"),
            DiminishingReturnMultiplier             = 0.1f
        };

        [NewTooltip(CacheAs.SubType, 7)] public static LocString PowerConsumptionTooltip() => Localizer.Do($"Consumes: {Text.Info(300)}w of {new MechanicalPower().Name} power.");
    }

    [RequiresSkill(typeof(MechanicsSkill), 2)]
    [Ecopedia("Housing Objects", "Living Room", subPageName: "Cathode Ray Television Item")]
    public class CathodeRayTelevisionRecipe : RecipeFamily
    {
        public CathodeRayTelevisionRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "CathodeRayTelevision",  //noloc
                displayName: Localizer.DoStr("CathodeRayTelevision"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(SteelPlateItem), 8, typeof(MechanicsSkill), typeof(MechanicsLavishResourcesTalent)),
                    new IngredientElement(typeof(BasicCircuitItem), 4, typeof(MechanicsSkill), typeof(MechanicsLavishResourcesTalent)),
                    new IngredientElement(typeof(RadiatorItem), 1, typeof(MechanicsSkill), typeof(MechanicsLavishResourcesTalent)),
                    new IngredientElement(typeof(GlassItem), 4, typeof(MechanicsSkill), typeof(MechanicsLavishResourcesTalent)),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<CathodeRayTelevisionItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 6;

            this.LaborInCalories = CreateLaborInCaloriesValue(160, typeof(MechanicsSkill));

            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(CathodeRayTelevisionRecipe), start: 8, skillType: typeof(MechanicsSkill), typeof(MechanicsFocusedSpeedTalent), typeof(MechanicsParallelSpeedTalent));

            this.Initialize(displayText: Localizer.DoStr("CathodeRayTelevision"), recipeType: typeof(CathodeRayTelevisionRecipe));

            CraftingComponent.AddRecipe(tableType: typeof(AssemblyLineObject), recipeFamily: this);
        }
    }
}
