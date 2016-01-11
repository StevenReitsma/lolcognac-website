using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LoLTournament.Models.Financial
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentMethod
    {
        [EnumMember(Value = "ideal")]
        iDeal,
        [EnumMember(Value = "bitcoin")]
        Bitcoin,
    }
}
