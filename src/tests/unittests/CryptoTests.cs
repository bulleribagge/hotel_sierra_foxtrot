using swingtrader;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Flurl;
using Flurl.Http;

namespace unittests
{
    public class CryptoTests
    {
        [Fact]
        public void TestCalculateHmacSha256()
        {
            var input = "hej";
            var key = "hej";
            var expected = "bb3d29faa1c57262afd53b3373034c925d4d1d82783f273114a127607f72bcf3";

            var res = Crypto.CalculateHmacSha256(input, key);
            Assert.Equal(expected, res);
        }

        [Fact]
        public void TestGenerateSignature()
        {
            var req = "https://api.binance.com"
                .AppendPathSegments("api", "v3", "order")
                .SetQueryParam("symbol", "LTCBTC")
                .SetQueryParam("side", "BUY")
                .SetQueryParam("type", "LIMIT")
                .SetQueryParam("timeInForce", "GTC")
                .SetQueryParam("quantity", "1")
                .SetQueryParam("price", "0.1")
                .SetQueryParam("recvWindow", "5000")
                .SetQueryParam("timestamp", "1499827319559")
                .AppendHmac("NhqPtmdSJYdKjVHjA7PZj4Mge3R5YNiP1e3UZjInClVN65XAbvqqM6A7H5fATj0j");

            var res = req.Url.QueryParams["signature"];
            var expected = "c8db56825ae71d6d79447849e617115f4a920fa2acdcab2b053c4b2838bd6b71";

            Assert.Equal(expected, res);
        }
    }
}
