using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace swingtrader.Domain
{
    public class GetAccountInformationResponse
    {
        //[JsonProperty(PropertyName = "makerCommission")]
        public int MakerCommission { get; set; }
        //[JsonProperty(PropertyName = "taketCommission")]
        public int TakerCommission { get; set; }
        //[JsonProperty(PropertyName = "buyerCommission")]
        public int BuyerCommission { get; set; }
        //[JsonProperty(PropertyName = "sellerCommission")]
        public int SellerCommission { get; set; }
        //[JsonProperty(PropertyName = "canTrade")]
        public bool CanTrade { get; set; }
        //[JsonProperty(PropertyName = "canWithdraw")]
        public bool CanWithdraw { get; set; }
        //[JsonProperty(PropertyName = "canDeposit")]
        public bool CanDeposit { get; set; }
        //[JsonProperty(PropertyName = "updateTime")]
        [JsonConverter(typeof(UnixMillisecondTimestampConverter))]
        public DateTime UpdateTime { get; set; }
        //[JsonProperty(PropertyName = "balances")]
        public List<Balance> Balances { get; set; }
    }
}
