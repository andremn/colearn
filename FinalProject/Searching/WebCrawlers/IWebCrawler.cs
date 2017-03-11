using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Searching.WebCrawlers
{
    public interface IWebCrawler<in TQuery, TResult>
        where TResult : IEnumerable<ICrawlerObject>
    {
        string Source { get; }

        Task<TResult> GetResultAsync(TQuery query);
    }
}