using JryVideo.Data.Attributes;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;
using System;
using System.Data;
using System.Threading.Tasks;

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
            var builder = new MongoUrlBuilder();

            builder.Server = MongoServerAddress.Parse("127.0.0.1:50710");
            builder.DatabaseName = "admin";

            builder.Username = "conanvista";
            builder.Password = "LVpMQhAt31hli8Uiq2Ir";

            this.Client = new MongoClient(builder.ToMongoUrl());

            this.Database = this.Client.GetDatabase("JryVideo_" + mode.ToString());

            return Task.FromResult(true);
        }

        public ISeriesSet GetSeriesSet()
        {
            return new MongoSeriesDataSource(this, this.Database.GetCollection<JrySeries>("Series"));
        }

        internal IMongoCollection<Model.JryVideo> VideoCollection
            => this.Database.GetCollection<Model.JryVideo>("Video");

        public IFlagableSet<Model.JryVideo> GetVideoSet()
            => new MongoVideoDataSource(this, this.VideoCollection);

        public IFlagSet GetFlagSet()
            => new MongoFlagDataSource(this, this.Database.GetCollection<JryFlag>("Flag"));

        public ICoverSet GetCoverSet()
            => new MongoCoverDataSource(this, this.Database.GetCollection<JryCover>("Cover"));

        public IArtistSet GetArtistSet()
            => new MongoArtistDataSource(this, this.Database.GetCollection<JryArtist>("Artist"));

        public IJasilyEntitySetProvider<VideoRoleCollection, string> GetVideoRoleInfoSet()
            => new MongoJryEntitySet<VideoRoleCollection>(this, this.Database.GetCollection<VideoRoleCollection>("VideoRole"));

        public IJasilyEntitySetProvider<JrySettingItem, string> GetSettingSet()
        {
            return new MongoEntitySet<JrySettingItem>(this.Database.GetCollection<JrySettingItem>("Setting"));
        }

        public string Name => DataSourceProviderName;
    }
}