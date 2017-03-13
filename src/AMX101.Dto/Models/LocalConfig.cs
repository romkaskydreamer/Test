using System.IO;
using System.Reflection;

namespace AMX101.Dto.Models
{
    public class LocalConfig: IConfig
    {
        public string Region { get; set; }

        private string path;
        public string LocalDataFolder
        {
            get
            {
                if (!Path.IsPathRooted(path))
                {
                    var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase).Replace(@"file:\", "");
                    path = Path.Combine(root, path);
                }
                return path;
            }

            set { path = value; }

        }
        public string[] Regions { get; set; }

        public LocalConfig()
        {
            path = "data";
            Regions = new [] { "aus", "nz", "sng" };
        }
    }
}
