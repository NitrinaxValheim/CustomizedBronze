// System
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

// UnityEngine
using UnityEngine;

// BepInEx
using BepInEx;
using BepInEx.Configuration;

// Jotunn
using Jotunn.Managers;
using Jotunn.Utils;
using Jotunn.Entities;
using Logger = Jotunn.Logger;

using Common;
using Plugin;

namespace CustomizedBronze
{
    // setup plugin data
    [BepInPlugin(Data.ModGuid, Data.ModName, Data.Version)]

    // check for running valheim process
    [BepInProcess("valheim.exe")]

    // check for jotunn dependency
    [BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]

    // check compatibility level
    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Patch)]
    internal class CustomizedBronze : BaseUnityPlugin
    {
        // Config strings
        private const string ConfigCategoryRecipeAlloy = "Recipe Alloy";
        private const string ConfigCategoryRecipeCustom = "Recipe Custom";

        private static readonly string ConfigEntryAttention =
            "Please note: Changes to the recipe are applied immediately (no relog required).";

        private static readonly string ConfigEntryRecipeAlloyDescription =
            $"Alloy composition:{Environment.NewLine}" +
            $"({ConfigEntryAttention}){Environment.NewLine}{Environment.NewLine}" +
            "Default = the standard alloy from Valheim (2 Copper + 1 Tin = 1 Bronze)" +
            "[If you select this preset, your custom settings will be ignored.]" +
            $"{Environment.NewLine}{Environment.NewLine}" +
            "WoWlike = an alloy like in World of Warcraft (1 Copper + 1 Tin = 2 Bronze)" +
            "[If you select this preset, your custom settings will be ignored.]" +
            $"{Environment.NewLine}{Environment.NewLine}" +
            "Realistic = a more realistic alloy (2 Copper + 1 Tin = 3 Bronze)" +
            "[If you select this preset, your custom settings will be ignored.]" +
            $"{Environment.NewLine}{Environment.NewLine}" +
            "Custom = a custom alloy with the mixing ratio you set" +
            "[If you select this, your custom settings will be used.]";

        private static readonly string ConfigEntryRecipeCustomCopperDescription =
            "Sets amount of needed Copper.";

        private static readonly string ConfigEntryRecipeCustomTinDescription =
            "Sets amount of needed Tin.";

        private static readonly string ConfigEntryRecipeCustomBronzeDescription =
            "Sets the amount of Bronze produced.";

        // Presets
        private static readonly int[] DefaultAlloy = { 2, 1, 1 };
        private static readonly int[] WoWlikeAlloy = { 1, 1, 2 };
        private static readonly int[] RealisticAlloy = { 2, 1, 3 };

        // Configuration entries
        public static ConfigEntry<bool> configModEnabled;
        public static ConfigEntry<int> configNexusID;
        public static ConfigEntry<bool> configShowChangesAtStartup;
        public static ConfigEntry<BronzeAlloy> configBronzeAlloy;
        public static ConfigEntry<int> configRecipeNeededCopper;
        public static ConfigEntry<int> configRecipeNeededTin;
        public static ConfigEntry<int> configRecipeProducedBronze;

        private int usedRequirementCopper;
        private int usedRequirementTin;
        private int usedQuantityBronze;

        // Prefab cache (optional performance improvement)
        private ItemDrop copperPrefab;
        private ItemDrop tinPrefab;

        // Store original recipe data for reset
        private Dictionary<string, (Piece.Requirement[] resources, int amount)> originalRecipes
            = new Dictionary<string, (Piece.Requirement[], int)>();

        // Flag to control startup logging
        private static bool firstUpdate = true;
        private static bool showChanges = true;

        private List<Recipe> bronzeRecipes = new List<Recipe>();

        #region[Unity Lifecycle]
        private void Awake()
        {
            if (DependencyOperations.CheckForDependencyErrors(Data.ModGuid))
                return;

            CreateConfigValues();
            SubscribeToConfigChanges();

            ItemManager.OnItemsRegistered += OnItemsRegistered;

            if (!configModEnabled.Value)
            {
                Logger.LogWarning($"{Data.Company} {Data.ModName} v{Data.Version} is loaded but disabled by config.");
            }
            else
            {
                Logger.LogInfo($"{Data.Company} {Data.ModName} v{Data.Version} loaded.");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe events to prevent memory leaks
            configModEnabled.SettingChanged -= OnConfigChanged;
            configBronzeAlloy.SettingChanged -= OnConfigChanged;
            configRecipeNeededCopper.SettingChanged -= OnConfigChanged;
            configRecipeNeededTin.SettingChanged -= OnConfigChanged;
            configRecipeProducedBronze.SettingChanged -= OnConfigChanged;
            ItemManager.OnItemsRegistered -= OnItemsRegistered;
            SynchronizationManager.OnConfigurationSynchronized -= OnConfigurationSynchronized;
        }
        #endregion

        #region[Event Handlers]
        private void OnItemsRegistered()
        {
            try
            {

                InitializeRecipeCache();

                if (configModEnabled.Value)
                    UpdateBronzeRecipe();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in OnItemsRegistered: {ex.Message}");
#if DEBUG
                Logger.LogError($"StackTrace: {ex.StackTrace}");
#endif
            }
            finally
            {
                // Unsubscribe after first execution (only needed once)
                ItemManager.OnItemsRegistered -= OnItemsRegistered;
            }
        }

        // Named event handler for config sync
        private void OnConfigurationSynchronized(object sender, ConfigurationSynchronizationEventArgs args)
        {
            Logger.LogMessage(args.InitialSynchronization
                ? "Initial Config sync event received"
                : "Config sync event received");
        }

        private void OnConfigChanged(object sender, EventArgs e)
        {

            // Mod disabled → restore original recipes
            if (!configModEnabled.Value)
            {
                
                RestoreOriginalRecipes();
                return;
            }

            // If the mod has never been initialized (bronzeRecipes is empty),
            // we need to do so now, since the user has activated the mod.
            if (bronzeRecipes.Count == 0)
            {
                InitializeRecipeCache();
            }            

            // Mod enabled → apply current settings
            UpdateBronzeRecipe();
        }

        private void InitializeRecipeCache()
        {
            // Extract the initialization logic into a separate method
            if (ObjectDB.instance != null && ObjectDB.instance.m_recipes != null)
            {
                bronzeRecipes = ObjectDB.instance.m_recipes
                    .Where(r => r.m_item?.name == "Bronze")
                    .ToList();

                foreach (var recipe in bronzeRecipes)
                {
                    SaveOriginalRecipe(recipe);
                }
            }
        }

        #endregion

        #region[Configuration]
        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;

            configModEnabled = Config.Bind(
                Data.ConfigCategoryGeneral,
                Data.ConfigEntryEnabled,
                Data.ConfigEntryEnabledDefaultState,
                new ConfigDescription(Data.ConfigEntryEnabledDescription,
                    null,
                    new ConfigurationManagerAttributes { Order = 0 }));

            configNexusID = Config.Bind(
                Data.ConfigCategoryGeneral,
                Data.ConfigEntryNexusID,
                Data.ConfigEntryNexusIDID,
                new ConfigDescription(Data.ConfigEntryNexusIDDescription,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        Browsable = false,
                        Order = 1,
                        ReadOnly = true
                    }));

            configShowChangesAtStartup = Config.Bind(
                Data.ConfigCategoryPlugin,
                Data.ConfigEntryShowChangesAtStartup,
                Data.ConfigEntryShowChangesAtStartupDefaultState,
                new ConfigDescription(Data.ConfigEntryShowChangesAtStartupDescription,
                    null,
                    new ConfigurationManagerAttributes { Order = 0 }));

            configBronzeAlloy = Config.Bind(
                ConfigCategoryRecipeAlloy,
                "Alloy Type",
                BronzeAlloy.Default,
                new ConfigDescription(ConfigEntryRecipeAlloyDescription,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        DefaultValue = BronzeAlloy.Default,
                        Order = 1,
                        DispName = $"Alloy Type{Environment.NewLine}{Environment.NewLine}{ConfigEntryAttention}"
                    }));

            configRecipeNeededCopper = Config.Bind(
                ConfigCategoryRecipeCustom,
                "Required Copper",
                DefaultAlloy[0],
                new ConfigDescription(ConfigEntryRecipeCustomCopperDescription,
                    null,
                    new ConfigurationManagerAttributes { Order = 0 }));

            configRecipeNeededTin = Config.Bind(
                ConfigCategoryRecipeCustom,
                "Required Tin",
                DefaultAlloy[1],
                new ConfigDescription(ConfigEntryRecipeCustomTinDescription,
                    null,
                    new ConfigurationManagerAttributes { Order = 1 }));

            configRecipeProducedBronze = Config.Bind(
                ConfigCategoryRecipeCustom,
                "Produced Bronze",
                DefaultAlloy[2],
                new ConfigDescription(ConfigEntryRecipeCustomBronzeDescription,
                    null,
                    new ConfigurationManagerAttributes { Order = 2 }));

            // Use a named method instead of anonymous handler
            SynchronizationManager.OnConfigurationSynchronized += OnConfigurationSynchronized;
        }
        private void SubscribeToConfigChanges()
        {
            configModEnabled.SettingChanged += OnConfigChanged;
            configBronzeAlloy.SettingChanged += OnConfigChanged;
            configRecipeNeededCopper.SettingChanged += OnConfigChanged;
            configRecipeNeededTin.SettingChanged += OnConfigChanged;
            configRecipeProducedBronze.SettingChanged += OnConfigChanged;
        }

        private void ReadConfigValues()
        {
            showChanges = configShowChangesAtStartup.Value && firstUpdate;
            if (showChanges) firstUpdate = false; // only log once per session

            BronzeAlloy preset = configBronzeAlloy.Value;

            switch (preset)
            {
                case BronzeAlloy.Default:
                    if (showChanges) Logger.LogInfo("Default option selected");
                        usedRequirementCopper = DefaultAlloy[0];
                        usedRequirementTin = DefaultAlloy[1];
                        usedQuantityBronze = DefaultAlloy[2];
                    break;

                case BronzeAlloy.WoWlike:
                    if (showChanges) Logger.LogInfo("WoWlike option selected");
                        usedRequirementCopper = WoWlikeAlloy[0];
                        usedRequirementTin = WoWlikeAlloy[1];
                        usedQuantityBronze = WoWlikeAlloy[2];
                    break;

                case BronzeAlloy.Realistic:
                    if (showChanges) Logger.LogInfo("Realistic option selected");
                        usedRequirementCopper = RealisticAlloy[0];
                        usedRequirementTin = RealisticAlloy[1];
                        usedQuantityBronze = RealisticAlloy[2];
                    break;

                case BronzeAlloy.Custom:
                    if (showChanges) Logger.LogInfo("Custom option selected");
                        // Validate custom values to avoid zero or negative amounts
                        usedRequirementCopper = Math.Max(1, configRecipeNeededCopper.Value);
                        usedRequirementTin = Math.Max(1, configRecipeNeededTin.Value);
                        usedQuantityBronze = Math.Max(1, configRecipeProducedBronze.Value);
                    break;

                default:
                    if (showChanges) Logger.LogWarning($"Unknown alloy preset '{preset}', falling back to Default.");
                        usedRequirementCopper = DefaultAlloy[0];
                        usedRequirementTin = DefaultAlloy[1];
                        usedQuantityBronze = DefaultAlloy[2];
                    break;
            }

#if DEBUG
            Logger.LogInfo(
                $"usedRequirementCopper = {usedRequirementCopper}, " +
                $"usedRequirementTin = {usedRequirementTin}, " +
                $"usedQuantityBronze = {usedQuantityBronze}");
#endif
        }

        #endregion

        #region[Recipe Update & Reset]
        private void UpdateBronzeRecipe()
        {
            ReadConfigValues();
            ChangeBronzeRecipe();
        }

        private void ChangeBronzeRecipe()
        {
            if (ObjectDB.instance == null || ObjectDB.instance.m_recipes == null)
            {
                Logger.LogWarning("ObjectDB not ready, skipping recipe update.");
                return;
            }

            // Cache prefabs once
            if (copperPrefab == null)
                copperPrefab = PrefabManager.Cache.GetPrefab<ItemDrop>("Copper");
            if (tinPrefab == null)
                tinPrefab = PrefabManager.Cache.GetPrefab<ItemDrop>("Tin");

            if (copperPrefab == null || tinPrefab == null)
            {
                Logger.LogError("Failed to get Copper or Tin prefab.");
                return;
            }

            if (bronzeRecipes.Count > 0)
            {
                foreach (Recipe recipe in bronzeRecipes)
                {
                    try
                    {
                        int multiplier = recipe.name == "Recipe_Bronze" ? 1 : 5;
                        int copperReq = usedRequirementCopper * multiplier;
                        int tinReq = usedRequirementTin * multiplier;
                        int bronzeQty = usedQuantityBronze * multiplier;

                        recipe.m_resources = new[]
                        {
                        new Piece.Requirement { m_resItem = copperPrefab, m_amount = copperReq },
                        new Piece.Requirement { m_resItem = tinPrefab, m_amount = tinReq }
                    };
                        recipe.m_amount = bronzeQty;

                        if (showChanges)
                        {
                            Logger.LogInfo(
                                $"Changed {recipe.name}: Copper = {copperReq}, Tin = {tinReq}, Bronze = {bronzeQty}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error updating recipe {recipe.name}: {ex.Message}");
                    }
                }
            }
            else
            {
                Logger.LogWarning($"Could not find any Bronze recipes.");
            }
        }

        // Save the original recipe data once (before any changes)
        private void SaveOriginalRecipe(Recipe recipe)
        {
            if (recipe == null || string.IsNullOrEmpty(recipe.name))
                return;

            if (!originalRecipes.ContainsKey(recipe.name))
            {
                // Deep copy of the requirements array (Piece.Requirement is a class)
                var reqCopy = new Piece.Requirement[recipe.m_resources?.Length ?? 0];
                if (recipe.m_resources != null)
                {
                    for (int i = 0; i < recipe.m_resources.Length; i++)
                    {
                        var orig = recipe.m_resources[i];
                        reqCopy[i] = new Piece.Requirement
                        {
                            m_resItem = orig.m_resItem,
                            m_amount = orig.m_amount,
                        };
                    }
                }
                originalRecipes[recipe.name] = (reqCopy, recipe.m_amount);
            }
        }

        // Restore all Bronze recipes to their original state
        private void RestoreOriginalRecipes()
        {
            if (ObjectDB.instance == null || ObjectDB.instance.m_recipes == null)
                return;

            foreach (Recipe recipe in bronzeRecipes)
            {
                if (originalRecipes.TryGetValue(recipe.name, out var original))
                {
                    recipe.m_resources = original.resources;
                    recipe.m_amount = original.amount;
                    Logger.LogInfo($"Restored original recipe: {recipe.name}");
                }
            }
        }

        #endregion

        #region[Enums]
        public enum BronzeAlloy
        {
            [Description("Default")]
            Default = 0,
            [Description("WoWlike")]
            WoWlike,
            [Description("Realistic")]
            Realistic,
            [Description("Custom")]
            Custom
        }
        #endregion
    }
}