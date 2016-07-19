using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Data.Attributes;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class JryVideoMongoDbDataEngine : IJryVideoDataEngine
    {
        private const string DataSourceProviderName = "MongoDb";

        public JryVideoMongoDbDataEngine()
        {
            this.InitializeParametersInfo = new JryVideoMongoDbJryVideoDataEngineInitializeParameters();
        }

        public IJryVideoDataEngineInitializeParameters InitializeParametersInfo { get; }

        public JryVideoDataSourceProviderManagerMode Mode { get; private set; }

        private sealed class JryVideoMongoDbJryVideoDataEngineInitializeParameters : IJryVideoDataEngineInitializeParameters
        {
            [RequiredParameter]
            public string Server { get; set; }

            [RequiredParameter]
            public string DatabaseName { get; set; }

            [RequiredParameter]
            public string Username { get; set; }

            [RequiredParameter]
            public string Password { get; set; }

            public bool IsVaild(string fieldName, object value)
            {
                return !(value as string).IsNullOrWhiteSpace();
            }
        }

        public MongoClient Client { get; private set; }

        public IMongoDatabase Database { get; private set; }

        public Task<bool> Initialize(JryVideoDataSourceProviderManagerMode mode)
        {
            if (!Enum.IsDefined(typeof(JryVideoDataSourceProviderManagerMode), mode))
                throw new ArgumentOutOfRangeException(nameof(mode));

            this.Mode = mode;

            var builder = new MongoUrlBuilder();

            builder.Server = MongoServerAddress.Parse("127.0.0.1:50710");
            builder.DatabaseName = "admin";

            builder.Username = "conanvista";
            builder.Password = "LVpMQhAt31hli8Uiq2Ir";

            this.Client = new MongoClient(builder.ToMongoUrl());

            this.Database = this.Client.GetDatabase("JryVideo_" + mode.ToString());

            return Task.FromResult(true);
        }

        public async Task<bool> HasPasswordAsync()
        {
            if (this.Mode == JryVideoDataSourceProviderManagerMode.Public) return false;
            return null != await this.GetPasswordAsync();
        }

        private bool isAuthed;

        internal void TestPass()
        {
            if (this.Mode == JryVideoDataSourceProviderManagerMode.Public) return;
            if (this.isAuthed) return;
            throw new AuthenticationException();
        }

        private async Task<JrySettingItem> GetPasswordAsync()
        {
            return (await (await this.SettingCollection().FindAsync(z => z.Id == "password_sha1"))
                .ToListAsync()).FirstOrDefault();
        }

        public async Task<bool> PasswordAsync(string password)
        {
            if (this.Mode == JryVideoDataSourceProviderManagerMode.Public) return true;

            var pw = await this.GetPasswordAsync();
            if (pw == null)
            {
                pw = new JrySettingItem("password_sha1", password);
                await this.SettingCollection().InsertOneAsync(pw);
            }
            this.isAuthed = password == pw.Value;
            return this.isAuthed;
        }

        public ISeriesSet GetSeriesSet()
            => new MongoSeriesDataSource(this, this.Database.GetCollection<JrySeries>("Series"));

        internal IMongoCollection<Model.JryVideo> VideoCollection
            => this.Database.GetCollection<Model.JryVideo>("Video");

        public IFlagableSet<Model.JryVideo> GetVideoSet()
            => new MongoVideoDataSource(this, this.VideoCollection);

        public IJasilyEntitySetProvider<UserWatchInfo, string> GetUserWatchInfoSet()
            => new MongoEntitySet<UserWatchInfo>(this, this.Database.GetCollection<UserWatchInfo>("UserWatchInfo"));

        public IFlagSet GetFlagSet()
            => new MongoFlagDataSource(this, this.Database.GetCollection<JryFlag>("Flag"));

        public ICoverSet GetCoverSet()
            => new MongoCoverDataSource(this, this.Database.GetCollection<JryCover>("Cover"));

        public IArtistSet GetArtistSet()
            => new MongoArtistDataSource(this, this.Database.GetCollection<Artist>("Artist"));

        public IVideoRoleCollectionSet GetVideoRoleInfoSet()
            => new MongoVideoRoleCollectionDataSource(this, this.Database.GetCollection<VideoRoleCollection>("VideoRole"));

        private IMongoCollection<JrySettingItem> SettingCollection()
            => this.Database.GetCollection<JrySettingItem>("Setting");

        public IJasilyEntitySetProvider<JrySettingItem, string> GetSettingSet()
            => new MongoEntitySet<JrySettingItem>(this, this.SettingCollection());

        public string Name => DataSourceProviderName;
    }
}