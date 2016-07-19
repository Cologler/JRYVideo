using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IFlagSet : IQueryableEntitySet<JryFlag, JryFlagType>
    {
        Task<bool> IncrementAsync(JryFlagType type, string value, int count);
    }
}