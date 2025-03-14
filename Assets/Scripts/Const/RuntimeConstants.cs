namespace Const
{
    public static class RuntimeConstants
    {
        public static class PlayerProgressKeys
        {
            public const string PLAYER_PROGRESS_KEY = "PlayerProgress";
        }


        public static class PrefabPaths
        {
            public const string PLAYER = "RuntimePrefabs/Player/PlayerBoulder";
            public const string VIRTUAL_CAMERA = "RuntimePrefabs/Camera/VirtualCamera";
            public const string COIN = "RuntimePrefabs/Coin/Coin";
            public const string PLAYER_CONTROLS_UI = "RuntimePrefabs/UI/PlayerControlsCanvas";
            public const string UI_CAMERA = "RuntimePrefabs/Camera/UICamera";
            public const string SCORE_AND_CURRENCY_UI = "RuntimePrefabs/UI/ScoreAndCurrencyCanvas";
        }


        public static class Scenes
        {
            public const string GAME_SCENE = "GameplayScene";
            public const string MAIN_MENU_SCENE = "MenuScene";
        }


        public static class StaticDataPaths
        {
            public const string MAP_GENERATION_CONFIGS = "StaticData/MapConfigs";
            public const string STRUCTURE_SPAWNER_CONFIGS = "StaticData/SpawnerConfigs";
        }


        public static class Tags
        {
            public const string FRAGMENTS_ROOT = "FragmentsRoot";
            public const string CONTAINER = "Container";
            public const string INDESTRUCTIBLE = "Indestructible";
        }


        public static class Layers
        {
            public const string STRUCTURE_FRAGMENTED = "StructureFragmented";
            public const string STRUCTURE_OBJECT = "StructureObject";
            public const string GROUND_LAYER = "Ground";
            public const string COIN_LAYER = "Coin";
        }
    }
}
