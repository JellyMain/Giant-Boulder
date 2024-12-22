using Constants;
using StaticData.Data;
using UnityEngine;
using Zenject;


namespace StaticData.Services
{
    public class StaticDataService : IInitializable
    {
        public MapGenerationConfig MapGenerationConfig { get; private set; }


        public void Initialize()
        {
            LoadStaticData();
        }


        private void LoadStaticData()
        {
            LoadMapChunkConfig();
        }


        private void LoadMapChunkConfig()
        {
            MapGenerationConfig =
                Resources.Load<MapGenerationConfig>(RuntimeConstants.StaticDataPaths.MAP_GENERATION_CONFIG);
        }
    }
}
