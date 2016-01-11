using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LoLTournament.Models.Financial
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardSecurityType
    {
        [EnumMember(Value = "normal")]
        Normal,
        [EnumMember(Value = "3dsecure")]
        ThreeDSecure
    }
}
