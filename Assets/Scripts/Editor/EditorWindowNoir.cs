using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    internal class NoirDevToolsEditor : EditorWindow {
        private float _worldShaderBendAmount = 2;
        private GameObject[] _spawnPoints = Array.Empty<GameObject>(); 

        private static readonly int WorldBendMagnitudeShaderId = Shader.PropertyToID("_WorldBendMagnitude");

        private string _currentScene;

        [MenuItem ("**Noir**/Dev Tools")]
        public static void ShowWindow () {
            GetWindow(typeof(NoirDevToolsEditor));
        }

        private void Awake()
        {
            OnHierarchyChange();
        }

        private void OnHierarchyChange()
        {
            CheckForSceneChange();
        }

        private void CheckForSceneChange()
        {
            if (_currentScene == SceneManager.GetActiveScene().name) return;
            OnSceneChange();
            _currentScene = SceneManager.GetActiveScene().name;
        }

        private void OnSceneChange()
        {
            // load saved world curve setting
            if (SceneUsesWorldBendShader())
            {
                var configSettings =
                    (GameConfigScriptableObject)AssetDatabase.LoadAssetAtPath("Assets/Settings/Game/GameConfig.asset",
                        typeof(GameConfigScriptableObject));
                var shaderBend = configSettings.WorldShaderCurveAmount;
                _worldShaderBendAmount = Mathf.Abs(shaderBend);
            }
            
            // reset the shader curve for non-city scenes. Probably should just not use those shaders in those scenes, but I'm lazy :^)
            SetGlobalWorldBendShader(SceneUsesWorldBendShader() ? _worldShaderBendAmount : 0);  
            _spawnPoints = Array.Empty<GameObject>();
        }

        bool SceneUsesWorldBendShader()
        {
            return SceneIsCity();
        }

        bool SceneIsCity()
        {
            return SceneManager.GetActiveScene().name == "City";
        }

        void OnGUI ()
        {
            switch (_currentScene)
            {
                case "City":
                    ShowShaderSettings();
                    GUILayout.Space(20);
                    ShowWayPoints();
                    break;
            }
        }

        private void ShowShaderSettings()
        {
            GUILayout.Label ("Shader settings", EditorStyles.boldLabel);
        
            EditorGUILayout.BeginHorizontal(); 
            _worldShaderBendAmount = EditorGUILayout.Slider ("World Bend Amount", _worldShaderBendAmount, 0, 20);
            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                SetGlobalWorldBendShader(_worldShaderBendAmount);
                
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
            // show / refresh waypoints button
            EditorGUILayout.BeginHorizontal(); 
            GUILayout.Label("City Spawn Points", EditorStyles.boldLabel); 
            if (GUILayout.Button(_spawnPoints.Length == 0 ? "Show" : "Refresh", GUILayout.Width(100))) 
                _spawnPoints = GameObject.FindGameObjectsWithTag("CitySpawnPoint"); 
            EditorGUILayout.EndHorizontal();
                
            // waypoint buttons
            if(_spawnPoints.Length > 0)
                GUILayout.Label("Click to set player position", EditorStyles.miniLabel);
            foreach (var sp in _spawnPoints)
            {
                EditorGUILayout.BeginHorizontal(); 
                if (GUILayout.Button(sp.name, GUILayout.Width(100)))
                {
                    // move player to the gameobject location
                    var player = GameObject.FindWithTag("PlayerCityToken");
                    Undo.RecordObject (player.transform, "Player Original Position"); // helps Unity recognize that something has changed that needs saving
                    player.transform.position = sp.transform.position;
                
                    // update the world bend shader position
                    player.GetComponent<PlayerCityToken>().SetGlobalShaderPosition();
                
                    // bring the scene camera with us
                    var playerPos = player.transform.position;
                    SceneView.lastActiveSceneView.AlignViewToObject(player.transform);
                    SceneView.lastActiveSceneView.LookAt(playerPos, Quaternion.Euler(35, 0, 0));
                }
                GUILayout.Label(sp.transform.position.ToString(), EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Label("(add new spawn points by tagging GameObjects with 'CitySpawnPoint')",  EditorStyles.miniLabel);
        }

        private void SetGlobalWorldBendShader(float worldShaderBendAmount)
        {
            Shader.SetGlobalFloat(WorldBendMagnitudeShaderId, -1 * worldShaderBendAmount);
        }
    }
}