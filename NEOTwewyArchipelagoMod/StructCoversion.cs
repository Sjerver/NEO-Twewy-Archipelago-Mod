using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace NEOTwewyArchipelagoMod
{
    public class GameLocationIDConverter : JsonConverter<GameLocationID>
    {
        public override void WriteJson(JsonWriter writer, GameLocationID value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Value);
        }

        public override GameLocationID ReadJson(JsonReader reader, Type objectType, GameLocationID existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
            {
                long v = Convert.ToInt64(reader.Value);
                return new GameLocationID(v);
            }

            if (reader.TokenType == JsonToken.String)
            {
                if (long.TryParse((string)reader.Value, out long v))
                    return new GameLocationID(v);
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing GameLocationID");
        }
    }

    public class ArchipelagoLocationIDConverter : JsonConverter<ArchipelagoLocationID>
    {
        public override void WriteJson(JsonWriter writer, ArchipelagoLocationID value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Value);
        }

        public override ArchipelagoLocationID ReadJson(JsonReader reader, Type objectType, ArchipelagoLocationID existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
            {
                long v = Convert.ToInt64(reader.Value);
                return new ArchipelagoLocationID(v);
            }

            if (reader.TokenType == JsonToken.String)
            {
                if (long.TryParse((string)reader.Value, out long v))
                    return new ArchipelagoLocationID(v);
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing ArchipelagoLocationID");
        }
    }

    // TypeConverters for dictionary keys (accept old ToString() and plain numbers)
    public class GameLocationIDTypeConverter : TypeConverter
    {
        private static readonly Regex _digits = new Regex(@"(-?\d+)", RegexOptions.Compiled);

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || sourceType == typeof(long) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is long l) return new GameLocationID(l);
            if (value is string s)
            {
                // Try plain parse
                if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out long v))
                    return new GameLocationID(v);

                // Try to extract digits from legacy ToString form: "GameLocationID { Value = 2801 }"
                var m = _digits.Match(s);
                if (m.Success && long.TryParse(m.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                    return new GameLocationID(v);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || destinationType == typeof(long) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is GameLocationID id)
            {
                if (destinationType == typeof(string)) return id.Value.ToString(CultureInfo.InvariantCulture);
                if (destinationType == typeof(long)) return id.Value;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class ArchipelagoLocationIDTypeConverter : TypeConverter
    {
        private static readonly Regex _digits = new Regex(@"(-?\d+)", RegexOptions.Compiled);

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || sourceType == typeof(long) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is long l) return new ArchipelagoLocationID(l);
            if (value is string s)
            {
                if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out long v))
                    return new ArchipelagoLocationID(v);

                var m = _digits.Match(s);
                if (m.Success && long.TryParse(m.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                    return new ArchipelagoLocationID(v);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || destinationType == typeof(long) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is ArchipelagoLocationID id)
            {
                if (destinationType == typeof(string)) return id.Value.ToString(CultureInfo.InvariantCulture);
                if (destinationType == typeof(long)) return id.Value;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
