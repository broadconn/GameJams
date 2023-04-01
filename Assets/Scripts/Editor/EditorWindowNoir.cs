using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal class NoirDevToolsEditor : EditorWindow {
        private float _worldShaderBendAmount = 2;
        private GameObject[] _spawnPoints = Array.Empty<GameObject>(); 

        private static readonly int WorldBendMagnitudeShaderId = Shader.PropertyToID("_WorldBendMagnitude");

        [MenuItem ("**Noir**/Dev Tools")]
        public static void  ShowWindow () {
            GetWindow(typeof(NoirDevToolsEditor));
        }
    
        void OnGUI () {
            ShowShaderSettings();
            GUILayout.Space(15);
            ShowWayPoints();
        }

        private void ShowShaderSettings()
        {
            GUILayout.Label ("Shader settings", EditorStyles.boldLabel);
        
            EditorGUILayout.BeginHorizontal(); 
            _worldShaderBendAmount = EditorGUILayout.Slider ("World Bend Amount", _worldShaderBendAmount, 0, 20);
            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            { 
                Shader.SetGlobalFloat(WorldBendMagnitudeShaderId, -1 * _worldShaderBendAmount);
                
                // set this in the config scriptableObject so we can use the value in the build
                var configSettings = (GameConfigScriptableObject)AssetDatabase.LoadAssetAtPath("Assets/Settings/Game/GameConfig.asset", typeof(GameConfigScriptableObject));
                configSettings.WorldShaderCurveAmount = -1 * _worldShaderBendAmount;
                EditorUtility.SetDirty(configSettings);
            }
            EditorGUILayout.EndHorizontal();
        
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal(); 
            GUILayout.Label("Set bend origin to player position");
            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                var player = GameObject.FindWithTag("PlayerCityToken");
                player.GetComponent<PlayerCityToken>().SetGlobalShaderPosition();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("(useful if you manually move the player)", EditorStyles.miniLabel);
        }

        private void ShowWayPoints()
        {
            // validate waypoints
            _spawnPoints = _spawnPoints.Where(sp => sp != null).ToArray();
            
            GUILayout.Label("City Spawn Points", EditorStyles.boldLabel);
            if(_spawnPoints.Length == 0)
                if (GUILayout.Button("Show")) 
                    _spawnPoints = GameObject.FindGameObjectsWithTag("CitySpawnPoint"); 
        
            if(_spawnPoints.Length > 0)
                GUILayout.Label("Click to set player position", EditorStyles.miniLabel);
            foreach (var wp in _spawnPoints)
            {
                EditorGUILayout.BeginHorizontal(); 
                if (GUILayout.Button(wp.name, GUILayout.Width(100)))
                {
                    // move player to the gameobject location
                    var player = GameObject.FindWithTag("PlayerCityToken");
                    Undo.RecordObject (player.transform, "Player Original Position"); // helps Unity recognize that something has changed that needs saving
                    player.transform.position = wp.transform.position;
                
                    // update the world bend shader position
                    player.GetComponent<PlayerCityToken>().SetGlobalShaderPosition();
                
                    // bring the scene camera with us
                    var playerPos = player.transform.position;
                    SceneView.lastActiveSceneView.AlignViewToObject(player.transform);
                    SceneView.lastActiveSceneView.LookAt(playerPos, Quaternion.Euler(35, 0, 0));
                }
                GUILayout.Label(wp.transform.position.ToString(), EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Label("(add new spawn points by tagging GameObjects with 'CitySpawnPoint')",  EditorStyles.miniLabel);
        }
    }
}