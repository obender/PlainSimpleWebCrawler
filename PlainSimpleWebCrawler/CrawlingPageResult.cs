using System.Net;

namespace PlainSimpleWebCrawler
{
    public class CrawlingPageResult
    {
        public CrawlingPageResult(string url)
        {
            Url = url;
        }

        public string Url { get; }

        public HttpStatusCode HttpStatusCode { get; set; }
        // ReSharper disable once InconsistentNaming
        public string MD5 { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as CrawlingPageResult;
            if (other == null)
                return false;

            return Url == other.Url && HttpStatusCode == other.HttpStatusCode;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return Url + " " + HttpStatusCode;
        }
    }
}
