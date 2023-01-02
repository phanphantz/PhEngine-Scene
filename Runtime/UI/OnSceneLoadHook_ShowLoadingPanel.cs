using UnityEngine;

namespace PhEngine.Scene.UI
{
    public class OnSceneLoadHook_ShowLoadingPanel : OnSceneLoadHook
    {
        [SerializeField] UILoadingPanelController controller;
        
        protected override void HandleOnStartLoading()
        {
            controller.PrepareUI();
        }

        protected override void HandleOnLoadingProgressUpdate(float value)
        {
            controller.SetLoadingScale(value);
        }
        
        protected override void HandleOnFinishLoading()
        {
            controller.NotifyFinishLoading();
        }

    }
}