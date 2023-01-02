using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhEngine.Scene
{
    [CreateAssetMenu(menuName = "PhEngine/Scene/GameSceneConfig" , fileName = "GameSceneConfig")]
    [Serializable]
    public class GameSceneConfig : ScriptableObject
    {
        public string id;
        public SceneReference mainScene;
        public SceneReference[] additiveScenes;
    }

}