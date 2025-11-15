using System;
using UnityEngine;

namespace Patty_TransferableSaves_MOD
{
    public class PlayerPrefs_Data
    {
        public PlayerPrefs_Data(string key, object value, Type dataType)
        {
            this.key = key;
            this.value = value;
            this.dataType = dataType;
        }

        public string key;
        public object value;
        public Type dataType;

        public void SetValue()
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                TransferableSaves.Logger.Warning("Unable to set PlayerPrefs value because the key is null");
                return;
            }
            try
            {
                switch (dataType)
                {
                    case Type.Int:
                        PlayerPrefs.SetInt(key, Convert.ToInt32(value));
                        break;
                    case Type.Float:
                        PlayerPrefs.SetFloat(key, Convert.ToSingle(value));
                        break;
                    case Type.String:
                        PlayerPrefs.SetString(key, Convert.ToString(value));
                        break;
                    default:
                        TransferableSaves.Logger.Error("Unknown type to save into player pref");
                        break;
                }
            }
            catch (InvalidCastException ex)
            {
                TransferableSaves.Logger.Error($"Unable to set value on key {key}");
                TransferableSaves.Logger.Error((ex.InnerException ?? ex).Message);
            }
        }

        public object GetPlayerPrefsValue()
        {
            switch (dataType)
            {
                case Type.Int:
                    return PlayerPrefs.GetInt(key);
                case Type.Float:
                    return PlayerPrefs.GetFloat(key);
                case Type.String:
                    return PlayerPrefs.GetString(key);
                default:
                    TransferableSaves.Logger.Error("Unknown type to get from player pref");
                    return null!;
            }
        }

        public enum Type
        {
            Unknown,
            Int,
            Float,
            String
        }
    }
}
