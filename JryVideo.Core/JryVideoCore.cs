using JryVideo.Core.Managers;
using JryVideo.Data;
using System;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core
{
    public class JryVideoCore
    {
        public static JryVideoCore Current { get; private set; }

        static JryVideoCore()
        {
            Current = new JryVideoCore();
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
            await normal.Initialize(JryVideoDataSourceProviderManagerMode.Public);
            this.NormalDataCenter = new DataCenter(normal);

            var secure = dataSourceManager.GetDefault();
            //foreach (var initializeParameter in secure.InitializeParametersInfo.GetRequiredParameters())
            //{
            //    var para = initializeParameter;
            //    para.ParameterValue = "";
            //    secure.InitializeParametersInfo.SetInitializeParameter(initializeParameter);
            //}
            await secure.Initialize(JryVideoDataSourceProviderManagerMode.Private);
            this.SecureDataCenter = new DataCenter(secure);

            this.Switch(JryVideoDataSourceProviderManagerMode.Public);

            await this.NormalDataCenter.FlagManager.UpdateNameAsync(JryFlagType.EntityFansub, "DmzJ", "DmzJ动漫之家");
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