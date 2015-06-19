using JryVideo.Core.Managers;

namespace JryVideo.Core
{
    public class JryVideoCore
    {
        public static void Initialize()
        {
            DataSourceManager.Current.Scan();
        }
    }
}