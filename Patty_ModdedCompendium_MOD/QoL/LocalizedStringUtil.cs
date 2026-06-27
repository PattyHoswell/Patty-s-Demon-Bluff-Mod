using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Patty_ModdedCompendium_MOD.QoL
{
    internal static class LocalizedStringUtil
    {
        public static LocalizedString AddString(string key, string value, string tableName = "Achievements")
        {
            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                LocalizationSettings.InitializationOperation.WaitForCompletion();
            }

            var currentLocale = LocalizationSettings.SelectedLocale;
            var tableOperation = LocalizationSettings.StringDatabase.GetTableAsync(tableName, currentLocale);
            var stringTable = tableOperation.WaitForCompletion();

            if (stringTable != null)
            {
                if (stringTable.GetEntry(key) == null)
                {
                    stringTable.AddEntry(key, value);
                }
                return new LocalizedString(tableName, key);
            }
            else
            {
                ModdedCompendium.Logger.BigError($"Failed to load table: {tableName}");
                return null;
            }
        }
        public static LocalizedString ChangeString(string key, string value, string tableName = "Achievements")
        {
            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                LocalizationSettings.InitializationOperation.WaitForCompletion();
            }

            var currentLocale = LocalizationSettings.SelectedLocale;
            var tableOperation = LocalizationSettings.StringDatabase.GetTableAsync(tableName, currentLocale);
            var stringTable = tableOperation.WaitForCompletion();

            if (stringTable != null)
            {
                if (stringTable.GetEntry(key) == null)
                {
                    stringTable.AddEntry(key, value);
                }
                stringTable.GetEntry(key).Value = value;
                return new LocalizedString(tableName, key);
            }
            else
            {
                ModdedCompendium.Logger.BigError($"Failed to load table: {tableName}");
                return null;
            }
        }
    }
}
