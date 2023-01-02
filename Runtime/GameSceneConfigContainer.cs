using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhEngine.Scene
{
    [CreateAssetMenu(menuName = "PhEngine/Scene/GameSceneConfigContainer" , fileName = "GameSceneConfigContainer")]
    public class GameSceneConfigContainer : ScriptableObject
    {
        public bool isSelected;
        public GameSceneConfig[] sceneConfigs;
        public GameSceneConfig coreScene;
        public GameSceneConfig loaderScene;
    }

}