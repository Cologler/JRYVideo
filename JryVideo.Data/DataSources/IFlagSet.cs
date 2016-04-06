using Jasily.Data;
using JryVideo.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface IFlagSet : IJasilyEntitySetProvider<JryFlag, string>
    {
        Task<IEnumerable<JryFlag>> QueryAsync(JryFlagType type);

        Task<bool> IncrementAsync(JryFlagType type, string value, int count);
    }
}