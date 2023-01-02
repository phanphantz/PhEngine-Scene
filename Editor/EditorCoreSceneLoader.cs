using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

namespace PhEngine.Scene.Editor
{
    [InitializeOnLoad]
    public static class GameSceneLoaderEditor
    {
        static GameSceneLoaderEditor()
        {
            EditorSceneManager.sceneOpened += SceneOpenedCallback;
        }

        static void SceneOpenedCallback(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            // Ignore if scene is opened during build process
            if (BuildPipeline.isBuildingPlayer)
                return;
            
            if (mode != OpenSceneMode.Single)
                return;

            TryLoadCoreSceneAdditively(scene);
        }

        static void TryLoadCoreSceneAdditively(UnityEngine.SceneManagement.Scene currentScene)
        {
            var gameSceneConfigContainer = GameSceneConfigValidator.GetValidSelectedGameSceneConfigContainer();
            if (gameSceneConfigContainer == null)
                return;
            
            var coreSceneConfig = gameSceneConfigContainer.coreScene;
            if (coreSceneConfig == null)
                return;

            if (IsCoreSceneAlreadyOpened()) 
                return;

            EditorSceneManager.OpenScene(coreSceneConfig.mainScene, OpenSceneMode.Additive);
            foreach (var additiveScene in coreSceneConfig.additiveScenes)
                EditorSceneManager.OpenScene(additiveScene, OpenSceneMode.Additive);

            bool IsCoreSceneAlreadyOpened()
            {
                return currentScene.name == coreSceneConfig.mainScene || coreSceneConfig.additiveScenes.Any(s => s == currentScene.name);
            }
        }
        
    }
}