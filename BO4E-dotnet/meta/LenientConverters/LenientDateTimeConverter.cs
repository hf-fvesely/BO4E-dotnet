﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Collections.Generic;
using System.Globalization;

namespace BO4E.meta.LenientConverters
{
    /// <summary>
    /// The lenient DateTimeConverter allows for transforming strings into (nullable) DateTimeOffset(?) objects,
    /// even if their formatting is somehow weird.
    /// </summary>
    public class LenientDateTimeConverter : IsoDateTimeConverter
    {
        // basic structure copied from https://stackoverflow.com/a/33172735/10009545

        private readonly DateTimeOffset? _defaultDateTime;

        /// <summary>
        /// initialize using a default date time
        /// </summary>
        /// <param name="defaultDateTime"></param>
        public LenientDateTimeConverter(DateTimeOffset? defaultDateTime = null)
        {
            this._defaultDateTime = defaultDateTime;
        }

        /// <summary>
        /// initialize using no default datetime
        /// </summary>
        public LenientDateTimeConverter() : this(null)
        {
        }

        private readonly List<(string, bool)> ALLOWED_DATETIME_FORMATS = new List<(string, bool)>()
            {
               ("yyyy-MM-ddTHH:mm:ss", false),
               ("yyyy-MM-ddTHH:mm:sszzzz",true),
               ("yyyyMMddHHmm",false),
               ("yyyyMMddHHmmss",false),
               (@"yyyyMMddHHmmss'--T::zzzz'",false), // ToDo: remove again. this is just a buggy, nasty workaround
            };

        /// <inheritdoc cref="JsonConverter.CanConvert(Type)"/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTimeOffset)
                || objectType == typeof(DateTimeOffset?)
                || objectType == typeof(DateTime)
                || objectType == typeof(DateTime?);
        }

        /// <inheritdoc cref="JsonConverter.ReadJson(JsonReader, Type, object, JsonSerializer)"/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string rawDate;
            switch (reader.Value)
            {
                case null:
                    return null;
                case string value:
                    rawDate = value;
                    break;
                case DateTimeOffset value when objectType == typeof(DateTimeOffset):
                    return value;
                case DateTime time when objectType == typeof(DateTime):
                    return time;
                default:
                    rawDate = reader.Value.ToString();
                    break;
            }
            // First try to parse the date string as is (in case it is correctly formatted)
            if (objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?))
            {
                if (DateTimeOffset.TryParse(rawDate, out DateTimeOffset dateTimeOffset))
                {
                    return dateTimeOffset;
                }
                foreach ((string dtf, bool asUniversal) in ALLOWED_DATETIME_FORMATS)
                {
                    if (DateTimeOffset.TryParseExact(rawDate, dtf, CultureInfo.InvariantCulture, asUniversal ? DateTimeStyles.AssumeUniversal : DateTimeStyles.None, out dateTimeOffset))
                    {
                        return dateTimeOffset;
                    }
                }
            }
            else if (objectType == typeof(DateTime) || objectType == typeof(DateTime?))
            {
                if (DateTime.TryParse(rawDate, out DateTime dateTime))
                {
                    return dateTime;
                }
                foreach ((string dtf, bool asUniversal) in ALLOWED_DATETIME_FORMATS)
                {
                    if (DateTime.TryParseExact(rawDate, dtf, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                    {
                        return dateTime;
                    }
                }
            }
            // It's not a date after all, so just return the default value
            if (objectType == typeof(DateTimeOffset?) || objectType == typeof(DateTime?))
            {
                return null;
            }
            if (this._defaultDateTime.HasValue)
            {
                return _defaultDateTime;
            }
            else
            {
                try
                {
                    return base.ReadJson(reader, objectType, existingValue, serializer);
                }
                catch (FormatException fe) when (fe.Message == "The UTC representation of the date '0001-01-01T00:00:00' falls outside the year range 1-9999.")
                {
                    if (objectType == typeof(DateTime))
                    {
                        return DateTime.MinValue;
                    }
                    else if (objectType == typeof(DateTime?) || objectType == typeof(DateTimeOffset?))
                    {
                        return null;
                    }
                    else
                    {
                        return DateTimeOffset.MinValue;
                    }
                }
                catch (ArgumentOutOfRangeException ae) when (ae.Message == "The UTC time represented when the offset is applied must be between year 0 and 10,000. (Parameter 'offset')")
                {
                    if (objectType == typeof(DateTime))
                    {
                        return DateTime.MinValue;
                    }
                    else if (objectType == typeof(DateTime?) || objectType == typeof(DateTimeOffset?))
                    {
                        return null;
                    }
                    else
                    {
                        return DateTimeOffset.MinValue;
                    }
                }
                //throw new JsonReaderException($"Couldn't convert {rawDate} to any of the allowed date time formats: {String.Join(";", ALLOWED_DATETIME_FORMATS)})");
            }
        }

        /// <inheritdoc cref="JsonConverter.CanWrite"/>
        public override bool CanWrite => false;

        /// <inheritdoc cref="JsonConverter.WriteJson(JsonWriter, object, JsonSerializer)"/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}