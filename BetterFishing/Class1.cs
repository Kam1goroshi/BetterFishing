using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;

/*
 * BetterFishing - A mod for Valheim
 * 
 * Copyright (c) 2024 Kam1goroshi
 * Licensed under the MIT License.
 * See the LICENSE file for details in https://github.com/Kam1goroshi/BetterFishing
 */
namespace BetterFishing
{
    [BepInPlugin("kam1goroshi.BetterFishing", "Better Fishing", "0.2.3")]
    [BepInProcess("valheim.exe")]
    public class BetterFishing : BaseUnityPlugin
    {
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
            baitBonusExpMap.Add("FishingBait", fishingBaitBonus.Value);
            baitBonusExpMap.Add("FishingBaitForest", fishingBaitForestBonus.Value);
            baitBonusExpMap.Add("FishingBaitSwamp", fishingBaitSwampBonus.Value);
            baitBonusExpMap.Add("FishingBaitCave", fishingBaitCaveBonus.Value);
            baitBonusExpMap.Add("FishingBaitPlains", fishingBaitPlainsBonus.Value);
            baitBonusExpMap.Add("FishingBaitOcean", fishingBaitOceanBonus.Value);
            baitBonusExpMap.Add("FishingBaitMistlands", fishingBaitMistlandsBonus.Value);
            baitBonusExpMap.Add("FishingBaitAshlands", fishingBaitAshlandsBonus.Value);
            baitBonusExpMap.Add("FishingBaitDeepNorth", fishingBaitDeepNorthBonus.Value);

            #if DEBUG
            foreach(var bait in baitBonusExpMap)
            {
                Debug.Log($"Added <key:{bait.Key},value:{bait.Value} in baitBonusExpMap");
            }
            #endif
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
                #if DEBUG
                Debug.Log($"Calculated exp gain: {accumulator}");
                #endif
                return accumulator;

            }
            else
            {
                Debug.LogError("Key wasn't found in bait bonus map.");
                return accumulator;
            }
        }

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
        * @dev This runs when the fishing line is < 0.5 and the fish is caught
        */
        [HarmonyPatch(typeof(FishingFloat), nameof(FishingFloat.Catch))]
        class FishCatchPatch
        {
            static void Prefix(Fish fish, Character owner)
            {
                //Next 2 lines follow the same logic asa the original version just in case.
                ItemDrop itemDrop = fish ? fish.gameObject.GetComponent<ItemDrop>() : null;
                if ((bool)fish)
                {
                    string baitPrefabName = Player.m_localPlayer.GetAmmoItem().m_dropPrefab.name;
                    float exp = getExpGainOnCatch(itemDrop.m_itemData.m_quality, baitPrefabName);
                    Player.m_localPlayer.RaiseSkill(Skills.SkillType.Fishing, exp);
                }
                else
                {
                    Debug.LogError("null fish in FishCatchPatch");
                }
            }
        }

        /**
         * @dev This runs when you press *E* to pickup a fish. Devs have a different logic for catching a fish because the line is < 0.5 \n
         * Could even be bad approach since it probably can but be used from catch() so duplicate exp call. Unsure about this from dissasemblying
         */
        [HarmonyPatch(typeof(Fish), nameof(Fish.Pickup))]
        class FishPickupPatch
        {
            static void Prefix(Fish __instance)
            {
                ItemDrop item = __instance.GetComponent<ItemDrop>();
                if (item != null)
                {
                    if (__instance.IsHooked()) //Interract with fish by pressing "E", check that it's actually on rod and not from floor.
                    {
                        string baitPrefabName = Player.m_localPlayer.GetAmmoItem().m_dropPrefab.name;
                        float exp = getExpGainOnCatch(item.m_itemData.m_quality, baitPrefabName);
                        Player.m_localPlayer.RaiseSkill(Skills.SkillType.Fishing, exp);
                    }
                }
                else
                    Debug.LogError("null fish in FishPickupPatch");
            }
        }
    }
}
