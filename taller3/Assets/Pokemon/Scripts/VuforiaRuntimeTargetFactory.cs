using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

namespace PokedexAR
{
    /// <summary>Creates the two runtime Image Targets once Vuforia is initialized.</summary>
    public sealed class VuforiaRuntimeTargetFactory : MonoBehaviour
    {
        [Serializable]
        public sealed class InstantTarget
        {
            [Tooltip("Vuforia device database name. Leave empty to use the texture directly.")]
            public string databaseName;

            [Tooltip("Image Target name inside the Vuforia database.")]
            public string databaseTargetName;

            [Tooltip("Readable image used by Vuforia as an Image Target.")]
            public Texture2D image;

            [Tooltip("Real-world printed width of the target in metres.")]
            public float physicalWidth = 0.12f;

            [Tooltip("Pokemon evolution content attached to the detected target.")]
            public PokemonTargetController content;
        }

        [SerializeField] private Camera arCamera;
        [SerializeField] private InstantTarget[] targets;
        [SerializeField] private Text statusLabel;

        public void Configure(Camera camera, InstantTarget[] instantTargets, Text status)
        {
            arCamera = camera;
            targets = instantTargets;
            statusLabel = status;
        }

        private void Awake()
        {
            if (targets == null)
            {
                return;
            }

            foreach (InstantTarget target in targets)
            {
                target.content?.EnableVuforiaMode();
            }
        }

        private IEnumerator Start()
        {
            if (arCamera == null || targets == null || targets.Length == 0)
            {
                SetStatus("CONFIGURACION AR INCOMPLETA");
                yield break;
            }

            if (arCamera.GetComponent<VuforiaBehaviour>() == null)
            {
                arCamera.gameObject.AddComponent<VuforiaBehaviour>();
            }

            SetStatus("INICIANDO CAMARA AR...");
            float timeout = Time.realtimeSinceStartup + 20f;
            while (!VuforiaApplication.Instance.IsInitialized && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            if (!VuforiaApplication.Instance.IsInitialized || VuforiaBehaviour.Instance == null)
            {
                SetStatus("AGREGA LA CLAVE EN VUFORIA CONFIGURATION");
                yield break;
            }

            ObserverFactory factory = VuforiaBehaviour.Instance.ObserverFactory;
            foreach (InstantTarget target in targets)
            {
                if (target.content == null)
                {
                    continue;
                }

                ImageTargetBehaviour observer;
                if (!string.IsNullOrWhiteSpace(target.databaseName) &&
                    !string.IsNullOrWhiteSpace(target.databaseTargetName))
                {
                    observer = factory.CreateImageTarget(target.databaseName, target.databaseTargetName);
                }
                else if (target.image != null)
                {
                    observer = factory.CreateImageTarget(target.image, target.physicalWidth, target.content.name);
                }
                else
                {
                    continue;
                }

                target.content.transform.SetParent(observer.transform, false);
                target.content.transform.localPosition = Vector3.zero;
                target.content.transform.localRotation = Quaternion.identity;

                VuforiaTargetStatusBridge bridge = observer.gameObject.AddComponent<VuforiaTargetStatusBridge>();
                bridge.Configure(observer, target.content);
            }

            SetStatus("APUNTA A UNA DE LAS DOS CARTAS");
        }

        private void SetStatus(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.text = message;
            }
        }
    }

    /// <summary>Forwards Vuforia tracking changes to the Pokemon presentation.</summary>
    public sealed class VuforiaTargetStatusBridge : MonoBehaviour
    {
        private ObserverBehaviour observer;
        private PokemonTargetController target;
        private bool previousTracked;

        public void Configure(ObserverBehaviour observerBehaviour, PokemonTargetController targetController)
        {
            observer = observerBehaviour;
            target = targetController;
        }

        private void Update()
        {
            if (observer == null || target == null)
            {
                return;
            }

            Status status = observer.TargetStatus.Status;
            // Keep the model visible only while the printed card itself is detected.
            bool tracked = status == Status.TRACKED;
            if (tracked == previousTracked)
            {
                return;
            }

            previousTracked = tracked;
            target.SetTracked(tracked);
        }
    }
}
