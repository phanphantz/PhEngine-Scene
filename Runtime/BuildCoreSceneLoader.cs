using PhEngine.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildCoreSceneLoader : MonoBehaviour
{
    [SerializeField] GameSceneConfig coreSceneConfig;
#if !UNITY_EDITOR
    void Awake()
    {
        LoadCoreSceneByUnitySceneManager();
    }
#endif

    void LoadCoreSceneByUnitySceneManager()
    {
        SceneManager.LoadScene(coreSceneConfig.mainScene, LoadSceneMode.Additive);
        foreach (var scene in coreSceneConfig.additiveScenes)
            SceneManager.LoadScene(scene, LoadSceneMode.Additive);
    }
}