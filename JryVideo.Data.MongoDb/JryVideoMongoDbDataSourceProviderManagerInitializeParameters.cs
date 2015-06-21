using System;
using System.Collections.Generic;

namespace JryVideo.Data.MongoDb
{
    public class JryVideoMongoDbJryVideoDataEngineInitializeParameters : IJryVideoDataEngineInitializeParameters
    {
        public string ConnectString { get; private set; }

        public IEnumerable<InitializeParameter> GetOptionalParameters()
        {
            yield break;
        }

        public IEnumerable<InitializeParameter> GetRequiredParameters()
        {
            yield return new InitializeParameter()
            {
                ParameterType = InitializeParameterType.String,
                ParameterName = "connect string",
                IsVaild = obj => !String.IsNullOrWhiteSpace(obj as string)
            };
        }

        public void SetInitializeParameter(InitializeParameter parameter)
        {
            switch (parameter.ParameterName)
            {
                case "connect string":
                    this.ConnectString = parameter.ParameterValue as string;
                    break;
            }
        }
    }
}