using JryVideo.Core.Managers;
using JryVideo.Core.TheTVDB;
using JryVideo.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Core
{
    public class JryVideoCore
    {
        public static JryVideoCore Current { get; private set; }

        public TheTVDBClient TheTVDBClient { get; private set; }

        public TheTVDBHost TheTVDBHost { get; }

        static JryVideoCore()
        {
            Current = new JryVideoCore();
        }

        private JryVideoCore()
        {
            this.TheTVDBHost = new TheTVDBHost();
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

        private async void BeginLazyInitialize()
        {

            this.TheTVDBClient = await this.TheTVDBHost.CreateAsync("2C8DAFF32B0E08A7", null);

            foreach (var dc in new[] { this.NormalDataCenter, /*this.SecureDataCenter*/ })
            {
                var tester = new DatabaseHealthTester(dc);
                tester.RunOnDebugAsync();
            }

            if (this.RunArgs != null)
            {
                if (this.RunArgs.Contains("--test"))
                {
                    var fix = this.RunArgs.Contains("--fix");
                    foreach (var dc in new[] { this.NormalDataCenter, this.SecureDataCenter })
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