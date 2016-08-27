using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JryVideo.Core.Managers;
using JryVideo.Core.Managers.Upgrades;
using JryVideo.Core.TheTVDB;

namespace JryVideo.Core
{
    public class JryVideoCore
    {
        private int theTVDBClientInitializing;
        private TheTVDBClient theTVDBClient;
#if DEBUG
        private const bool DebugMode = true;
#else
        private const bool DebugMode = false;
#endif

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

            var args = this.RunArgs.Select(z => z.ToLower()).ToArray();
            if (args.Contains("--upgrade") || DebugMode)
            {
                await new Upgrader(Current.DataAgent.CurrentDataCenter).RunAsync();
            }

            this.BeginLazyInitialize();
        }

        public TheTVDBClient GetTheTVDBClient()
        {
            var client = this.theTVDBClient;
            if (client == null) this.InitializeTheTVDBClient();
            return client;
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

#if DEBUG
            foreach (var dc in new[] { this.DataAgent.NormalDataCenter, /*this.SecureDataCenter*/ })
            {
                new DatabaseHealthTester(dc).RunOnDebugAsync();
            }
#endif

            if (this.RunArgs != null)
            {
                var args = this.RunArgs.Select(z => z.ToLower()).ToArray();

                if (args.Contains("--test"))
                {
                    var fix = args.Contains("--fix");
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