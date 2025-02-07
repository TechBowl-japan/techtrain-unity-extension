#nullable enable
using System.Threading.Tasks;
using TechtrainExtension.Api.Models.v3;

namespace TechtrainExtension
{
    public class RailwayManager
    {
        private Manifests.Manager manifestsManager;
        private Manifests.Models.Railway? manifestRailway;

        private Api.Client apiClient;
        private Api.Models.v3.Railway? apiRailway;

        private bool isUserPaid;


        public RailwayManager(Api.Client _apiClient, bool _isUserPaid)
        {
            manifestsManager = new Manifests.Manager();
            apiClient = _apiClient;
            isUserPaid = _isUserPaid;
        }

        public async Task<bool> Initialize()
        {
            manifestRailway = manifestsManager.GetRailway();
            var response = await apiClient.GetRailway(manifestRailway.railwayId);
            if (response == null)
            {
                return false;
            }
            apiRailway = response.data;
            return true;
        }

        public Manifests.Models.Railway GetManifestRailway()
        {
            return manifestRailway;
        }

        public Api.Models.v3.Railway GetApiRailway()
        {
            return apiRailway;
        }

        public bool IsInitialized()
        {
            return apiRailway != null;
        }

        public async Task<bool> Reload()
        {
            return await Initialize();
        }

        public bool IsClearAllStations()
        {
            if (apiRailway == null)
            {
                return false;
            }
            return apiRailway.total_stations_count == apiRailway.clear_stations_count;
        }

        public bool IsAlreadyChallenging()
        {
            if (apiRailway == null || apiRailway.railway_stations.Length == 0)
            {
                return false;
            }
            var firstStation = apiRailway.railway_stations[0];
            return firstStation.user_railway_station != null;
        }

        public RailwayStation? GetCurrentStation()
        {
            foreach (var station in apiRailway!.railway_stations)
            {
                if (station.user_railway_station != null && station.user_railway_station.status != UserRailwayStationStatus.completed)
                {
                    return station;
                }
            }
            return null;
        }

        public bool IsStationPermitted(RailwayStation station)
        {
            if (apiRailway == null)
            {
                return false;
            }
            if (station.access_level == RailwayStationAccessLevel.free)
            {
                return true;
            }
            return isUserPaid;
        }
    }
}