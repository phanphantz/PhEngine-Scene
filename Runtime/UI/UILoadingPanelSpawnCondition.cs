using System;

namespace PhEngine.Scene.UI
{
    [Serializable]
    public class UILoadingPanelSpawnCondition
    {
        public enum SpawnConditionType
        {
            FromAToB, AnySceneToB
        }

        public UILoadingPanelConfig config;
        public SpawnConditionType conditionType;
        public GameSceneConfig sceneConfigA;
        public GameSceneConfig sceneConfigB;

        public bool IsPass(GameSceneConfig currentSceneConfig, GameSceneConfig targetSceneConfig)
        {
            switch (conditionType)
            {
                case SpawnConditionType.AnySceneToB:
                    return targetSceneConfig == sceneConfigB;
                
                case SpawnConditionType.FromAToB:
                    return currentSceneConfig == sceneConfigA && targetSceneConfig == sceneConfigB;
            }

            return false;
        }
    }
}