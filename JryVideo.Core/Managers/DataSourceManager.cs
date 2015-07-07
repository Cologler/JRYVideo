using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JryVideo.Data;

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

        private readonly Dictionary<string, Func<IJryVideoDataEngine>> SourceSetProviders =
            new Dictionary<string, Func<IJryVideoDataEngine>>();

        private DataSourceManager()
        {
        }

        public void Scan()
        {
            foreach (var providerManagerCreator in GetLocalAllSourceSetProviders())
            {
                string name = null;

                try
                {
                    name = providerManagerCreator().Name;
                    
                }
                catch
                {
                    // ignored
                }

                if (name == null) continue;

                this.SourceSetProviders.Add(name, providerManagerCreator);
            }
        }

        public IJryVideoDataEngine GetDefault()
        {
            var firstOrDefault = this.SourceSetProviders.Values.FirstOrDefault();
            return firstOrDefault != null ? firstOrDefault() : null;
        }

        private static IEnumerable<Func<IJryVideoDataEngine>> GetLocalAllSourceSetProviders()
        {
            var path = Environment.GetCommandLineArgs().First();

            var dir = Path.GetDirectoryName(path);

            if (dir == null) yield break;

            var files = Directory.GetFiles(dir, "JryVideo.Data.*.dll");

            var @interface = typeof(IJryVideoDataEngine);

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
                    IJryVideoDataEngine instance = null;

                    try
                    {
                        instance = type.CreateInstance<IJryVideoDataEngine>();
                    }
                    catch
                    {
                        // ignored
                    }

                    if (instance != null)
                        yield return type.CreateInstance<IJryVideoDataEngine>;
                }
            }
        }
    }
}