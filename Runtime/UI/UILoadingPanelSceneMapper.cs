using UnityEngine;

namespace PhEngine.Scene.UI
{
    [CreateAssetMenu(menuName = "PhEngine/UI/UILoadingPanelSceneMapper", fileName = "UILoadingPanelSceneMapper", order = 0)]
    public class UILoadingPanelSceneMapper : ScriptableObject
    {
        public bool isForceUseDefaultLoadingPanelConfigIfNotMatch = true;
        public UILoadingPanelConfig defaultLoadingPanelConfig;
        public UILoadingPanelSpawnCondition[] spawnConditions;

        public UILoadingPanelConfig GetMatchedConfig(GameSceneConfig currentSceneConfig, GameSceneConfig targetSceneConfig)
        {
            var result = isForceUseDefaultLoadingPanelConfigIfNotMatch? defaultLoadingPanelConfig : null;
            foreach (var condition in spawnConditions)
            {
                if (!condition.IsPass(currentSceneConfig, targetSceneConfig)) 
                    continue;
                
                result = condition.config;
                break;
            }

            return result;
        }
    }
}