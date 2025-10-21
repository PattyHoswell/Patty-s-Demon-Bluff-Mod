using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using Patty_CustomStartingHP_MOD;
using System.Collections;
using UnityEngine;


[assembly: MelonInfo(typeof(CustomStartingHP), "Patty_CustomStartingHP_MOD", "1.0.0", "PattyHoswell")]
[assembly: MelonGame("UmiArt", "Demon Bluff")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace Patty_CustomStartingHP_MOD
{
    public class CustomStartingHP : MelonMod
    {
        public const string MAIN_CATEGORY = "CustomStartingHPSettings",
                            STARTING_HP = "StartingHP";

        public MelonPreferences_Category configCategory = null!;
        public override void OnLateInitializeMelon()
        {
            configCategory = MelonPreferences.CreateCategory(MAIN_CATEGORY);
            configCategory.CreateEntry(STARTING_HP, 25, description: "Specify the amount of starting HP");
            configCategory.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, "CustomStartingHP.cfg"));
            configCategory.SaveToFile();
            MelonCoroutines.Start(ApplyStartingHPSettings());
        }

        IEnumerator ApplyStartingHPSettings()
        {
            const int MAX_ITERATION = 1000;

            yield return null;
            yield return null;

            var iteratedCount = 0;
            var playerController = GameObject.FindObjectOfType<PlayerController>(true);
            while (playerController == null && iteratedCount <= MAX_ITERATION)
            {
                yield return null;
                playerController = GameObject.FindObjectOfType<PlayerController>(true);
                iteratedCount++;
            }
            LoggerInstance.Msg($"PlayerController found after {iteratedCount} iterations.");
            if (iteratedCount >= MAX_ITERATION && playerController == null)
            {
                LoggerInstance.Error("Failed to find PlayerController to apply starting HP settings.");
                yield break;
            }
            var currentMaxValue = default(CurrentMaxValue);
            if (playerController != null)
            {
                currentMaxValue = playerController.startingPlayerInfo.health.value.TryCast<CurrentMaxValue>();
            }
            else if (playerController == null)
            {
                LoggerInstance.Error("PlayerController is null. Cannot apply starting HP settings.");
                yield break;
            }
            if (currentMaxValue != null)
            {
                var startingHP = Mathf.Max(1, configCategory.GetEntry<int>(STARTING_HP).Value);
                LoggerInstance.Msg($"Applying starting hp into '{startingHP}'");
                currentMaxValue.max = startingHP;
                currentMaxValue.current = startingHP;
            }
            else
            {
                LoggerInstance.Error("PlayerController startingPlayerInfo.health is not of type CurrentMaxValue. Cannot apply starting HP settings.");
            }
        }
    }
}
