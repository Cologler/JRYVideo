using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IFlagSet : IEntitySet<JryFlag>
    {
        Task<IEnumerable<JryFlag>> QueryAsync(JryFlagType type);

        Task<bool> IncrementAsync(JryFlagType type, string value, int count);
    }
}