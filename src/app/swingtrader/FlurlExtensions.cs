using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace swingtrader
{
    public static class FlurlExtensions
    {
        public static IFlurlRequest AppendHmac(this IFlurlRequest req, string key)
        {
            var hmacString = Crypto.CalculateHmacSha256(req.Url.Query, key);
            req.SetQueryParam("signature", hmacString);
            return req;
        }

        public static IFlurlRequest AppendHmac(this Url url, string key) => new FlurlRequest(url).AppendHmac(key);

        public static IFlurlRequest AppendHmac(this string url, string key) => new FlurlRequest(url).AppendHmac(key);
    }
}
