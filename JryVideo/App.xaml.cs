using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;
using JryVideo.Configs;

namespace JryVideo
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public UserConfig UserConfig { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.BeginLoadUserConfig();
        }

        private void BeginLoadUserConfig()
        {
            Task.Run(() =>
            {
                var path = @"UserConfig.json";
                if (!File.Exists(path)) return;
                try
                {
                    using (var reader = File.OpenText(path))
                    {
                        var text = reader.ReadToEnd();
                        this.UserConfig = text.JsonToObject<UserConfig>();
                    }
                }
                catch { /* ignored */ }
            });
        }
    }
}
