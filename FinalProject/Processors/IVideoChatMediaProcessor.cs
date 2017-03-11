using System.Threading.Tasks;

namespace FinalProject.Processors
{
    public interface IVideoChatMediaProcessor
    {
        long ChatId { get; }

        Task ProcessAsync();
    }
}