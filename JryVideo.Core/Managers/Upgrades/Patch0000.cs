using System.Security.Policy;
using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Data.MongoDb;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Core.Managers.Upgrades
{
    public class Patch0000 :
        IPatch<JrySeries>, IEveryTimePatch<JrySeries>,
        IPatch<JryCover>, IEveryTimePatch<JryCover>
    {
        public Task<bool> UpgradeAsync(JrySeries series) => Task.FromResult(true);

        public Task<bool> UpgradeAsync(JryCover cover) => Task.FromResult(true);

        public Task ExecuteAsync(IJasilyEntitySetReader<JrySeries, string> reader, IObjectEditProvider<JrySeries> provider)
            => this.CreateVersionAsync(reader, provider);

        public Task ExecuteAsync(IJasilyEntitySetReader<JryCover, string> reader, IObjectEditProvider<JryCover> provider)
            => this.CreateVersionAsync(reader, provider);

        private async Task CreateVersionAsync<T>(IJasilyEntitySetReader<T, string> reader,
            IObjectEditProvider<T> provider) where T : class, IObject
        {
            await ((MongoEntitySet<T>)reader).NotExistsFieldCursorAsync(nameof(IObject.Version), async z => await provider.UpdateAsync(z));
        }
    }
}