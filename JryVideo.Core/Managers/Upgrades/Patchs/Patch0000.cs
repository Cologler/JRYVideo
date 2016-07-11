using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Data.MongoDb;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Core.Managers.Upgrades.Patchs
{
    public class Patch0000 :
        IPatch<JrySeries>, IEveryTimePatch<JrySeries>,
        IPatch<JryCover>, IEveryTimePatch<JryCover>,
        IPatch<VideoRoleCollection>, IEveryTimePatch<VideoRoleCollection>,
        IPatch<Artist>, IEveryTimePatch<Artist>,
        IPatch<JryFlag>, IEveryTimePatch<JryFlag>
    {
        public static readonly Task<bool> TrueTask = Task.FromResult(true);
        public static readonly Task<bool> FalseTask = Task.FromResult(false);

        private async Task CreateVersionAsync<T>(IJasilyEntitySetReader<T, string> reader,
            IObjectEditProvider<T> provider) where T : class, IObject
        {
            await ((MongoEntitySet<T>)reader).NotExistsFieldCursorAsync(nameof(IObject.Version), async z => await provider.UpdateAsync(z));
        }

        public Task<bool> UpgradeAsync(JrySeries series) => TrueTask;

        public Task<bool> UpgradeAsync(JryCover cover) => TrueTask;

        public Task<bool> UpgradeAsync(VideoRoleCollection item) => TrueTask;

        public Task<bool> UpgradeAsync(Artist item) => TrueTask;

        public Task<bool> UpgradeAsync(JryFlag item) => TrueTask;

        public Task ExecuteAsync(IJasilyEntitySetReader<JrySeries, string> reader, IObjectEditProvider<JrySeries> provider)
            => this.CreateVersionAsync(reader, provider);

        public Task ExecuteAsync(IJasilyEntitySetReader<JryCover, string> reader, IObjectEditProvider<JryCover> provider)
            => this.CreateVersionAsync(reader, provider);

        public Task ExecuteAsync(IJasilyEntitySetReader<VideoRoleCollection, string> reader, IObjectEditProvider<VideoRoleCollection> provider)
            => this.CreateVersionAsync(reader, provider);

        public Task ExecuteAsync(IJasilyEntitySetReader<Artist, string> reader, IObjectEditProvider<Artist> provider)
            => this.CreateVersionAsync(reader, provider);

        public Task ExecuteAsync(IJasilyEntitySetReader<JryFlag, string> reader, IObjectEditProvider<JryFlag> provider)
            => this.CreateVersionAsync(reader, provider);
    }
}