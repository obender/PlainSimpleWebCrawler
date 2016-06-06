using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace PlainSimpleWebCrawler
{
    public class PageContentParser
    {

        public string GetPageContnent(string htmlDocument)
        {
            var document = new HtmlDocument();
            document.LoadHtml(htmlDocument);
            return GetPageContnent(document);

        }

        public string GetPageContnent(HtmlDocument htmlDocument)
        {
            var documentRoot = htmlDocument.DocumentNode;

            var textInNodes =
                documentRoot.DescendantsAndSelf()
                    .Where(node => node.HasChildNodes == false
                                   && node.NodeType == HtmlNodeType.Text
                                   && node.ParentNode.Name != "script"
                                   && node.ParentNode.Name != "style"
                                   && node.InnerText != "\r\n"
                                   && node.InnerText != "\r\n\t")
                    .Select(node => node.InnerText.Trim('\t', '\r', '\n'))
                    .Where(text => text.IsNullOrEmpty() == false);

            var contentBuilder = new StringBuilder();

            foreach (var text in textInNodes)
                contentBuilder.Append(text);

            var pageContent = contentBuilder.ToString();

            return pageContent;
        }
    }
}