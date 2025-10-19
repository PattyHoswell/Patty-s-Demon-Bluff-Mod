using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace Patty_CustomAudio_MOD
{
    internal static class PatchList
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SfxController), nameof(SfxController.PlaySfx))]
        [HarmonyPatch(typeof(SfxController), nameof(SfxController.PlayOneShotSfx))]
        internal static void PlayClip(ref AudioClip clip)
        {
            if (Utility.TryGetLoadedClip(clip.name, out AudioClip result))
            {
                clip = result;
            }
        }
    }
}
