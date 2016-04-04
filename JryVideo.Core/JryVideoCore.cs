using JryVideo.Core.Managers;
using JryVideo.Core.TheTVDB;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JryVideo.Core
{
    public class JryVideoCore
    {
        private int theTVDBClientInitializing;
        private TheTVDBClient theTVDBClient;

        public static JryVideoCore Current { get; private set; }

        public TheTVDBHost TheTVDBHost { get; } = new TheTVDBHost();

        static JryVideoCore()
        {
            Current = new JryVideoCore();
        }

        private JryVideoCore()
        {
        }

        public string[] RunArgs { get; set; }

        public DataAgent DataAgent { get; } = new DataAgent();

        public async Task InitializeAsync()
        {
            await this.DataAgent.InitializeAsync();

            this.BeginLazyInitialize();
        }

        public TheTVDBClient TheTVDBClient
        {
            get
            {
                var client = this.theTVDBClient;
                if (client == null) this.InitializeTheTVDBClient();
                return client;
            }
        }

        private async void InitializeTheTVDBClient()
        {
            if (this.theTVDBClientInitializing == 0)
            {
                if (Interlocked.CompareExchange(ref this.theTVDBClientInitializing, 1, 0) == 0)
                {
                    var client = await this.TheTVDBHost.CreateAsync("2C8DAFF32B0E08A7", null);
                    if (client != null) Interlocked.CompareExchange(ref this.theTVDBClient, client, null);
                    this.theTVDBClientInitializing = 0;
                }
            }
        }

        private async void BeginLazyInitialize()
        {
            this.InitializeTheTVDBClient();

            foreach (var dc in new[] { this.DataAgent.NormalDataCenter, /*this.SecureDataCenter*/ })
            {
                new DatabaseHealthTester(dc).RunOnDebugAsync();
            }

            if (this.RunArgs != null)
            {
                if (this.RunArgs.Contains("--test"))
                {
                    var fix = this.RunArgs.Contains("--fix");
                    foreach (var dc in new[] { this.DataAgent.NormalDataCenter, /*this.SecureDataCenter*/ })
                    {
                        var tester = new DatabaseHealthTester(dc);
                        await tester.RunAsync(fix);
                    }
                }
            }
        }
    }
}