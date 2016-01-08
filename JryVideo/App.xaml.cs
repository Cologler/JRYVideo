using JryVideo.Configs;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;

namespace JryVideo
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private const string UserConfigPath = @"UserConfig.json";

        public UserConfig UserConfig { get; private set; }

        private FileSystemWatcher configWatcher;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.configWatcher = new FileSystemWatcher(".", "*.json");
            this.configWatcher.Created += this.ConfigWatcher_Changed;
            this.configWatcher.Changed += this.ConfigWatcher_Changed;
            this.configWatcher.Deleted += this.ConfigWatcher_Changed;
            this.configWatcher.EnableRaisingEvents = true;


            this.BeginLoadUserConfig();
        }

        private void ConfigWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name == UserConfigPath)
            {
                if (e.ChangeType == WatcherChangeTypes.Deleted ||
                    e.ChangeType == WatcherChangeTypes.Created ||
                    e.ChangeType == WatcherChangeTypes.Changed)
                {
                    this.BeginLoadUserConfig();
                }
            }

        }

        private void BeginLoadUserConfig()
        {
            Task.Run(() =>
            {
                if (File.Exists(UserConfigPath))
                {
                    try
                    {
                        using (var reader = File.OpenText(UserConfigPath))
                        {
                            var text = reader.ReadToEnd();
                            this.UserConfig = text.JsonToObject<UserConfig>();
                        }
                    }
                    catch
                    {
                        /* ignored */
                    }
                }
                else
                {
                    this.UserConfig = null;
                }
            });
        }
    }
}
