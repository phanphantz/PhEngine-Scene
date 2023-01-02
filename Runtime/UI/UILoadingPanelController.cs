using System;
using System.Collections;
using System.Text;
using PhEngine.Core;
using PhEngine.UI;
using UnityEngine;

namespace PhEngine.Scene.UI
{
    public class UILoadingPanelController : SceneLoadDelayer
    {
        [SerializeField] UILoadingPanelSceneMapper sceneMapper;
        public UILoadingPanelSceneMapper SceneMapper => sceneMapper;
        public void SetSceneMapper(UILoadingPanelSceneMapper sceneMapper)
        {
            this.sceneMapper = sceneMapper;
        }
        
        [SerializeField] UILoadingPanelConfig currentPanelConfig;
        public UILoadingPanelConfig CurrentPanelConfig => currentPanelConfig;
        public void SetConfig(UILoadingPanelConfig config) => this.currentPanelConfig = config;

        public UILoadingPanel CurrentPanel { get; private set; }
        public string CurrentPanelId { get; private set; }
        
        Coroutine closePanelRoutine;
        
        protected override void HandleOnStartDelay()
        {
            FetchConfigFromSceneMapper();
            FinishDelayIfConfigIsNull();
        }
        
        public void FetchConfigFromSceneMapper()
        {
            if (sceneMapper == null)
                return;

            if (SceneLoader.Instance == null)
                return;

            var currentSceneConfig = SceneLoader.Instance.LastLoadedSceneConfig;
            var targetSceneConfig = SceneLoader.Instance.TargetSceneConfigToLoad;
            if (currentSceneConfig == null && targetSceneConfig == null)
                return;

            SetConfig(sceneMapper.GetMatchedConfig(currentSceneConfig , targetSceneConfig));
        }

        void FinishDelayIfConfigIsNull()
        {
            if (currentPanelConfig == null)
                FinishDelayAndNotifySceneLoader();
        }

        protected override void HandleOnFinishDelay() {}
        
        public UILoadingPanel PrepareUI()
        {
            if (currentPanelConfig == null)
                return null;

            if (IsPanelAlreadyLoaded())
                PrepareCurrentUI();
            else
                SpawnNewUI();

            return CurrentPanel;
        }
        
        bool IsPanelAlreadyLoaded()
        {
            return CurrentPanel && CurrentPanelId.Equals(currentPanelConfig.loadingScreenPanelId);
        }
        
        void PrepareCurrentUI()
        {
            CurrentPanel.OnShowFinish += HandleFinishDelayAndNotifySceneLoader;
            void HandleFinishDelayAndNotifySceneLoader()
            {
                FinishDelayAndNotifySceneLoader();
                CurrentPanel.OnShowFinish -= HandleFinishDelayAndNotifySceneLoader;
            }
        }
        
        void SpawnNewUI()
        {
            CurrentPanelId = currentPanelConfig.loadingScreenPanelId;
            CurrentPanel = Spawn(CurrentPanelId);
            PrepareCurrentUI();
        }

        public void SetLoadingScale(float scale)
        {
            if (CurrentPanel)
                CurrentPanel.SetLoadingProgress(scale, GetProgressTextAndIconData(scale));
        }

        protected UITextAndIconData GetProgressTextAndIconData(float scale)
        {
            if (!currentPanelConfig.isSetTextByLoadingPercent)
                return null;

            var stringBuilder = new StringBuilder(currentPanelConfig.prefixText);
            stringBuilder.Append(Mathf.RoundToInt(scale * 100f));
            stringBuilder.Append("%");
            stringBuilder.Append(currentPanelConfig.subfixText);
            var text = stringBuilder.ToString();
            return new UITextAndIconData(text);
        }

        public void NotifyFinishLoading()
        {
            if (currentPanelConfig == null)
                return;

            if (closePanelRoutine != null)
                return;

            closePanelRoutine = StartCoroutine(WaitBeforeCloseRoutine());
            IEnumerator WaitBeforeCloseRoutine()
            {
                yield return new WaitForSeconds(currentPanelConfig.delayBeforeCloseInSeconds);
                if (currentPanelConfig.isCloseLoadingScreenOnFinish)
                    TryClearUI();

                closePanelRoutine = null;
            }
        }

        public static UILoadingPanel Spawn(string panelId)
        {
            return UIPanelManager.Instance.Spawn(panelId).GetComponent<UILoadingPanel>();
        }

        public void TryClearUI()
        {
            CurrentPanelId = string.Empty;
            if (CurrentPanel == null)
                return;
            
            CurrentPanel.Close();
            CurrentPanel = null;
        }
        
    }
}