using UnityEngine;

namespace PhEngine.Scene.UI
{
    [CreateAssetMenu(menuName = "PhEngine/UI/UILoadingPanelConfig", fileName = "UILoadingPanelConfig", order = 0)]
    public class UILoadingPanelConfig : ScriptableObject
    {
        [Header("Spawning")]
        public string loadingScreenPanelId;

        [Header("Updating")] 
        public bool isSetTextByLoadingPercent;
        public string prefixText;
        public string subfixText;
        
        [Header("Closing")]
        public bool isCloseLoadingScreenOnFinish = true;
        public float delayBeforeCloseInSeconds = 1f;
    }
}