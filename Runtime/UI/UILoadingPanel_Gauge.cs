using PhEngine.UI;
using UnityEngine;

namespace PhEngine.Scene.UI
{
    public class UILoadingPanel_Gauge : UILoadingPanel
    {
        [SerializeField] UIGauge gaugeUI;
        
        public override void SetLoadingProgress(float scale, UITextAndIconData uiTextAndIconData = null)
        {
            gaugeUI.SetFill(scale , uiTextAndIconData);
        }
    }
}