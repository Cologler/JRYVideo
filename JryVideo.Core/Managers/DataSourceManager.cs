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
    public class DataSourceManager
    {
        public static DataSourceManager Current { get; private set; }

        static DataSourceManager()
        {
            Current = new DataSourceManager();
        }

        private readonly List<IJryVideoDataSourceSetProvider> SourceSetProviders = new List<IJryVideoDataSourceSetProvider>();

        public DataSourceManager()
        {
        }

        public void Scan()
        {
            this.SourceSetProviders.AddRange(GetLocalAllSourceSetProviders());
        }

        public IDataSourceProvider<JrySeries> GetDefault()
        {
            return null;
        }

        private static IEnumerable<IJryVideoDataSourceSetProvider> GetLocalAllSourceSetProviders()
        {
            var path = System.Environment.GetCommandLineArgs().First();

            var dir = System.IO.Path.GetDirectoryName(path);

            if (dir == null) yield break;

            var files = System.IO.Directory.GetFiles(dir, "JryVideo.Data.*.dll");

            var @interface = typeof(IJryVideoDataSourceSetProvider);

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
                    IJryVideoDataSourceSetProvider instance = null;

                    try
                    {
                        instance = type.CreateInstance<IJryVideoDataSourceSetProvider>();
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