namespace swingtrader.Domain
{
    public class GetKlinesRequest
    {
        public string Symbol { get; set; }
        public string Interval { get; set; }
        public int? Limit { get; set; }
    }
}
