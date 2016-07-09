using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Model.Interfaces;

namespace JryVideo.Core.Managers.Upgrades
{
    public interface IEveryTimePatch<T> : IPatch where T : class, IObject
    {
        Task ExecuteAsync(IJasilyEntitySetReader<T, string> reader, IObjectEditProvider<T> provider);
    }
}