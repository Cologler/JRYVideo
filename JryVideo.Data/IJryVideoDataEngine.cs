﻿using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Data
{
    public interface IJryVideoDataEngine
    {
        string Name { get; }

        IJryVideoDataEngineInitializeParameters InitializeParametersInfo { get; }

        Task<bool> Initialize(JryVideoDataSourceProviderManagerMode mode);

        IDataSourceProvider<JrySeries> GetSeriesDataSourceProvider();

        ICounterDataSourceProvider GetCounterDataSourceProvider();

        ICoverDataSourceProvider GetCoverDataSourceProvider();

        IDataSourceProvider<JryArtist> GetArtistDataSourceProvider();
    }
}