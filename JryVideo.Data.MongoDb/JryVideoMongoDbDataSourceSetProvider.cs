using System;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class JryVideoMongoDbDataEngine : IJryVideoDataEngine
    {
        public const string DataSourceProviderName = "MongoDb";

        private readonly JryVideoMongoDbJryVideoDataEngineInitializeParameters _initializeParameters;

        public JryVideoMongoDbDataEngine()
        {
            this._initializeParameters = new JryVideoMongoDbJryVideoDataEngineInitializeParameters();
        }

        public IJryVideoDataEngineInitializeParameters InitializeParametersInfo
        {
            get { return this._initializeParameters; }
        }

        public MongoClient Client { get; private set; }

        public IMongoDatabase Database { get; private set; }

        public async Task<bool> Initialize(JryVideoDataSourceProviderManagerMode mode)
        {
            var builder = new MongoUrlBuilder();
            
            builder.Server = MongoServerAddress.Parse("127.0.0.1:50710");
            builder.DatabaseName = "admin";

            builder.Username = "conanvista";
            builder.Password = "LVpMQhAt31hli8Uiq2Ir";

            this.Client = new MongoDB.Driver.MongoClient(builder.ToMongoUrl());
            //var server = mongoClient.

            //try
            //{
            //    server.Connect(TimeSpan.FromSeconds(1));
            //}
            //catch (MongoConnectionException)
            //{
            //    return null;
            //}

            //if (!server.DatabaseExists("Video5Db"))
            //    throw new NullReferenceException();

            //var db = server.GetDatabase("Video5Db");

            //if (!db.CollectionExists(Video5Series.CollectionName))
            //    db.CreateCollection(Video5Series.CollectionName);
            //if (!db.CollectionExists(Video5Tag.CollectionName))
            //    db.CreateCollection(Video5Tag.CollectionName);
            //if (!db.CollectionExists(Video5Cover.CollectionName))
            //    db.CreateCollection(Video5Cover.CollectionName);

            //return db;

            this.Database = this.Client.GetDatabase("JryVideo_" + mode.ToString());

            return true;
        }

        public ISeriesDataSourceProvider GetSeriesDataSourceProvider()
        {
            return new MongoSeriesDataSource(this, this.Database.GetCollection<JrySeries>("Series"));
        }

        public IDataSourceProvider<Model.JryVideo> GetVideoDataSourceProvider()
        {
            return new MongoVideoDataSource(this, this.Database.GetCollection<Model.JryVideo>("Video"));
        }

        public IFlagDataSourceProvider GetCounterDataSourceProvider()
        {
            return new MongoFlagDataSource(this, this.Database.GetCollection<JryFlag>("Flag"));
        }

        public ICoverDataSourceProvider GetCoverDataSourceProvider()
        {
            return new MongoCoverDataSource(this, this.Database.GetCollection<JryCover>("Cover"));
        }

        public IDataSourceProvider<JryArtist> GetArtistDataSourceProvider()
        {
            return new MongoArtistDataSource(this, this.Database.GetCollection<JryArtist>("Artist"));
        }

        public string Name
        {
            get { return DataSourceProviderName; }
        }
    }
}