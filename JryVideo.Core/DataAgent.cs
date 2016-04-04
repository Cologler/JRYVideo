using JryVideo.Core.Managers;
using JryVideo.Data;
using JryVideo.Model;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace JryVideo.Core
{
    public sealed class DataAgent
    {
        private int threadId;

        public DataAgent()
        {
            this.threadId = Thread.CurrentThread.ManagedThreadId;
        }

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
        }

        public DataCenter NormalDataCenter { get; private set; }

        public DataCenter SecureDataCenter { get; private set; }

        public DataCenter CurrentDataCenter { get; private set; }

        public void Switch(JryVideoDataSourceProviderManagerMode mode)
        {
            if (this.threadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException();
            }

            var old = this.CurrentDataCenter;

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

            var @new = this.CurrentDataCenter;

            if (old != @new)
            {
                if (old != null)
                {
                    old.FlagManager.FlagChanged -= this.FlagManager_FlagChanged;
                }

                Debug.Assert(@new != null);
                @new.FlagManager.FlagChanged += this.FlagManager_FlagChanged;
            }
        }

        private void FlagManager_FlagChanged(object sender, EventArgs<JryFlagType, string, string> e)
            => this.FlagChanged?.Invoke(this, Tuple.Create(e.Value1, e.Value2, e.Value3));

        public event EventHandler<EventArgs<Tuple<JryFlagType, string, string>>> FlagChanged;
    }
}