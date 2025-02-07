using System.IO;
using Newtonsoft.Json;
using TechtrainExtension.Manifests.Models;
using UnityEngine;

namespace TechtrainExtension.Manifests
{
    public class Manager
    {
        private Railway railway;
        private static string projectRootPath = Path.Join(Application.dataPath, "..");

        private static string manifestRootPath = Path.Join(projectRootPath, ".techtrain", "manifests");

        public Manager() { 
            var railwayPath = Path.Join(manifestRootPath, "railway.json");
            railway = ReadJon<Railway>(railwayPath);
        }

        public Railway GetRailway()
        {
            return railway;
        }

        public Station GetStation(int order)
        {
            if (!railway.stations.TryGetValue(order.ToString(), out var stationFileName))
            {
                return null;
            }


            var stationPath = Path.Join(manifestRootPath, stationFileName);
            return ReadJon<Station>(stationPath);
        }

        private T ReadJon<T>(string path)
        {
            using (var sr = new StreamReader(path, System.Text.Encoding.UTF8))
            {
                var jsonReadData = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(jsonReadData);
            }
        }
    }
}