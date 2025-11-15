using Il2Cpp;
using Patty_CustomRole_MOD.Json;
using UnityEngine;

namespace Patty_CustomRole_MOD.QoL
{
    public static class ModdedAchievementCompendium
    {
        public readonly static HashSet<AchievementData> moddedAchievements = new HashSet<AchievementData>();
        public readonly static Dictionary<FileInfo, AchievementData_Json> allAchievementDataJson = new();
        public readonly static Dictionary<AchievementData, AchievementData_Json> dataToJson = new();

        internal static void CreateNewPage(Achievements achievementCompendium, List<AchievementData> achievementDatas)
        {
            if (achievementDatas.Count <= 0)
            {
                return;
            }
            var achievementPages = achievementCompendium.pages;
            var pagesCount = achievementPages.Count();
            var page = achievementPages.FirstOrDefault(x => x.achivsData.Length < achievementCompendium.achivs.Length) ?? new AchievementCompendiumPage();
            if (page.achivsData == null)
            {
                page.achivsData = new AchievementData[0];
            }
            var availableSpace = achievementCompendium.achivs.Length - page.achivsData.Length;
            var sliceCount = Mathf.Min(availableSpace, achievementDatas.Count);
            var slicedList = achievementDatas.GetRange(0, Mathf.Max(sliceCount, 0));
            achievementDatas.RemoveRange(0, slicedList.Count);
            page.achivsData = page.achivsData.Union(slicedList).ToArray();

            if (!achievementCompendium.pages.Contains(page))
            {
                achievementCompendium.pages = achievementCompendium.pages.Append(page).ToArray();
                page.pageName = "Achievement";
                if (pagesCount >= 1)
                {
                    page.pageName += $" {pagesCount}/?";
                }
            }

            if (achievementDatas.Count > 0)
            {
                CreateNewPage(achievementCompendium, achievementDatas);
            }
        }

    }
}
