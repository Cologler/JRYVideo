using Jasily.ComponentModel;
using JryVideo.Core;
using JryVideo.Core.Managers;
using JryVideo.Core.TheTVDB;
using JryVideo.Main;

namespace JryVideo.Common
{
    public static class JryVideoViewModel
    {
        public static DataAgent GetAgent(this JasilyViewModel vm)
            => JryVideoCore.Current.DataAgent;

        public static DataCenter GetManagers(this JasilyViewModel vm)
            => JryVideoCore.Current.DataAgent.CurrentDataCenter;

        public static TheTVDBClient GetTVDBClient(this JasilyViewModel vm)
            => JryVideoCore.Current.TheTVDBHost.LastClientInstance;

        public static void ShowStatueMessage(this JasilyViewModel vm, string msg)
            => MainWindow.ShowMessage(msg);
    }
}