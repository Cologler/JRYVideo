using JryVideo.Core.Managers;
using JryVideo.Data;
using JryVideo.Data.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JryVideo.Core
{
    public class JryVideoCore
    {
        public static JryVideoCore Current { get; private set; }

        public event EventHandler<string> Failed;

        static JryVideoCore()
        {
            Current = new JryVideoCore();
        }

        private void Error_EngineNotFound()
        {
            this.Failed?.Invoke(this, "cannot found any data engine");
        }

        public async Task InitializeAsync()
        {
            var dataSourceManager = DataSourceManager.Default;
            dataSourceManager.Scan();

            var normal = dataSourceManager.GetDefault();
            if (normal == null)
            {
                this.Error_EngineNotFound();
                return;
            }
            //foreach (var initializeParameter in normal.InitializeParametersInfo.GetRequiredParameters())
            //{
            //    var para = initializeParameter;
            //    para.ParameterValue = "";
            //    normal.InitializeParametersInfo.SetInitializeParameter(initializeParameter);
            //}
            await normal.Initialize(JryVideoDataSourceProviderManagerMode.Public);
            this.NormalDataCenter = new DataCenter(normal);

            var secure = dataSourceManager.GetDefault();
            if (secure == null)
            {
                this.Error_EngineNotFound();
                return;
            }
            //foreach (var initializeParameter in secure.InitializeParametersInfo.GetRequiredParameters())
            //{
            //    var para = initializeParameter;
            //    para.ParameterValue = "";
            //    secure.InitializeParametersInfo.SetInitializeParameter(initializeParameter);
            //}
            await secure.Initialize(JryVideoDataSourceProviderManagerMode.Private);
            this.SecureDataCenter = new DataCenter(secure);

            this.Switch(JryVideoDataSourceProviderManagerMode.Public);
        }

        private void InitializeEngine(IJryVideoDataEngine engine)
        {
            var mapper = engine.GetType().GetProperties().Select(z => new
            {
                Attr = z.GetCustomAttribute<ParameterAttribute>(),
                Property = z
            }).Where(z => z.Attr != null)
            .ToArray()
            .Split(z => z.Attr.IsOptional);
            foreach (var item in mapper.Item2)
            {

            }
            foreach (var item in mapper.Item1) // optional
            {

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