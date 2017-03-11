using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Searching.WebCrawlers
{
    public abstract class LearningMaterialCrawler : IWebCrawler<IEnumerable<string>, IList<LearningMaterial>>
    {
        protected LearningMaterialCrawler(string source)
        {
            Source = source;
        }

        public string Source { get; }

        public abstract Task<IList<LearningMaterial>> GetResultAsync(IEnumerable<string> keywords);
    }
}