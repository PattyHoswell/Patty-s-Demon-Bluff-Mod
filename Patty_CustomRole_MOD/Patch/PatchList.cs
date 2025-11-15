using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Patty_CustomRole_MOD.QoL;
using UnityEngine;

namespace Patty_CustomRole_MOD.Patch
{
    internal static class PatchList
    {
        public static Lazy<Il2CppArrayBase<AchievementData>> allAchievementData = new Lazy<Il2CppArrayBase<AchievementData>>(() => Resources.FindObjectsOfTypeAll<AchievementData>(), isThreadSafe: false);


        [HarmonyPrefix, HarmonyPatch(typeof(Achievements), nameof(Achievements.InitPage))]
        public static void InitPage(Achievements __instance)
        {
            if (__instance.unlockedAchieves == null)
            {
                __instance.unlockedAchieves = new Il2CppSystem.Collections.Generic.List<AchievementData>();
            }
            List<AchievementData> newAchievement = new List<AchievementData>();
            foreach (var achievementData in allAchievementData.Value)
            {
                if (achievementData == null)
                {
                    continue;
                }
                if (!ModdedAchievementCompendium.dataToJson.ContainsKey(achievementData))
                {
                    continue;
                }
                if (ModdedAchievementCompendium.moddedAchievements.Contains(achievementData))
                {
                    continue;
                }
                ModdedAchievementCompendium.moddedAchievements.Add(achievementData);
                newAchievement.Add(achievementData);
            }
            foreach (var achievement in ModdedAchievementCompendium.moddedAchievements)
            {
                var isUnlockedInSave = SavesGame.UnlockedAchievements.ids.Contains(achievement.id);
                var isAutoUnlocked = ModdedAchievementCompendium.dataToJson.TryGetValue(achievement, out var achievementData) && achievementData.AutoUnlocked;
                if (!isUnlockedInSave && !isAutoUnlocked)
                {
                    newAchievement.Remove(achievement);
                    continue;
                }
                if (!isUnlockedInSave && isAutoUnlocked)
                {
                    SavesGame.UnlockedSkins.ids.Add(achievement.id);
                }
                if (!__instance.unlockedAchieves.Contains(achievement))
                    __instance.unlockedAchieves.Add(achievement);
            }
            ModdedAchievementCompendium.CreateNewPage(__instance, newAchievement);
            __instance.UpdateCompletion();
        }
    }
}
