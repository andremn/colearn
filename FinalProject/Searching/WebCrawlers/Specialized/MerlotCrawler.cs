using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace FinalProject.Searching.WebCrawlers.Specialized
{
    public class MerlotCrawler : LearningMaterialCrawler
    {
        private const uint MaxNumberOfPages = 10;

        private const string MaterialLinkFormat = "http://fedsearch.merlot.org/fedsearch/{0}";

        private const string SearchUrlFormat =
            @"http://fedsearch.merlot.org/fedsearch/fedsearch2.jsp?searchterms={0}&searchtype=search&MERLOT=MERLOT&OpenStax+CNX=OpenStax+CNX&MIT+OCW=MIT+OCW&OER+Commons=OER+Commons&Orange+Grove=Orange+Grove&Wikipedia=Wikipedia&Wisc+Online=Wisc+Online&Google+Books=Google+Books&Scribd=Scribd&SlideShare=SlideShare&Flickr=Flickr&YouTube=YouTube&MERLOT+Physics=MERLOT+Physics&comPADRE=comPADRE&MERLOT+Faculty+Development=MERLOT+Faculty+Development&MERLOT+Information+Technology=MERLOT+Information+Technology&IEEE=IEEE";

        private const string SearchUrlPageFormat = @"http://fedsearch.merlot.org/fedsearch/fedsearch2.jsp?pageon={0}";

        private const string SourceName = "MERLOT - Multimedia Educational Resource for Learning and Online Teaching";

        private static readonly Lazy<MerlotCrawler> LazyInstance = new Lazy<MerlotCrawler>(() => new MerlotCrawler());

        public static MerlotCrawler Instance => LazyInstance.Value;

        private CookieContainer CookieContainer { get; } = new CookieContainer();

        private bool IsReady { get; set; }

        public uint PageNumbers { get; private set; }

        public IList<string> Keywords { get; private set; }

        private MerlotCrawler()
            : base(SourceName)
        {
        }

        public override async Task<IList<LearningMaterial>> GetResultAsync(IEnumerable<string> keywords)
        {
            Keywords = keywords as IList<string> ?? keywords.ToList();
            return await GetMaterials();
        }

        public async Task<IList<LearningMaterial>> GetResultForPageAsync(uint pageNumber)
        {
            if (!IsReady)
            {
                throw new InvalidOperationException($"{nameof(GetResultAsync)} must be called first.");
            }

            if (pageNumber > PageNumbers)
            {
                throw new ArgumentOutOfRangeException($"The '{pageNumber}' argument must be less than or equal {PageNumbers}.");
            }
            
            return await GetMaterialsForPage(pageNumber);
        }

        private static IEnumerable<LearningMaterial> FindMaterials(HtmlDocument htmlDoc)
        {
            var materials = new List<LearningMaterial>();
            var anchorNodes = htmlDoc.DocumentNode.Descendants("a");
            var linkNodes =
                anchorNodes.Where(
                    node => node.Attributes.Contains("href") && node.Attributes["href"].Value.Contains("HitLogger?"));
            
            foreach (var node in linkNodes)
            {
                var link = node.Attributes["href"].Value;

                link = string.Format(MaterialLinkFormat, link);
                link = HttpUtility.UrlDecode(link);

                // <a> -> <strong> -> <font> -> inner text
                var materialName = HttpUtility.UrlDecode(node.FirstChild.FirstChild.InnerText);
                var materialSource = GetMaterialSource(node);

                var material = new LearningMaterial
                {
                    Title = materialName,
                    Link = link,
                    Source = materialSource
                };

                materials.Add(material);
            }

            return materials;
        }

        private static string GetMaterialSource(HtmlNode materialLinkNode)
        {
            var tableColumn = materialLinkNode.ParentNode.ParentNode.ChildNodes.First(node => node.Name == "td");
            var lastLink = tableColumn.LastChild.ChildNodes.First(node => node.Name == "a");
            var firstImg = lastLink.ChildNodes.First(node => node.Name == "img");

            return firstImg.Attributes.Contains("alt") 
                ? firstImg.Attributes["alt"].Value
                : null;
        }

        private async Task<string> GetHtmlAsync(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.CookieContainer = CookieContainer;

            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var receiveStream = response.GetResponseStream();

            if (receiveStream == null)
            {
                return null;
            }

            var readStream = response.CharacterSet == null
                ? new StreamReader(receiveStream)
                : new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

            var data = await readStream.ReadToEndAsync();

            response.Close();
            readStream.Close();

            return data;
        }

        private async Task<IList<LearningMaterial>> GetMaterials()
        {
            var query = string.Join("+", Keywords);
            var encodedQuery = HttpUtility.HtmlEncode(query);
            var searchUrl = string.Format(SearchUrlFormat, encodedQuery);
            var htmlPage = await GetHtmlAsync(searchUrl);
            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(htmlPage);
            PageNumbers = GetPageNumbers(htmlDoc);
            IsReady = true;

            var materials = FindMaterials(htmlDoc);

            return materials.ToList();
        }

        private async Task<IList<LearningMaterial>> GetMaterialsForPage(uint pageNumber)
        {
            var searchUrl = string.Format(SearchUrlPageFormat, pageNumber);
            var htmlPage = await GetHtmlAsync(searchUrl);
            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(htmlPage);

            var materials = FindMaterials(htmlDoc);

            return materials.ToList();
        }

        private static uint GetPageNumbers(HtmlDocument htmlDoc)
        {
            var pagesNumberNode = htmlDoc.DocumentNode.Descendants("span")
            .FirstOrDefault(node => node.InnerText.Contains("Results page"));

            if (pagesNumberNode == null)
            {
                return 0u;
            }

            var pagesNumberText = pagesNumberNode.InnerText;
            var pagesNumberIndex = pagesNumberText.LastIndexOf(" ", StringComparison.OrdinalIgnoreCase);

            if (pagesNumberIndex == -1)
            {
                return 0u;
            }

            int pagesNumber;

            if (!int.TryParse(pagesNumberText.Substring(pagesNumberIndex + 1), out pagesNumber))
            {
                return 0u;
            }

            if (pagesNumber > MaxNumberOfPages)
            {
                return MaxNumberOfPages;
            }

            return (uint)pagesNumber;
        }
    }
}