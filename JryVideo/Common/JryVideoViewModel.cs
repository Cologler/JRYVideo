using Jasily.ComponentModel;
using JryVideo.Core;
using JryVideo.Core.Managers;
using JryVideo.Core.TheTVDB;

namespace JryVideo.Common
{
    public static class JryVideoViewModel
    {
        public static DataCenter GetManagers(this JasilyViewModel vm) => JryVideoCore.Current.CurrentDataCenter;

        public static TheTVDBClient GetTVDBClient(this JasilyViewModel vm) => JryVideoCore.Current.TheTVDBClient;
    }
}