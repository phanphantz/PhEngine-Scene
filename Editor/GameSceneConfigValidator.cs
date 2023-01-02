#if UNITY_EDITOR
using PhEngine.Core.Editor;
using System.Linq;
using PhEngine.Core;

namespace PhEngine.Scene.Editor
{
    public class GameSceneConfigValidator
    {
        public static GameSceneConfigContainer GetValidSelectedGameSceneConfigContainer()
        {
            var selectedConfigContainer = GetSelectedGameSceneConfigContainer();
            if (selectedConfigContainer == null)
                return null;

            return IsGameSceneConfigContainerValid(selectedConfigContainer) ? selectedConfigContainer : null;
        }

        static GameSceneConfigContainer GetSelectedGameSceneConfigContainer()
        {
            var selectedGameSceneConfigContainers = GetSelectedGameSceneConfigContainers();
            if (!selectedGameSceneConfigContainers.Any())
            {
                PhDebug.LogError<GameSceneConfigValidator>("No selected Game Scene Config Container found");
                return null;
            }

            if (selectedGameSceneConfigContainers.Count() > 1)
            {
                PhDebug.LogError<GameSceneConfigValidator>(
                    "You have more than one Game Scene Config Container selected");
                return null;
            }

            var gameSceneConfigContainer = selectedGameSceneConfigContainers.FirstOrDefault();
            if (gameSceneConfigContainer == null)
            {
                PhDebug.LogError<GameSceneConfigValidator>("Could not find selected Game Scene Config Container");
                return null;
            }

            if (IsSceneConfigsValid())
                return gameSceneConfigContainer;

            PhDebug.LogError<GameSceneConfigValidator>("Scene Configs are empty");
            return null;

            bool IsSceneConfigsValid()
            {
                return gameSceneConfigContainer.sceneConfigs != null &&
                       gameSceneConfigContainer.sceneConfigs.Length != 0;
            }
        }

        static GameSceneConfigContainer[] GetSelectedGameSceneConfigContainers()
        {
            return GetAllSceneConfigContainers()
                .Where(o => o.isSelected).ToArray();
        }

        static GameSceneConfigContainer[] GetAllSceneConfigContainers()
        {
            return EditorAssetUtils.FindAllScriptableObjects<GameSceneConfigContainer>();
        }

        static bool IsGameSceneConfigContainerValid(GameSceneConfigContainer gameSceneConfigContainer)
        {
            var atLeastOneError = IsCoreSceneAndLoaderSceneValid();

            var index = 0;
            foreach (var sceneConfig in gameSceneConfigContainer.sceneConfigs)
            {
                if (IsGameSceneConfigValid(sceneConfig, index) == false)
                    atLeastOneError = true;

                index++;
            }

            return !atLeastOneError;

            bool IsCoreSceneAndLoaderSceneValid()
            {
                return !IsConfigNullOrValid(gameSceneConfigContainer.coreScene) ||
                       !IsConfigNullOrValid(gameSceneConfigContainer.loaderScene);
            }
        }

        static bool IsConfigNullOrValid(GameSceneConfig sceneConfig)
        {
            return !sceneConfig || IsGameSceneConfigValid(sceneConfig);
        }

        static bool IsGameSceneConfigValid(GameSceneConfig scene, int indexer = -1)
        {
            var atLeastOneError = false;
            if (scene == null)
            {
                if (indexer == -1)
                    PhDebug.LogError<GameSceneConfigValidator>("Game Scene Config is null");
                else
                    PhDebug.LogError<GameSceneConfigValidator>("Game Scene Config at element " + indexer + " is null");

                atLeastOneError = true;
            }
            else
            {
                if (scene.id == null || string.IsNullOrEmpty(scene.id))
                {
                    if (indexer == -1)
                        PhDebug.LogError<GameSceneConfigValidator>("Id of Game Scene Config is null or empty");
                    else
                        PhDebug.LogError<GameSceneConfigValidator>("Id of Game Scene Config at element " + indexer +
                                                                  " is null or empty");

                    atLeastOneError = true;
                }

                if (scene.mainScene == null || string.IsNullOrEmpty(scene.mainScene.ScenePath))
                {
                    if (indexer == -1)
                        PhDebug.LogError<GameSceneConfigValidator>("Main scene of Game Scene Config : " + scene.name +
                                                                  " is missing");
                    else
                        PhDebug.LogError<GameSceneConfigValidator>("Main scene of Game Scene Config : " + scene.name +
                                                                  " (element " + indexer +
                                                                  ") is missing");

                    atLeastOneError = true;
                }

                var additiveSceneIndexer = 0;
                foreach (var s in scene.additiveScenes)
                {
                    if (s == null || string.IsNullOrEmpty(s.ScenePath))
                    {
                        PhDebug.LogError<GameSceneConfigValidator>("Additive scene of Game Scene Config : " + scene.id +
                                                                  " (element " +
                                                                  additiveSceneIndexer + ") is missing");
                        atLeastOneError = true;
                    }

                    additiveSceneIndexer++;
                }
            }

            return !atLeastOneError;
        }
    }
}
#endif