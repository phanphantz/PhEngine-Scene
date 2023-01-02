using System;
using System.Collections;
using System.Collections.Generic;
using PhEngine.Core;
using UnityEngine;

namespace PhEngine.Scene
{
    public abstract class SceneLoadDelayer : MonoBehaviour
    {
        void OnEnable()
        {
            if (SceneLoader.Instance)
                SceneLoader.Instance.AddDelayer(this);
        }

        void OnDestroy()
        {
            if (SceneLoader.Instance)
                SceneLoader.Instance.RemoveDelayer(this);
        }

        public bool IsDelaying { get; private set; }

        public void StartDelay()
        {
            if (IsDelaying)
                return;

            IsDelaying = true;
            HandleOnStartDelay();
        }

        protected abstract void HandleOnStartDelay();

        public void FinishDelayAndNotifySceneLoader()
        {
            if (!IsDelaying)
                return;
            
            IsDelaying = false;
            HandleOnFinishDelay();
            
            if (SceneLoader.Instance)
                SceneLoader.Instance.NotifyFinishDelay(this);
        }
        
        protected abstract void HandleOnFinishDelay();
        
    }
}