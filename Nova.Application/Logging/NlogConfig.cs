
using NLog;

namespace Nova.Application.Logging
{
    public static class NlogConfig
    {
        public static Logger InstatiateNlog()
        {
             return LogManager.GetCurrentClassLogger();
        }
    }
}
