using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    internal class NoirDevToolsEditor : EditorWindow {
        private float _worldShaderBendAmount = 2;
        private GameObject[] _citySpawnPoints = Array.Empty<GameObject>(); 

        private static readonly int WorldBendMagnitudeShaderId = Shader.PropertyToID("_WorldBendMagnitude");

        private string _currentScene;
        private int _lastDoneStoryIdx = 0; // might not need both of these
        private int _enteringStoryIdx = 0;

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
            _currentScene = SceneManager.GetActiveScene().name;
            OnSceneChange();
        }

        private void OnSceneChange()
        {
            // load saved world curve setting
            if (SceneUsesWorldBendShader())
            {
                var gameConfig =
                    (GameConfigScriptableObject)AssetDatabase.LoadAssetAtPath("Assets/Settings/Game/GameConfig.asset",
                        typeof(GameConfigScriptableObject)); 
                _worldShaderBendAmount = Mathf.Abs(gameConfig.WorldShaderCurveAmount);
            }
            
            // Reset the shader curve for non-city scenes. Probably should just not use those shaders in those scenes, but I'm lazy :^)
            SetGlobalWorldBendShader(SceneUsesWorldBendShader() ? _worldShaderBendAmount : 0);

            _citySpawnPoints = Array.Empty<GameObject>();
            if(_currentScene == "City") 
                _citySpawnPoints = GameObject.FindGameObjectsWithTag("CitySpawnPoint");
            
            // Story pos
            _lastDoneStoryIdx = PlayerPrefs.GetInt(StoryController.LastStoryIdPrefName);
            _enteringStoryIdx = PlayerPrefs.GetInt(StoryController.EnteringStoryIdPrefName);
        }

        bool SceneUsesWorldBendShader()
        {
            return _currentScene == "City";
        }

        void OnGUI ()
        {
            switch (_currentScene)
            {
                case "City":
                    ShowLastStorySetter();
                    GUILayout.Space(20);
                    ShowShaderSettings();
                    GUILayout.Space(20);
                    ShowWayPoints();
                    break; 
                case "Conversation":
                    ShowEnteringStorySetter();
                    GUILayout.Space(20);
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

        private void ShowLastStorySetter() {
            GUILayout.Label ("Story Pref", EditorStyles.boldLabel);
            var options = (StoryId[])Enum.GetValues(typeof(StoryId));
            var optionsStrs = options.Select(s => s.ToString()).ToArray(); 
            EditorGUILayout.BeginHorizontal(); 
            _lastDoneStoryIdx = EditorGUILayout.Popup("Last done StoryId", _lastDoneStoryIdx, optionsStrs);
            if (GUILayout.Button("Apply", GUILayout.Width(100))) {
                PlayerPrefs.SetInt(StoryController.LastStoryIdPrefName, _lastDoneStoryIdx);
                PlayerPrefs.SetInt(StoryController.EnteringStoryIdPrefName, _lastDoneStoryIdx+1);
            }
            EditorGUILayout.EndHorizontal(); 
        }

        private void ShowEnteringStorySetter() {
            GUILayout.Label ("Story Pref", EditorStyles.boldLabel);
            var options = (StoryId[])Enum.GetValues(typeof(StoryId));
            var optionsStrs = options.Select(s => s.ToString()).ToArray(); 
            EditorGUILayout.BeginHorizontal(); 
            _enteringStoryIdx = EditorGUILayout.Popup("Entering StoryId", _enteringStoryIdx, optionsStrs);
            if (GUILayout.Button("Apply", GUILayout.Width(100))) {
                PlayerPrefs.SetInt(StoryController.LastStoryIdPrefName, _enteringStoryIdx-1);
                PlayerPrefs.SetInt(StoryController.EnteringStoryIdPrefName, _enteringStoryIdx);
            }
            EditorGUILayout.EndHorizontal(); 
        }

        private void ShowWayPoints()
        {
            _citySpawnPoints = _citySpawnPoints.Where(sp => sp != null).ToArray(); // changing scene can make these references null
            
            // show / refresh waypoints button
            EditorGUILayout.BeginHorizontal(); 
            GUILayout.Label("City Spawn Points", EditorStyles.boldLabel); 
            if (GUILayout.Button(_citySpawnPoints.Length == 0 ? "Show" : "Refresh", GUILayout.Width(100))) 
                _citySpawnPoints = GameObject.FindGameObjectsWithTag("CitySpawnPoint"); 
            EditorGUILayout.EndHorizontal();

            // waypoint buttons
            if(_citySpawnPoints.Length > 0)
                GUILayout.Label("Click to set player position", EditorStyles.miniLabel);
            foreach (var sp in _citySpawnPoints)
            {
                EditorGUILayout.BeginHorizontal(); 
                if (GUILayout.Button(sp.name, GUILayout.Width(100)))
                {
                    // move player to the gameobject location
                    var player = GameObject.FindWithTag("PlayerCityToken");
                    Undo.RecordObject (player.transform, "Player Original Position"); // this helps Unity recognize that something has changed that needs saving
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