using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Patty_TransferableSaves_MOD
{
    public class PlayerPrefs_Json
    {
        public List<PlayerPrefs_Data> Datas { get; set; } = new List<PlayerPrefs_Data>();
    }

    public class PlayerPrefs_Json_Converter : JsonConverter<PlayerPrefs_Json>
    {
        public override PlayerPrefs_Json? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token.");
            }

            var playerPrefs_Json = new PlayerPrefs_Json();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return playerPrefs_Json;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected PropertyName token.");
                }
                string? propertyName = reader.GetString();
                if (propertyName == null)
                {
                    throw new JsonException("Property name is null.");
                }

                // Advance the reader to the property value
                reader.Read();

                if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int intValue))
                {
                    playerPrefs_Json.Datas.Add(new PlayerPrefs_Data(propertyName, intValue, PlayerPrefs_Data.Type.Int));
                }
                else if (reader.TokenType == JsonTokenType.Number && reader.TryGetSingle(out float floatValue))
                {
                    playerPrefs_Json.Datas.Add(new PlayerPrefs_Data(propertyName, floatValue, PlayerPrefs_Data.Type.Float));
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    playerPrefs_Json.Datas.Add(new PlayerPrefs_Data(propertyName, reader.GetString(), PlayerPrefs_Data.Type.String));
                }
            }
            throw new JsonException("Expected EndObject token.");
        }

        public override void Write(Utf8JsonWriter writer, PlayerPrefs_Json value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var str in value.Datas)
            {
                switch (str.dataType)
                {
                    case PlayerPrefs_Data.Type.Int:
                        writer.WriteNumber(str.key, (int)str.GetPlayerPrefsValue());
                        break;
                    case PlayerPrefs_Data.Type.Float:
                        writer.WriteNumber(str.key, (float)str.GetPlayerPrefsValue());
                        break;
                    case PlayerPrefs_Data.Type.String:
                        writer.WriteString(str.key, (string)str.GetPlayerPrefsValue());
                        break;
                }
            }
            writer.WriteEndObject();
        }
    }
}
