using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;
using BepInEx.Logging;

/*
 * BetterFishing - A mod for Valheim
 * 
 * Copyright (c) 2024 Kam1goroshi
 * Licensed under the MIT License.
 * See the LICENSE file for details in https://github.com/Kam1goroshi/BetterFishing
 */
namespace BetterFishing
{
    [BepInPlugin("kam1goroshi.BetterFishing", "Better Fishing", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class BetterFishing : BaseUnityPlugin
    {
        private static readonly int maxFishLevel = 5; //might be useful in the future
        private static readonly int minFishLevel = 0; //might be useful in the future
        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("BetterFishing");
        private readonly Harmony harmony = new Harmony("kam1goroshi.BetterFishing");
        private static Dictionary<string, float> baitBonusExpMap = new Dictionary<string, float>();
        private static ConfigEntry<float> hookExpMultiplier;
        private static ConfigEntry<float> stepsForCatch;
        private static ConfigEntry<float> bonusPerFishLevel;
        private static ConfigEntry<float> fishingBaitBonus;
        private static ConfigEntry<float> fishingBaitForestBonus;
        private static ConfigEntry<float> fishingBaitSwampBonus;
        private static ConfigEntry<float> fishingBaitCaveBonus;
        private static ConfigEntry<float> fishingBaitPlainsBonus;
        private static ConfigEntry<float> fishingBaitOceanBonus;
        private static ConfigEntry<float> fishingBaitAshlandsBonus;
        private static ConfigEntry<float> fishingBaitDeepNorthBonus;
        private static ConfigEntry<float> fishingBaitMistlandsBonus;
        private static ConfigEntry<int> fishingBoosterStartLevel;
        private static ConfigEntry<float> fishingBoosterMaxChance;
        void Awake()
        {
            hookExpMultiplier = Config.Bind<float>("General", "Hook_Exp_Multiplier", 2.0f, new ConfigDescription("Reeling exp with a hooked fish compared to vanilla empty reel. Vanilla/Default: 2x", new AcceptableValueRange<float>(0.0f, 5.0f)));
            stepsForCatch = Config.Bind<float>("General", "Steps_For_Catch", 10.0f, new ConfigDescription("Catching bonus compared to vanilla empty reel. 0 gives no bonus in any case", new AcceptableValueRange<float>(0.0f, 100.0f)));
            bonusPerFishLevel = Config.Bind<float>("General", "Bonus_Per_Fish_Level", 0.5f, new ConfigDescription("Exp bonus multiplier given for fish level. 0 for no bonus. With 1.0 a 5 star fish will give +500% exp", new AcceptableValueRange<float>(0.0f, 10.0f)));
            fishingBaitBonus = Config.Bind<float>("Fish type bonus", "Simple_Fishing_Bait_bonus", 0.0f, new ConfigDescription("Bonus exp multiplier for using the default bait. 0 for no additional bonus.", new AcceptableValueRange<float>(0.0f, 10.0f)));
            fishingBaitForestBonus = Config.Bind<float>("Fish type bonus", "Mossy_Fishing_Bait_bonus", 0.25f, new ConfigDescription("Bonus exp multiplier for using black forrest bait. 0 for no additional bonus.", new AcceptableValueRange<float>(0.0f, 10.0f)));
            fishingBaitSwampBonus = Config.Bind<float>("Fish type bonus", "Sticky_Fishing_Bait_bonus", 0.4f, new ConfigDescription("Bonus exp multiplier for using swamp bait. 0 for no additional bonus.", new AcceptableValueRange<float>(0.0f, 10.0f)));
            fishingBaitCaveBonus = Config.Bind<float>("Fish type bonus", "Cold_Fishing_Bait_bonus", 0.8f, new ConfigDescription("Bonus exp multiplier for using cave bait. 0 for no additional bonus.", new AcceptableValueRange<float>(0.0f, 10.0f)));
            fishingBaitPlainsBonus = Config.Bind<float>("Fish type bonus", "Stingy_Fishing_Bait_bonus", 0.5f, new ConfigDescription("Bonus exp multiplier for using plains bait. 0 for no additional bonus.", new AcceptableValueRange<float>(0.0f, 10.0f)));
            fishingBaitOceanBonus = Config.Bind<float>("Fish type bonus", "Heavy_Fishing_Bait_bonus", 1.2f, new ConfigDescription("Bonus exp multiplier for using ocean bait. 0 for no additional bonus.", new AcceptableValueRange<float>(0.0f, 10.0f)));
            fishingBaitMistlandsBonus = Config.Bind<float>("Fish type bonus", "Misty_Fishing_Bait_bonus", 0.75f, new ConfigDescription("Bonus exp multiplier for using mistlands bait. 0 for no additional bonus.", new AcceptableValueRange<float>(0.0f, 10.0f)));
            fishingBaitAshlandsBonus = Config.Bind<float>("Fish type bonus", "Hot_Fishing_Bait_bonus", 1.0f, new ConfigDescription("Bonus exp multiplier for using ashlands bait. 0 for no additional bonus.", new AcceptableValueRange<float>(0.0f, 10.0f)));
            fishingBaitDeepNorthBonus = Config.Bind<float>("Fish type bonus", "Frosty_Fishing_Bait_bonus", 1.3f, new ConfigDescription("Bonus exp multiplier for using deep north bait. 0 for no additional bonus.", new AcceptableValueRange<float>(0.0f, 10.0f)));
            fishingBoosterStartLevel = Config.Bind<int>("Fish Level Booster", "Boosting_Starting_Level", 20, new ConfigDescription("At what level you can increase the level of fish by hooking them", new AcceptableValueRange<int>(0, 100)));
            fishingBoosterMaxChance = Config.Bind<float>("Fish Level Booster", "Max_Boosting_Chance", 1.0f, new ConfigDescription("What is the max chance? You chances increase linearly as you level until max. 0 to turn off", new AcceptableValueRange<float>(0.0f, 1.0f)));
            baitBonusExpMap.Add("FishingBait", fishingBaitBonus.Value);
            baitBonusExpMap.Add("FishingBaitForest", fishingBaitForestBonus.Value);
            baitBonusExpMap.Add("FishingBaitSwamp", fishingBaitSwampBonus.Value);
            baitBonusExpMap.Add("FishingBaitCave", fishingBaitCaveBonus.Value);
            baitBonusExpMap.Add("FishingBaitPlains", fishingBaitPlainsBonus.Value);
            baitBonusExpMap.Add("FishingBaitOcean", fishingBaitOceanBonus.Value);
            baitBonusExpMap.Add("FishingBaitMistlands", fishingBaitMistlandsBonus.Value);
            baitBonusExpMap.Add("FishingBaitAshlands", fishingBaitAshlandsBonus.Value);
            baitBonusExpMap.Add("FishingBaitDeepNorth", fishingBaitDeepNorthBonus.Value);

            foreach(var bait in baitBonusExpMap)
            {
                logger.LogDebug($"Added <key:{bait.Key},value:{bait.Value} in baitBonusExpMap");
            }
            harmony.PatchAll();
        }

        void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        /**
         *  @param quality is item stars + 1 (item.quality in dissasembly)
         *  @param baitPrefabName the name of the bait in order to find its bonus from the map 
         */
        private static float getExpGainOnCatch(float quality, string baitPrefabName)
        {
            float accumulator = stepsForCatch.Value;
            accumulator *= (1f + (bonusPerFishLevel.Value * (quality - 1f)));
            if(baitBonusExpMap.TryGetValue(baitPrefabName, out float baitBonus))
            {
                accumulator *= (baitBonus + 1.0f);
                logger.LogMessage($"Skill raised by {accumulator}");
                return accumulator;

            }
            else
            {
                logger.LogError("Key wasn't found in bait bonus map.");
                return accumulator;
            }
        }

        /**
         *  Patch that initializes things in Fishing Awake 
         */
        [HarmonyPatch(typeof(FishingFloat), "Awake")]
        class FishingFixedUpdatePatch
        {
            /*
             * When fishingFloat is initialized change the default multiplier
             */
            static void Postfix(ref float ___m_fishingSkillImproveHookedMultiplier)
            {
                ___m_fishingSkillImproveHookedMultiplier = hookExpMultiplier.Value;
            }
        }

       /**
        * 
       */
        [HarmonyPatch(typeof(Fish), nameof(Fish.OnHooked))]
        class fishLevelBoostPatch
        {
            static void Postfix(Fish __instance, FishingFloat ff)
            {
                if (ff != null && __instance.m_itemDrop != null && __instance.m_itemDrop.m_itemData != null)
                {
                    float fishingLevel = Player.m_localPlayer.GetSkillLevel(Skills.SkillType.Fishing);
                    //Check that the fish is not boosted and that fishing skill is strong enough to boost fish levels
                    if (fishingLevel >= fishingBoosterStartLevel.Value &&
                            __instance.m_itemDrop.m_itemData.m_customData.ContainsKey("BoostedByFishingLevel") == false)
                    {
                        //Calculate chance by projecting skill leveling progress on max chance
                        float progress = fishingLevel / 100.0f; //max level is 100 so no need to clamp for now
                        float successRate = fishingBoosterMaxChance.Value * progress;
                        
                        //Run RNG, on success attempt to level up fish
                        int counter = 0; //using counter due to multiple rolls consideration in the future
                        float rng = UnityEngine.Random.Range(0.0f, 1.0f);
                        logger.LogMessage($"Boosting success rate: {successRate:F2}% and rolled {rng:F2}%");
                        if (successRate > rng)
                        {
                            //run rng before this
                            __instance.m_itemDrop.SetQuality(Mathf.Clamp(__instance.m_itemDrop.m_itemData.m_quality + 1, minFishLevel, maxFishLevel));
                            __instance.m_itemDrop.Save();
                            counter++;
                        }
                        if (counter > 0)
                        {
                            ff.GetOwner().Message(MessageHud.MessageType.Center, $"Fish Level raised by {counter}!");
                            __instance.m_itemDrop.m_itemData.m_customData.Add("BoostedByFishingLevel", $"{counter}");
                        }
                    }
                }
            }
        }

        /**
        * @dev This runs when the fishing line is < 0.5 and the fish is caught
        * 
        * Note after reading dissasembly a bit: If there is no m_pickupitem, the m_pickupitem becomes the fish itself. Otherwise it's unknown.
        */
        [HarmonyPatch(typeof(FishingFloat), nameof(FishingFloat.Catch))]
        class FishCatchPatch
        {
            static void Prefix(Fish fish, Character owner)
            {
                if (fish != null)
                {
                    ItemDrop itemDrop = fish.gameObject.GetComponent<ItemDrop>() ;
                    
                    #if DEBUG
                    logger.LogDebug("ItemDrop: " + (itemDrop != null ? itemDrop.name : "---"));
                    logger.LogDebug("Pickup item: " + (fish.m_pickupItem != null ? fish.m_pickupItem.name : "---"));
                    #endif
                    string baitPrefabName = Player.m_localPlayer.GetAmmoItem().m_dropPrefab.name;
                    float exp = getExpGainOnCatch(itemDrop.m_itemData.m_quality, baitPrefabName);
                    Player.m_localPlayer.RaiseSkill(Skills.SkillType.Fishing, exp);
                }
                else
                {
                    logger.LogError("null fish in FishCatchPatch");
                }
            }
        }

        /**
         * @dev This runs when you press *E* to pickup a fish. Devs have a different logic for catching a fish because the line is < 0.5 \n
         */
        [HarmonyPatch(typeof(Fish), nameof(Fish.Pickup))]
        class FishPickupPatch
        {
            static void Prefix(Fish __instance)
            {
                ItemDrop itemDrop = __instance.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    if (__instance.IsHooked()) //Interract with fish by pressing "E", check that it's actually on rod and not from floor.
                    {
                        #if DEBUG
                        logger.LogDebug("ItemDrop: " + (itemDrop != null ? itemDrop.name : "---"));
                        logger.LogDebug("Pickup item: " + (__instance.m_pickupItem != null ? __instance.m_pickupItem.name : "---"));
                        #endif
                        string baitPrefabName = Player.m_localPlayer.GetAmmoItem().m_dropPrefab.name;
                        float exp = getExpGainOnCatch(itemDrop.m_itemData.m_quality, baitPrefabName);
                        Player.m_localPlayer.RaiseSkill(Skills.SkillType.Fishing, exp);
                    }
                }
                else
                    logger.LogError("null fish in FishPickupPatch");
            }
        }
    }
}
