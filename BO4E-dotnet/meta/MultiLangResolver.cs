﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Reflection;


namespace BO4E.meta
{
    /// <summary>
    /// An ovverride function for JsonConvert.SerializeObject() that checks FieldName attribute and takes changes on translated field name.   
    /// var settings = new JsonSerializerSettings
    ///  {
    ///     ContractResolver = new MultiLangResolver(BO4E.ENUM.Language.EN),
    ///     Formatting = Formatting.Indented
    ///  };
    /// </summary>
    public class MultiLangResolver : DefaultContractResolver
    {
        private readonly Language _language;
        /// <summary>
        /// sending language as a parameter
        /// </summary>
        /// <param name="lang"></param>
        public MultiLangResolver(Language lang)
        {
            _language = lang;
        }
        /// <summary>
        /// Check every field for having translated name or no
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty prop = base.CreateProperty(member, memberSerialization);

            // See if there is a [FieldName] attribute applied to the property 
            // for the requested language
            var att = prop.AttributeProvider.GetAttributes(true)
                                            .OfType<FieldName>()
                                            .FirstOrDefault(a => a.language == _language);

            // if so, change the property name to the one from the attribute
            if (att != null)
            {
                prop.PropertyName = att.text;
            }
            return prop;
        }
    }
}