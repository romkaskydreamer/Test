using System;
using System.IO;
using System.Reflection;

namespace AMX101.JsonExport
{
    public class Program
    {
        /// <summary>
        /// Exports data to the specified folder in the json format
        /// </summary>
        /// <param name="args">
        /// args[0] the utility name
        /// args[1] the region
        /// args[2] the target folder
        /// args[3] the source server
        /// </param>
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("parameters: region [target folder] [server]");
            }
            else
            {
                var region = "aus";

                var server = "";
                if (args.Length > 0)
                {
                    region = args[0];
                }

                var path = "data";

                if (args.Length > 1)
                {
                    path = @args[1];
                    if (!Path.IsPathRooted(path))
                    {
                        var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase).Replace(@"file:\","");
                        path = Path.Combine(root, path);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                }

                if (args.Length > 2)
                {
                    server = args[2];
                }

                var exportJson = new ExportJson(server, region, path);

                try
                {

                    Console.WriteLine("Exporting Postcodes...");
                    exportJson.ExportPostCodesToJson();

                    Console.WriteLine("Exporting Claims...");
                    exportJson.ExportClaimsToJson();
                    Console.WriteLine("Exporting Static Claims...");
                    exportJson.ExportStaticClaimsToJson();
                    Console.WriteLine("Exporting Sources...");
                    exportJson.ExportSourcesToJson();
                    Console.WriteLine("Exporting Claim Values...");
                    exportJson.ExportClaimValuesToJson();

                    Console.WriteLine("Done!");
                }
                catch (Exception e)
                {
                    var err = e.ToString();
                    Console.WriteLine($"Error exporting from {server} to {path} for {region}: {err}", server, path, region, err);
                }
            }

            Console.WriteLine("Press a key ");
            Console.ReadKey();
        }
    }
}
