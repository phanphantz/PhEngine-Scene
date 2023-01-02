using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhEngine.Scene
{
    public abstract class OnSceneLoadHook : MonoBehaviour
    {
        public bool IsHooked { get; private set; }
        void OnEnable()
        {
            Hook();
        }

        void OnDisable()
        {
            Unhook();
        }

        void OnDestroy()
        {
            Unhook();
        }

        public void Hook()
        {
            if (IsHooked)
                return;
            
            var sceneLoader = SceneLoader.Instance;
            if (sceneLoader == null)
                return;
            
            sceneLoader.onLoadingBegin += HandleOnStartLoading;
            sceneLoader.onLoadingProgress += HandleOnLoadingProgressUpdate;
            sceneLoader.onLoadingFinish += HandleOnFinishLoading;
            IsHooked = true;
        }
        
        public void Unhook()
        {
            if (!IsHooked)
                return;
            
            var sceneLoader = SceneLoader.Instance;
            if (sceneLoader == null)
                return;
            
            sceneLoader.onLoadingBegin -= HandleOnStartLoading;
            sceneLoader.onLoadingProgress -= HandleOnLoadingProgressUpdate;
            sceneLoader.onLoadingFinish -= HandleOnFinishLoading;
            IsHooked = false;
        }

        protected abstract void HandleOnStartLoading();
        protected abstract void HandleOnLoadingProgressUpdate(float value);
        protected abstract void HandleOnFinishLoading();
      
    }
}