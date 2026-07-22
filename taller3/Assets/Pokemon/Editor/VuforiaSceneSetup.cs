using PokedexAR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vuforia;

namespace PokedexAR.Editor
{
    public static class VuforiaSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/PokedexAR.unity";
        private const string ConfigurationPath = "Assets/Resources/VuforiaConfiguration.asset";

        [MenuItem("Pokedex AR/Preparar Vuforia")]
        public static void PrepareProject()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("No se encontro la camara principal de la Pokedex.");
                return;
            }

            if (camera.GetComponent<VuforiaBehaviour>() == null)
            {
                camera.gameObject.AddComponent<VuforiaBehaviour>();
            }

            VuforiaRuntimeTargetFactory targetFactory = Object.FindObjectOfType<VuforiaRuntimeTargetFactory>();
            if (targetFactory != null)
            {
                var serializedFactory = new SerializedObject(targetFactory);
                SerializedProperty targets = serializedFactory.FindProperty("targets");
                ConfigureDatabaseTarget(targets, 0, "rati");
                ConfigureDatabaseTarget(targets, 1, "greninja");
                ConfigureDatabaseTarget(targets, 2, "abra");
                serializedFactory.ApplyModifiedPropertiesWithoutUndo();
            }

            VuforiaConfiguration configuration = VuforiaConfiguration.Instance;
            configuration.Vuforia.MaxSimultaneousImageTargets = 3;
            if (!EditorUtility.IsPersistent(configuration))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                AssetDatabase.CreateAsset(configuration, ConfigurationPath);
            }

            EditorUtility.SetDirty(configuration);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("POKEDEX_VUFORIA_READY: escena y configuracion preparadas; falta App License Key.");
        }

        private static void ConfigureDatabaseTarget(SerializedProperty targets, int index, string targetName)
        {
            if (targets == null || index >= targets.arraySize)
            {
                return;
            }

            SerializedProperty target = targets.GetArrayElementAtIndex(index);
            target.FindPropertyRelative("databaseName").stringValue = "taller_3";
            target.FindPropertyRelative("databaseTargetName").stringValue = targetName;
        }
    }
}
