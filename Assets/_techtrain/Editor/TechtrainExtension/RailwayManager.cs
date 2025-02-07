#nullable enable
using NUnit.Framework;
using System.Threading.Tasks;
using TechtrainExtension.Api.Models.v3;
using TechtrainExtension.Manifests.Models;
using UnityEngine;
using System.Collections.Generic;

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

        public Manifests.Models.Station GetManifestStation(int order)
        {
            return manifestsManager.GetStation(order);
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
            if (apiRailway == null)
            {
                return null;
            }
            foreach (var station in apiRailway.railway_stations)
            {
                if (station?.user_railway_station != null && station.user_railway_station.status != UserRailwayStationStatus.completed)
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

        public Station? GetCurrentStationManifest()
        {
            var currentStation = GetCurrentStation();
            if (manifestRailway == null || currentStation == null)
            {
                return null;
            }
            return GetManifestStation(currentStation.order);
        }

        internal async Task ReportTestResult(int order, TestRunner runner)
        {
            if (apiRailway == null || runner.isRestored)
            {
                return;
            }
            string error = "";
            foreach(var result in runner.results)
            {
                if (result.isPassed != true)
                {
                    error += $"{result.path}\n{result.errorMessage}\n";
                }
            }

            var body = new Api.Models.v3.StationClearJudgementBody()
            {
                order = order,
                is_clear = runner.IsTestSucessful(order),
                error_content = error
            };


            var response = await apiClient.PostStationClearJudgement(apiRailway.id, body);
            Debug.Log(response?.message);
            Debug.Log(response?.data);
            Debug.Log(response?.code);
        }
    }
}