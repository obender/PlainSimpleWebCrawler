using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PlainSimpleWebCrawler
{
    public class SimpleCrawler
    {
        private readonly HttpClient _client;
        private readonly int _maxConcurrentDownload;
        private readonly string _baseUrl;
        private readonly ConcurrentBag<CrawlingPageResult> _results;
        private readonly ConcurrentBag<string> _crawlBag;

        public SimpleCrawler(string baseUrl)
        {
            _maxConcurrentDownload = 3;
            _baseUrl = baseUrl.WithHttp();
            if (!baseUrl.EndsWith("/"))
                _baseUrl = _baseUrl.TrimEnd('/');

            _client = new HttpClient();
            _results = new ConcurrentBag<CrawlingPageResult>();
            _crawlBag = new ConcurrentBag<string>();
        }

        /// <exception cref="ArgumentNullException"><paramref /> is null.</exception>
        private async Task<IEnumerable<string>> Crawl(string startUrl)
        {
            var result = new ConcurrentBag<string>(); // I know list is not the ideal data structure here. Let's use it for simplicity
            var pagesToProcess = new Queue<string>();

            var runningTasks = new List<Task<IEnumerable<string>>> { ProcessUrl(startUrl) };
            result.Add(startUrl);

            while (runningTasks.Any())
            {
                var firstCompletedTask = await Task.WhenAny(runningTasks);
                runningTasks.Remove(firstCompletedTask);
                var urlsFound = await firstCompletedTask;

                /*
                 * At this point we know the url we found in the page
                 * we finished to process. 
                 * It's time to enque them
                 */
                foreach (var url in urlsFound.Distinct())
                {
                    if (!pagesToProcess.Any(d => d.Equals(url, StringComparison.InvariantCultureIgnoreCase)))
                        pagesToProcess.Enqueue(url);
                }
                /*
                 * Now we should start some new Taks
                 * to process new pages
                 */
                while (pagesToProcess.Any() && runningTasks.Count < _maxConcurrentDownload)
                {
                    var url = pagesToProcess.Dequeue();
                    runningTasks.Add(ProcessUrl(url));
                    result.Add(url);
                }
            }
            return result;
        }

        private async Task<IEnumerable<string>> ProcessUrl(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);
                var childUrls = await ProcessResponse(response);
                return childUrls;
            }
            catch (Exception loggException)
            {
                //ToDO:  Add logging here
                return new List<string>();
            }
        }

        private async Task<List<string>> ProcessResponse(HttpResponseMessage response)
        {
            var html = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(html))
                return new List<string>();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            if (doc.DocumentNode == null)
                return new List<string>();

            var next = doc.DocumentNode.SelectNodes("//a");
            if (next == null || next.Count == 0)
                return new List<string>();


            var mainDomain = _baseUrl.UrlGetDomain();
            var nextBach = new List<string>();
            foreach (var link in next)
            {
                var url = link.GetAttributeValue("href", string.Empty);
                if (url.IsNullOrEmpty())
                    continue;

                if (url.StartsWith("/") && !url.StartsWith("//"))
                    url = (_baseUrl + url).UrlFixUrl();
                else if (url.StartsWith("/www.", StringComparison.InvariantCultureIgnoreCase))
                    url = _baseUrl.ToUri().Scheme + url;
                else if (url.StartsWith("//"))
                    url = _baseUrl.ToUri().Scheme + url;

                var domain = url.UrlGetDomain();
                if (domain != mainDomain)
                    continue;

                if (_results.Any(d => d.Url.Equals(url, StringComparison.InvariantCultureIgnoreCase)))
                    continue;
                if (_crawlBag.Any(d => d.Equals(url, StringComparison.InvariantCultureIgnoreCase)))
                    continue;
                nextBach.Add(url);
                _crawlBag.Add(url);
            }

            var contentParser = new PageContentParser();
            var pageContent = contentParser.GetPageContnent(doc);
            var md5 = pageContent.Md5Get();
            var pageResult = new CrawlingPageResult(response.RequestMessage.RequestUri.OriginalString)
            {
                HttpStatusCode = response.StatusCode,
                MD5 = md5
            };
            _results.Add(pageResult);
            return nextBach;
        }


        public ConcurrentBag<CrawlingPageResult> Run()
        {
            try
            {
                Crawl(_baseUrl.WithHttp()).Wait(TimeSpan.FromHours(24));
            }
            catch (Exception exception)
            {
                //ToDO:  Add logging here
            }
            return _results;
        }
    }
}
