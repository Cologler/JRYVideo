using System.Collections.Generic;

namespace JryVideo.Data
{
    public interface IDataSourceProviderManagerInitializeParameters
    {
        IEnumerable<InitializeParameter> GetOptionalParameters();

        IEnumerable<InitializeParameter> GetRequiredParameters();

        void SetInitializeParameter(InitializeParameter parameter);
    }
}