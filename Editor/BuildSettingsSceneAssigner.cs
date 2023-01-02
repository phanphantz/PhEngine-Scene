using System.Collections.Generic;
using System.Linq;
using PhEngine.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace PhEngine.Scene.Editor
{
    public class BuildSettingsSceneAssigner : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        //Before build, Check for game scene validity and Assign scenes to Build Settings 
        public void OnPreprocessBuild(BuildReport report)
        {
            var buildSettingsAssignmentResult = TryAssignBuildSettings();
            if (buildSettingsAssignmentResult == false)
                throw new BuildFailedException("Game Scene Configs are not valid !");
        }

        [MenuItem("PhEngine/Scene/Refresh Build Settings")]
        public static void RefreshBuildSettingsFromMenu()
        {
            var buildSettingsAssignmentResult = TryAssignBuildSettings();
            if (buildSettingsAssignmentResult)
                PhDebug.Log<GameSceneConfigValidator>("All Game Scene Configs are valid and assigned to Build Settings");
        }

        static bool TryAssignBuildSettings()
        {
            var gameSceneConfigContainer = GameSceneConfigValidator.GetValidSelectedGameSceneConfigContainer();
            if (gameSceneConfigContainer == null) 
                return false;

            AssignScenesToBuildSettings(gameSceneConfigContainer);
            return true;
        }
        
        static void AssignScenesToBuildSettings(GameSceneConfigContainer gameSceneConfigContainer)
        {
            var editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            TryAddLoaderSceneToBuildSettings();
            TryAddCoreSceneToBuildSettings();
            AddTheRestOfSceneConfigsToBuildSettings();
            SaveToBuildSettings();

            void TryAddLoaderSceneToBuildSettings()
            {
                if (gameSceneConfigContainer.loaderScene && gameSceneConfigContainer.loaderScene != null)
                {
                    editorBuildSettingsScenes.Add(
                        new EditorBuildSettingsScene(gameSceneConfigContainer.loaderScene.mainScene.ScenePath, true));
                }
            }

            void TryAddCoreSceneToBuildSettings()
            {
                if (gameSceneConfigContainer.coreScene && gameSceneConfigContainer.coreScene != null)
                {
                    editorBuildSettingsScenes.Add(
                        new EditorBuildSettingsScene(gameSceneConfigContainer.coreScene.mainScene.ScenePath, true));
                }
            }
            
            void AddTheRestOfSceneConfigsToBuildSettings()
            {
                foreach (var sceneConfig in gameSceneConfigContainer.sceneConfigs)
                    AddScenesFromConfigToBuildSettings(sceneConfig);
            }
            
            void AddScenesFromConfigToBuildSettings(GameSceneConfig sceneConfig)
            {
                AddMainSceneToBuildSettings(sceneConfig);
                AddAdditiveScenesToBuildSettings(sceneConfig);
            }
            
            void AddMainSceneToBuildSettings(GameSceneConfig sceneConfig)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(sceneConfig.mainScene.ScenePath, true));
            }
        
            void AddAdditiveScenesToBuildSettings(GameSceneConfig sceneConfig)
            {
                editorBuildSettingsScenes.AddRange(
                    sceneConfig.additiveScenes.Select(s => new EditorBuildSettingsScene(s.ScenePath, true)));
            }

            void SaveToBuildSettings()
            {
                EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
            }
            
        }
    }
}