using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PhEngine.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhEngine.Core;
using AsyncOperation = UnityEngine.AsyncOperation;

namespace PhEngine.Scene
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] bool isShowingLog;
        [SerializeField] GameSceneConfigContainer configContainer;
        public GameSceneConfigContainer ConfigContainer => configContainer;
        
        #region Scene Load Delayer
        
        [Header("Scene Load Delay")]
        [SerializeField] List<SceneLoadDelayer> allSceneLoadDelayerList = new List<SceneLoadDelayer>();
        public List<SceneLoadDelayer> AllSceneLoadDelayerList => allSceneLoadDelayerList;
        
        [SerializeField] List<SceneLoadDelayer> activeSceneLoadDelayerList = new List<SceneLoadDelayer>();
        public List<SceneLoadDelayer> ActiveSceneLoadDelayerList => activeSceneLoadDelayerList;
        
        public void AddDelayer(SceneLoadDelayer delayer)
        {
            if (allSceneLoadDelayerList.Contains(delayer))
                return;
            
            allSceneLoadDelayerList.Add(delayer);
        }

        public void RemoveDelayer(SceneLoadDelayer delayer)
        {
            allSceneLoadDelayerList.Remove(delayer);
        }

        public void ActivateSceneLoadDelayers()
        {
            if (allSceneLoadDelayerList == null)
                return;
            
            activeSceneLoadDelayerList.Clear();
            foreach (var delayer in allSceneLoadDelayerList.Where(delayer => delayer != null))
            {
                activeSceneLoadDelayerList.Add(delayer);
                delayer.StartDelay();
            }
        }

        public void NotifyFinishDelay(SceneLoadDelayer delayer)
        {
            if (activeSceneLoadDelayerList.Contains(delayer))
                activeSceneLoadDelayerList.Remove(delayer);
        }
        
        #endregion
        
        #region Actions

        public Action onLoadingBegin;
        public Action<float> onLoadingProgress;
        public Action onLoadingFinish;

        #endregion

        #region Getters
        
        [Header("Info")]
        [SerializeField] List<GameSceneConfig> loadedSceneConfigList = new List<GameSceneConfig>();
        public List<GameSceneConfig> LoadedSceneConfigList => loadedSceneConfigList;

        List<string> sceneWaitingToBeLoadedList = new List<string>();
        public List<string> SceneWaitingToBeLoadedList => sceneWaitingToBeLoadedList;
        
        public GameSceneConfig LastLoadedSceneConfig { get; private set; }
        
        public GameSceneConfig TargetSceneConfigToLoad { get; private set; }
        public bool IsCoreSceneLoaded { get; private set; }
        bool isJustLoadedCoreScene;
        public int CurrentLoadedSceneCount => TargetSceneToLoadCount - sceneWaitingToBeLoadedList.Count;
        public int TargetSceneToLoadCount { get; private set; }
        public float CurrentOneByOneSceneLoadingProgress { get; private set; }

        public float TotalLoadProgress
        {
            get
            {
                if (TargetSceneToLoadCount == 0)
                    return 0;

                if (sceneWaitingToBeLoadedList.Count == 0)
                    return 1f;

                var currentProgress = CurrentLoadedSceneCount + CurrentOneByOneSceneLoadingProgress;
                var totalLoadProgress = currentProgress / TargetSceneToLoadCount;
                return totalLoadProgress;
            }
        }

        public bool IsLoading { get; private set; }

        #endregion

        #region Load Scene Functions

        public void LoadScenesByConfigId(string id, LoadSceneMode mode, Action onFinishLoading = null)
        {
            if (!TryGetConfigById(id, out var targetConfigToLoad)) 
                return;

            LoadScenesByConfig(targetConfigToLoad, mode, onFinishLoading);
        }

        bool TryGetConfigById(string id, out GameSceneConfig targetConfigToLoad)
        {
            targetConfigToLoad = null;
            if (configContainer == null)
            {
                PhDebug.LogError<SceneLoader>("Cannot load scene. Scene Config Container is missing");
                return false;
            }

            if (configContainer.sceneConfigs == null)
            {
                PhDebug.LogError<SceneLoader>("Cannot load scene. Scene Configs are null");
                return false;
            }

            targetConfigToLoad = FindSceneConfigById(id);
            if (targetConfigToLoad == null)
            {
                PhDebug.LogError<SceneLoader>("Cannot load scene. Config with id : " + id + " not found");
                return false;
            }

            return true;
        }

        public GameSceneConfig FindSceneConfigById(string id)
        {
            return configContainer.sceneConfigs.FirstOrDefault(config => config.id == id);
        }

        public void LoadScenesByConfig(GameSceneConfig config, LoadSceneMode mode,
            Action onFinishLoading = null)
        {
            if (CheckIsCannotLoadAndTryLogError(mode))
                return;
            
            StartCoroutine(LoadScenesFromConfigRoutine(config, mode, onFinishLoading));
        }

        IEnumerator LoadScenesFromConfigRoutine(GameSceneConfig config, LoadSceneMode mode,
            Action onFinishLoading = null)
        {
            //Start
            PrepareScenesToLoad(config);

            //Loading scenes from config
            yield return LoadMainSceneRoutine(config, mode);
            yield return LoadAdditiveScenesRoutine();
            
            //Finish
            FinishLoadingForConfig(config, onFinishLoading, mode);
        }

        void PrepareScenesToLoad(GameSceneConfig config)
        {
            TargetSceneConfigToLoad = config;
            if (isShowingLog)
                PhDebug.Log<SceneLoader>("Loading scenes from game scene config : " + config.id);

            ScheduleSceneLoadFromConfig(config);
        }
        
        IEnumerator LoadMainSceneRoutine(GameSceneConfig config, LoadSceneMode mode)
        {
            var mainSceneLoadMode = isJustLoadedCoreScene ? LoadSceneMode.Additive : mode;
            yield return LoadSceneRoutine(config.mainScene, mainSceneLoadMode);
            isJustLoadedCoreScene = false;
        }

        IEnumerator LoadAdditiveScenesRoutine()
        {
            while (sceneWaitingToBeLoadedList.Count > 0)
            {
                var scene = sceneWaitingToBeLoadedList[0];
                yield return LoadSceneRoutine(scene, LoadSceneMode.Additive);
            }
        }

        void FinishLoadingForConfig(GameSceneConfig config,
            Action onFinishLoading, LoadSceneMode mode)
        {
            if (isShowingLog)
                PhDebug.Log<SceneLoader>("Finished Loading scenes from game scene config : " + config.id);
            
            ModifyLoadedSceneConfigList(config, mode);
            RememberLastLoadedConfig(config);
            onFinishLoading?.Invoke();
            TargetSceneConfigToLoad = null;
        }

        void ModifyLoadedSceneConfigList(GameSceneConfig config, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                loadedSceneConfigList.Clear();

            loadedSceneConfigList.Add(config);
        }

        void RememberLastLoadedConfig(GameSceneConfig config)
        {
            LastLoadedSceneConfig = config;
        }

        bool CheckIsCannotLoadAndTryLogError(LoadSceneMode mode)
        {
            if (IsCanStartLoading())
                return false;

            PhDebug.LogError<SceneLoader>("Cannot load scene. Loading Operation is ongoing");
            return true;

            bool IsCanStartLoading() => !IsLoading;
        }

        void ScheduleSceneLoadFromConfig(GameSceneConfig config)
        {
            ScheduleSceneLoad(config.mainScene);
            if (config.additiveScenes == null)
                return;

            foreach (var additiveScene in config.additiveScenes)
                ScheduleSceneLoad(additiveScene);
        }

        void InsertSceneToLoad(int index, SceneReference sceneRef)
        {
            sceneWaitingToBeLoadedList.Insert(index, sceneRef);
            RefreshTargetSceneToLoadCount();
        }

        void ScheduleSceneLoad(string scene)
        {
            sceneWaitingToBeLoadedList.Add(scene);
            RefreshTargetSceneToLoadCount();
        }

        void RefreshTargetSceneToLoadCount()
        {
            TargetSceneToLoadCount = sceneWaitingToBeLoadedList.Count;
        }

        public void LoadSceneByReference(SceneReference sceneRef, LoadSceneMode mode, Action onFinish = null)
        {
            LoadSceneByName(sceneRef, mode, onFinish);
        }

        public void LoadSceneByName(string sceneName, LoadSceneMode mode, Action onFinish = null)
        {
            if (CheckIsCannotLoadAndTryLogError(mode))
                return;

            ScheduleSceneLoad(sceneName);
            StartCoroutine(LoadSceneRoutine(sceneName, mode, onFinish));
        }

        IEnumerator LoadSceneRoutine(string sceneRef, LoadSceneMode mode, Action onFinish = null)
        {
            //Start
            NotifyLoadingStart();
            yield return new WaitUntil(() => activeSceneLoadDelayerList.Count == 0);
            yield return MakeSureCoreSceneIsLoadedIfNeededRoutine(mode);

            //Loading
            var loadOperation = SceneManager.LoadSceneAsync(sceneRef, mode);
            while (!loadOperation.isDone)
            {
                SetCurrentOneByOneSceneLoadingProgress(loadOperation.progress);
                yield return null;
            }

            //Finish
            HandleLoadSceneRoutineEnd();

            void HandleLoadSceneRoutineEnd()
            {
                UpdateProgressAfterSceneLoadFinished();
                onFinish?.Invoke();
                MarkWaitedSceneLoadedAndTryNotifyLoadingFinish();
            }

            void UpdateProgressAfterSceneLoadFinished()
            {
                SetCurrentOneByOneSceneLoadingProgress(1f);
                TryUpdateSceneLoadProgressLog(sceneRef);
            }
            
            void MarkWaitedSceneLoadedAndTryNotifyLoadingFinish()
            {
                sceneWaitingToBeLoadedList.Remove(sceneRef);
                if (sceneWaitingToBeLoadedList.Count == 0)
                    NotifyLoadingFinish();
            }
        }

        void NotifyLoadingStart()
        {
            CurrentOneByOneSceneLoadingProgress = 0;
            if (IsLoading)
                return;
            
            ActivateSceneLoadDelayers();
            IsLoading = true;
            onLoadingBegin?.Invoke();
        }
        
        IEnumerator MakeSureCoreSceneIsLoadedIfNeededRoutine(LoadSceneMode mode)
        {
            if (!IsNeedCoreSceneToBeLoaded())
                yield break;

            if (IsAlreadyHasCoreSceneOpened())
                yield break;

            yield return LoadCoreSceneRoutine();

            bool IsNeedCoreSceneToBeLoaded() => IsCoreSceneLoaded == false && configContainer.coreScene;
            bool IsAlreadyHasCoreSceneOpened()
            {
                var allSceneCount = SceneManager.sceneCount;
                for (var i = 0; i < allSceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.path == Instance.configContainer.coreScene.mainScene.ScenePath)
                    {
                        IsCoreSceneLoaded = true;
                        return true;
                    }
                }

                return false;
            }

            IEnumerator LoadCoreSceneRoutine()
            {
                InsertSceneToLoad(0, configContainer.coreScene.mainScene);
                yield return LoadSceneRoutine(Instance.configContainer.coreScene.mainScene, mode);
                IsCoreSceneLoaded = true;
                isJustLoadedCoreScene = true;
            }
        }

        void TryUpdateSceneLoadProgressLog(string sceneRef)
        {
            if (isShowingLog)
                PhDebug.Log<SceneLoader>("Loaded scene : " + sceneRef + " (" + (TotalLoadProgress * 100f) + "%)");
        }
        
        void NotifyLoadingFinish()
        {
            TargetSceneToLoadCount = 0;
            CurrentOneByOneSceneLoadingProgress = 0;
            IsLoading = false;
            onLoadingFinish?.Invoke();
        }

        void SetCurrentOneByOneSceneLoadingProgress(float progress)
        {
            CurrentOneByOneSceneLoadingProgress = progress;
            onLoadingProgress?.Invoke(TotalLoadProgress);
        }

        public void ReloadCurrentScene(LoadSceneMode mode = LoadSceneMode.Single, Action onFinish = null)
        {
            if (LastLoadedSceneConfig == null)
                LoadSceneByName(CurrentActiveSceneName, mode, onFinish);
            else
                LoadScenesByConfig(LastLoadedSceneConfig, mode, onFinish);
        }

        public string CurrentActiveSceneName => SceneManager.GetActiveScene().name;
        
        #endregion

        #region Scene Unloading
        
        public void UnloadScenesFromConfigId(string configId, Action onFinishUnload = null, UnloadSceneOptions unloadSceneOptions = UnloadSceneOptions.None)
        {
            if (!TryGetConfigById(configId, out var targetConfigToUnload)) 
                return;

            UnloadScenesFromConfig(targetConfigToUnload , onFinishUnload, unloadSceneOptions);
        }

        public void UnloadScenesFromConfig(GameSceneConfig config , Action onFinishUnload = null, UnloadSceneOptions unloadSceneOptions = UnloadSceneOptions.None)
        {
            StartCoroutine(UnloadScenesFromConfigRoutine());
            IEnumerator UnloadScenesFromConfigRoutine()
            {
                yield return UnloadSceneRoutine(config.mainScene, null , unloadSceneOptions);
                foreach (var configAdditiveScene in config.additiveScenes)
                    yield return UnloadSceneRoutine(configAdditiveScene, null ,unloadSceneOptions);
                
                LoadedSceneConfigList.Remove(config);
                if (isShowingLog)
                    PhDebug.Log<SceneLoader>("Finished Unloaded scenes from config : " + config.id);
                
                onFinishUnload?.Invoke();
            }
        }

        public void UnloadSceneBySceneReference(SceneReference sceneRefToUnload, Action onFinishUnload = null, UnloadSceneOptions unloadSceneOptions = UnloadSceneOptions.None)
            => UnloadSceneByName(sceneRefToUnload, onFinishUnload, unloadSceneOptions);
        
        public void UnloadSceneByName(string sceneNameToUnload, Action onFinishUnload = null, UnloadSceneOptions unloadSceneOptions = UnloadSceneOptions.None)
        {
            StartCoroutine(UnloadSceneRoutine(sceneNameToUnload, onFinishUnload, unloadSceneOptions));
        }

        IEnumerator UnloadSceneRoutine(string sceneNameToUnload, Action onFinishUnload = null, UnloadSceneOptions unloadSceneOptions = UnloadSceneOptions.None)
        {
            var unloadOperation = SceneManager.UnloadSceneAsync(sceneNameToUnload, unloadSceneOptions);
            while (unloadOperation is {isDone: false})
            {
                yield return null;
            }
            
            if (isShowingLog)
                PhDebug.Log<SceneLoader>("Unloaded scene : " + sceneNameToUnload);
            
            onFinishUnload?.Invoke();
        }

        #endregion

        protected override void InitAfterAwake()
        {
        }
    }
}