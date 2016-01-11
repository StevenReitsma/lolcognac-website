using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LoLTournament.Models.Financial
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentMode
    {
        [EnumMember(Value = "live")]
        Live,
        [EnumMember(Value = "test")]
        Test
    }
}
