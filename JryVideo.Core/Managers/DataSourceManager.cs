using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using JryVideo.Data;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    /// <summary>
    /// 对客户端支持的数据源进行管理及选择
    /// </summary>
    public class DataSourceManager
    {
        public static DataSourceManager Default { get; private set; }

        static DataSourceManager()
        {
            Default = new DataSourceManager();
        }

        private readonly List<IJryVideoDataSourceProviderManager> SourceSetProviders = new List<IJryVideoDataSourceProviderManager>();

        private DataSourceManager()
        {
        }

        public void Scan()
        {
            this.SourceSetProviders.AddRange(GetLocalAllSourceSetProviders());
        }

        public IJryVideoDataSourceProviderManager GetDefault()
        {
            return this.SourceSetProviders.FirstOrDefault();
        }

        private static IEnumerable<IJryVideoDataSourceProviderManager> GetLocalAllSourceSetProviders()
        {
            var path = System.Environment.GetCommandLineArgs().First();

            var dir = System.IO.Path.GetDirectoryName(path);

            if (dir == null) yield break;

            var files = System.IO.Directory.GetFiles(dir, "JryVideo.Data.*.dll");

            var @interface = typeof(IJryVideoDataSourceProviderManager);

            foreach (var file in files)
            {
                Assembly assembly = null;

                try
                {
                    assembly = Assembly.LoadFrom(file);
                }
                catch
                {
                    // ignored
                }

                if (assembly == null) continue;

                foreach (var type in assembly.GetExportedTypes()
                    .Where(z => @interface.IsAssignableFrom(z)))
                {
                    IJryVideoDataSourceProviderManager instance = null;

                    try
                    {
                        instance = type.CreateInstance<IJryVideoDataSourceProviderManager>();
                    }
                    catch
                    {
                        // ignored
                    }

                    if (instance != null)
                        yield return instance;
                }
            }
        }
    }
}