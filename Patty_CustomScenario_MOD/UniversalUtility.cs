using HarmonyLib;
using Il2Cpp;
using Il2CppDG.Tweening;
using Il2CppDG.Tweening.Core;
using Il2CppDG.Tweening.Plugins.Options;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using Patty_CustomScenario_MOD.Patch;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Patty_CustomScenario_MOD
{
    public static class UniversalUtility
    {
        public readonly static Lazy<JsonSerializerOptions> UnsafeEnumSerializerOptions = new(() => new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        }, isThreadSafe: false);

        public static bool LoadJson<T>(string path, out T result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                result = default!;
                CustomScenario.Logger.BigError($"Cannot load file {path}, reason: {(ex.InnerException ?? ex).Message}");
            }
            return result != null;
        }

        public static bool SerializeJson<T>(string path, T value)
        {
            var result = false;
            try
            {
                var json = JsonSerializer.Serialize(value, UniversalUtility.UnsafeEnumSerializerOptions.Value);
                File.WriteAllText(path, json);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                CustomScenario.Logger.BigError($"Cannot serialize file {path}, reason: {(ex.InnerException ?? ex).Message}");
            }
            return result == true;
        }

        public static void ExtractAscension(AscensionsData data, bool overrides = false)
        {
            DirectoryInfo scenarioFolder = Directory.CreateDirectory(CustomScenario.AscensionDataFolder);
            DirectoryInfo customScriptsFolder = Directory.CreateDirectory(CustomScenario.CustomScriptDataFolder);

            var scenarioData = new AscensionsData_Json();
            scenarioData.Initialize(data);

            scenarioData.Serialize(scenarioFolder.FullName, overrides);
            scenarioData.SerializeCustomScriptData(customScriptsFolder.FullName, overrides);
        }

        public static TweenerCore<Color, Color, ColorOptions> DOColor(this Image targetImage, Color targetColor, float time)
        {
            DOGetter<Color> getter = (DOGetter<Color>)(() => targetImage.color);
            DOSetter<Color> setter = (DOSetter<Color>)((Color newColor) => targetImage.color = newColor);
            return DOTween.To(getter, setter, targetColor, time);
        }
        public static TweenerCore<float, float, FloatOptions> DOAlpha(this CanvasGroup targetCanvas, float targetAlpha, float time)
        {
            DOGetter<float> getter = (DOGetter<float>)(() => targetCanvas.alpha);
            DOSetter<float> setter = (DOSetter<float>)((float newAlpha) => targetCanvas.alpha = newAlpha);
            return DOTween.To(getter, setter, targetAlpha, time);
        }

        public static Type FindType(string typeName)
        {
            var result = typeof(CharacterData).Assembly.GetType($"Il2Cpp.{typeName}");
            if (result != null)
            {
                return result;
            }
            foreach (MelonBase melon in MelonBase.RegisteredMelons)
            {
                result = melon.MelonAssembly.Assembly.GetType(typeName);
                if (result != null)
                {
                    return result;
                }
            }
            return null!;
        }

        public static void AddEnum<T>(string enumName, object value) where T : System.Enum
        {
            if (!AccessTools.IsValue(value.GetType()))
            {
                MelonLogger.Error($"Value must be a number, applied type {value.GetType().FullName}");
                return;
            }
            if (!typeof(T).IsEnum)
            {
                MelonLogger.Error($"Type T must be an enum, applied type {typeof(T).FullName}");
                return;
            }
            var enumType = Il2CppType.Of<T>();
            if (!SimpleEnumPatcher.enumData.ContainsKey(enumType))
                SimpleEnumPatcher.enumData[enumType] = new System.Collections.Generic.List<(string name, object value)>();

            SimpleEnumPatcher.enumData[enumType].Add((enumName, value));
            var valuesToAdd = new System.Collections.Generic.Dictionary<string, object>
            {
                { enumName, value }
            };

            EnumInjector.InjectEnumValues<T>(valuesToAdd);
            CustomScenario.Logger.Msg($"Added enum value {enumName} to enum type {typeof(T).FullName}");
        }

        public static object ToNumber(Il2CppSystem.Object value)
        {
            var possibleNumberTypes = new List<(string typeName, Func<object> unboxFunc)>
            {
                ("System.Int32", () => value.Unbox<int>()),
                ("System.Byte", () => value.Unbox<byte>()),
                ("System.Int16", () => value.Unbox<short>()),
                ("System.Int64", () => value.Unbox<long>()),
                ("System.SByte", () => value.Unbox<sbyte>()),
                ("System.UInt16", () => value.Unbox<ushort>()),
                ("System.UInt32", () => value.Unbox<uint>()),
                ("System.UInt64", () => value.Unbox<ulong>()),
                ("System.Single", () => value.Unbox<float>()),
                ("System.Double", () => value.Unbox<double>()),
                ("System.Decimal", () => value.Unbox<decimal>())
            };

            // Check if the value is already an enum
            if (value.GetIl2CppType().IsEnum)
            {
                var type = value.GetIl2CppType();
                var unboxMethod = AccessTools.DeclaredMethod(typeof(Il2CppObjectBase), "Unbox");
                foreach (var (typeName, _) in possibleNumberTypes)
                {
                    if (type.GetEnumUnderlyingType().FullName == typeName)
                    {
                        return unboxMethod.MakeGenericMethod(FindType(type.FullName)).Invoke(value, null);
                    }
                }
            }

            foreach (var (typeName, unboxFunc) in possibleNumberTypes)
            {
                if (value.GetIl2CppType().FullName == typeName)
                {
                    return unboxFunc();
                }
            }

            throw new InvalidCastException($"The value of type {value.GetIl2CppType().FullName} cannot be converted to a number.");
        }

        public static object ToNumber(object value)
        {
            if (value is IConvertible convertible)
            {
                object convertedValue = default;
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Byte:
                        convertedValue = convertible.ToByte(NumberFormatInfo.CurrentInfo);
                        break;
                    case TypeCode.SByte:
                        convertedValue = convertible.ToSByte(NumberFormatInfo.CurrentInfo);
                        break;
                    case TypeCode.Int16:
                        convertedValue = convertible.ToInt16(NumberFormatInfo.CurrentInfo);
                        break;
                    case TypeCode.UInt16:
                        convertedValue = convertible.ToUInt16(NumberFormatInfo.CurrentInfo);
                        break;
                    case TypeCode.Int32:
                        convertedValue = convertible.ToInt32(NumberFormatInfo.CurrentInfo);
                        break;
                    case TypeCode.UInt32:
                        convertedValue = convertible.ToUInt32(NumberFormatInfo.CurrentInfo);
                        break;
                    case TypeCode.Int64:
                        convertedValue = convertible.ToInt64(NumberFormatInfo.CurrentInfo);
                        break;
                    case TypeCode.UInt64:
                        convertedValue = convertible.ToUInt64(NumberFormatInfo.CurrentInfo);
                        break;
                    case TypeCode.Single:
                        convertedValue = convertible.ToSingle(NumberFormatInfo.CurrentInfo);
                        break;
                    case TypeCode.Double:
                        convertedValue = convertible.ToDouble(NumberFormatInfo.CurrentInfo);
                        break;
                    case TypeCode.Decimal:
                        convertedValue = convertible.ToDecimal(NumberFormatInfo.CurrentInfo);
                        break;
                    default:
                        MelonLogger.Error($"Unhandled enum type: {value.GetType()}");
                        break;
                }
                if (convertedValue != default)
                {
                    return convertedValue;
                }
            }
            var possibleNumberTypes = new List<(Type type, Func<object, object> convertFunc)>
            {
                (typeof(int), v => Convert.ToInt32(v)),
                (typeof(byte), v => Convert.ToByte(v)),
                (typeof(short), v => Convert.ToInt16(v)),
                (typeof(long), v => Convert.ToInt64(v)),
                (typeof(sbyte), v => Convert.ToSByte(v)),
                (typeof(ushort), v => Convert.ToUInt16(v)),
                (typeof(uint), v => Convert.ToUInt32(v)),
                (typeof(ulong), v => Convert.ToUInt64(v)),
                (typeof(float), v => Convert.ToSingle(v)),
                (typeof(double), v => Convert.ToDouble(v)),
                (typeof(decimal), v => Convert.ToDecimal(v))
            };

            foreach (var (type, convertFunc) in possibleNumberTypes)
            {
                if (value.GetType() == type)
                {
                    return convertFunc(value);
                }
            }

            throw new InvalidCastException($"The value of type {value.GetType().FullName} cannot be converted to a number.");
        }

        public static bool IsNumberEqual(object left, object right, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return ToNumber(left).ToString().Equals(ToNumber(right).ToString(), stringComparison);
        }

        public static string GetName(this Enum @enum)
        {
            return Enum.GetName(@enum.GetType(), @enum);
        }

        public static void AddTrigger(this GameObject gameObject, EventTriggerType triggerType, UnityAction<BaseEventData> action, EventTrigger attachedTrigger = null!)
        {
            EventTrigger eventTrigger = attachedTrigger;
            if (eventTrigger == null && !gameObject.TryGetComponent(out eventTrigger))
            {
                eventTrigger = gameObject.AddComponent<EventTrigger>();
            }
            var triggers = eventTrigger.triggers;
            var entry = new EventTrigger.Entry
            {
                eventID = triggerType,
                callback = new EventTrigger.TriggerEvent()
            };
            entry.callback.AddListener(action);
            triggers.Add(entry);
        }
    }
}
