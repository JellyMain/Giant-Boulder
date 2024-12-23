using Constants;
using StaticData.Data;
using UnityEngine;
using Zenject;


namespace StaticData.Services
{
    public class StaticDataService 
    {
        public MapGenerationConfig MapGenerationConfig { get; private set; }

        
        public void LoadStaticData()
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
