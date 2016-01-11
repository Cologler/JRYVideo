using Jasily.ComponentModel;
using JryVideo.Data;

namespace JryVideo.Main
{
    public class DataModeViewModel : JasilyViewModel<JryVideoDataSourceProviderManagerMode>
    {
        public DataModeViewModel(JryVideoDataSourceProviderManagerMode source)
            : base(source)
        {
        }

        public string Name => this.Source.ToString();
    }
}