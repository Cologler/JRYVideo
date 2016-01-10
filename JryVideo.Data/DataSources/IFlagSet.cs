using JryVideo.Model;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface IFlagSet : IJasilyEntitySetProvider<JryFlag, string>
    {
        Task<IEnumerable<JryFlag>> QueryAsync(JryFlagType type);

        Task<bool> RefMathAsync(JryFlagType type, string value, int count);
    }
}