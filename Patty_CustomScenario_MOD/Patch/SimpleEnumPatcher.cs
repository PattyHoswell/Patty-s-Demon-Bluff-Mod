using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Patty_CustomScenario_MOD.Patch
{
    internal static class SimpleEnumPatcher
    {
        public static Dictionary<Il2CppSystem.Type, List<(string name, object value)>> enumData = new();

        /*
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix, HarmonyPatch(typeof(Enum), nameof(Enum.TryParse), new Type[]
        {
            typeof(Type),
            typeof(string),
            typeof(object)
        })]
        */
        public static bool Enum_TryParse(Type enumType, ReadOnlySpan<char> value, ref object result)
        {
            if (enumType == null)
                return true;

            foreach (var (registeredEnumType, enumValueList) in enumData)
            {
                if (("Il2Cpp." + registeredEnumType.FullName) != enumType.FullName)
                    continue;

                var val = string.Join("", value.ToArray());
                (string enumName, object enumValue) = enumValueList.Find(x => x.name == val);

                if (!string.IsNullOrEmpty(enumName))
                {
                    result = enumValue;
                    return false;
                }

            }
            return true;
        }

        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix, HarmonyPatch(typeof(Enum), nameof(Enum.Parse), new Type[]
        {
            typeof(Type),
            typeof(ReadOnlySpan<char>),
            typeof(bool)
        })]
        public static bool Enum_Parse(Type enumType, ReadOnlySpan<char> value, bool ignoreCase, ref object __result)
        {
            if (enumType == null)
                return true;

            foreach (var (registeredEnumType, enumValueList) in enumData)
            {
                if (registeredEnumType.FullName != enumType.FullName)
                    continue;

                var val = string.Join("", value.ToArray());
                (string name, _) = enumValueList.Find(x => x.name == val);

                if (!string.IsNullOrEmpty(name))
                {
                    __result = name;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Enum), nameof(Enum.GetName), new Type[]
        {
            typeof(Type),
            typeof(object)
        })]
        public static void Enum_GetName(Type enumType, object value, ref string __result)
        {
            if (enumType == null)
                return;

            foreach (var (registeredEnumType, enumValueList) in enumData)
            {
                var comparison = ("Il2Cpp." + registeredEnumType.FullName) != enumType.FullName;
                if (comparison)
                    continue;

                (string enumName, _) = enumValueList.Find(x =>
                {
                    return UniversalUtility.IsNumberEqual(x.value, value);
                });
                if (string.IsNullOrEmpty(enumName))
                    return;
                __result = enumName;
            }
        }

        /* Will break on IL2CPP side
        [HarmonyPostfix, HarmonyPatch(typeof(Enum), nameof(Enum.GetNames), new Type[]
        {
            typeof(Type)
        })]
        public static void Enum_GetNames(Type enumType, ref string[] __result)
        {
            if (enumType == null)
                return;

            foreach (var (registeredEnumType, enumValueList) in enumData)
            {
                if (("Il2Cpp." + registeredEnumType.FullName) != enumType.FullName)
                    continue;

                var result = new List<string>(__result);
                foreach (var (name, _) in enumValueList)
                {
                    result.Add(name);
                }
                __result = result.ToArray();
            }
        }
        */
    }
}
