using System;

namespace Services.PersistenceProgress.Analytic
{
    [Serializable]
    public class Application
    {
        public string Version;
        public string UnityVersion;
        public string BundleID;
    }
}