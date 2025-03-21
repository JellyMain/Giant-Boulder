using GameLoop;
using TerrainGenerator;
using UnityEngine;
using Zenject;


namespace Infrastructure.Installers.Scene
{
    public class GameplaySceneInstaller: MonoInstaller
    {
        [SerializeField] private ChunkUpdater chunkUpdater;
        [SerializeField] private GameTimer gameTimer;
        
        
        public override void InstallBindings()
        {
            BindChunkUpdater();
            BindGameTimer();
        }


        private void BindGameTimer()
        {
            Container.Bind<GameTimer>().FromComponentInNewPrefab(gameTimer).AsSingle().NonLazy();
        }


        private void BindChunkUpdater()
        {
            Container.Bind<ChunkUpdater>().FromComponentInNewPrefab(chunkUpdater).AsSingle().NonLazy();
        }
    }
}
