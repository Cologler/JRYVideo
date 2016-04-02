using JryVideo.Core.Managers;
using JryVideo.Core.TheTVDB;
using JryVideo.Data;
using System;
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

        public async Task InitializeAsync()
        {
            var dataSourceManager = DataSourceManager.Default;
            dataSourceManager.Scan();

            var normal = dataSourceManager.GetDefault();
            //foreach (var initializeParameter in normal.InitializeParametersInfo.GetRequiredParameters())
            //{
            //    var para = initializeParameter;
            //    para.ParameterValue = "";
            //    normal.InitializeParametersInfo.SetInitializeParameter(initializeParameter);
            //}
            if (!await normal.Initialize(JryVideoDataSourceProviderManagerMode.Public))
            {
                throw new NotSupportedException();
            }
            this.NormalDataCenter = new DataCenter(normal);

            var secure = dataSourceManager.GetDefault();
            //foreach (var initializeParameter in secure.InitializeParametersInfo.GetRequiredParameters())
            //{
            //    var para = initializeParameter;
            //    para.ParameterValue = "";
            //    secure.InitializeParametersInfo.SetInitializeParameter(initializeParameter);
            //}
            await secure.Initialize(JryVideoDataSourceProviderManagerMode.Private);
            this.SecureDataCenter = await secure.Initialize(JryVideoDataSourceProviderManagerMode.Private)
                ? new DataCenter(secure)
                : DataCenter.NotWork;

            this.Switch(JryVideoDataSourceProviderManagerMode.Public);

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

            foreach (var dc in new[] { this.NormalDataCenter, /*this.SecureDataCenter*/ })
            {
                new DatabaseHealthTester(dc).RunOnDebugAsync();
            }

            if (this.RunArgs != null)
            {
                if (this.RunArgs.Contains("--test"))
                {
                    var fix = this.RunArgs.Contains("--fix");
                    foreach (var dc in new[] { this.NormalDataCenter, /*this.SecureDataCenter*/ })
                    {
                        var tester = new DatabaseHealthTester(dc);
                        await tester.RunAsync(fix);
                    }
                }
            }
        }

        public DataCenter NormalDataCenter { get; private set; }

        public DataCenter SecureDataCenter { get; private set; }

        public DataCenter CurrentDataCenter { get; private set; }

        public void Switch(JryVideoDataSourceProviderManagerMode mode)
        {
            switch (mode)
            {
                case JryVideoDataSourceProviderManagerMode.Public:
                    this.CurrentDataCenter = this.NormalDataCenter;
                    break;

                case JryVideoDataSourceProviderManagerMode.Private:
                    this.CurrentDataCenter = this.SecureDataCenter;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}