// Copyright (c) Vignette Project.
// Licensed under MIT. Please see LICENSE for more details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using osuTK;

namespace Vignette.Application.Live2D.Json
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject token");

            var vector = new Vector2();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return vector;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected PropertyName token");

                string prop = reader.GetString();
                reader.Read();

                if (prop == "X" && reader.TryGetSingle(out float x))
                    vector.X = x;

                if (prop == "Y" && reader.TryGetSingle(out float y))
                    vector.Y = y;
            }

            throw new JsonException("Expected EndObject token");
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteEndObject();
        }
    }
}
