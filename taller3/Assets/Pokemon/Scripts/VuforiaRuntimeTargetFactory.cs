using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

namespace PokedexAR
{
    /// <summary>Creates the runtime Image Targets once Vuforia is initialized.</summary>
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

            [Tooltip("Scale applied to the normalized model so it fits on the physical card.")]
            public float contentScale = 0.075f;

            [Tooltip("Pokemon evolution content attached to the detected target.")]
            public PokemonTargetController content;
        }

        [SerializeField] private Camera arCamera;
        [SerializeField] private InstantTarget[] targets;
        [SerializeField] private Text statusLabel;

        private bool initializationFinished;
        private VuforiaInitError initializationError = VuforiaInitError.NONE;
        private VuforiaEngineError runtimeError;
        private bool hasRuntimeError;

        public void Configure(Camera camera, InstantTarget[] instantTargets, Text status)
        {
            arCamera = camera;
            targets = instantTargets;
            statusLabel = status;
        }

        private void Awake()
        {
            HideDemoEnvironment();

            VuforiaApplication.Instance.OnVuforiaInitialized += HandleVuforiaInitialized;
            VuforiaApplication.Instance.OnVuforiaError += HandleVuforiaError;

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

            VuforiaBehaviour behaviour = arCamera.GetComponent<VuforiaBehaviour>();
            if (behaviour == null)
            {
                behaviour = arCamera.gameObject.AddComponent<VuforiaBehaviour>();
            }

            behaviour.enabled = true;

            SetStatus("INICIANDO CAMARA AR...");
            yield return null;

            // Auto-initialization can run before the first scene is ready on Android.
            // Calling Initialize is idempotent and also recovers when that startup was skipped.
            if (!VuforiaApplication.Instance.IsInitialized)
            {
                VuforiaApplication.Instance.Initialize();
            }

            float timeout = Time.realtimeSinceStartup + 30f;
            while (!initializationFinished &&
                   !VuforiaApplication.Instance.IsInitialized &&
                   Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            if (initializationError != VuforiaInitError.NONE)
            {
                SetStatus(GetInitializationErrorMessage(initializationError));
                yield break;
            }

            if (!VuforiaApplication.Instance.IsInitialized)
            {
                SetStatus("NO SE PUDO INICIAR AR. REVISA CAMARA E INTERNET");
                yield break;
            }

            float runningTimeout = Time.realtimeSinceStartup + 10f;
            while (!VuforiaApplication.Instance.IsRunning && Time.realtimeSinceStartup < runningTimeout)
            {
                yield return null;
            }

            if (!VuforiaApplication.Instance.IsRunning || VuforiaBehaviour.Instance == null)
            {
                SetStatus(hasRuntimeError
                    ? $"ERROR VUFORIA: {runtimeError}"
                    : "NO SE PUDO ABRIR LA CAMARA AR");
                yield break;
            }

            ObserverFactory factory = VuforiaBehaviour.Instance.ObserverFactory;
            int createdTargetCount = 0;
            foreach (InstantTarget target in targets)
            {
                if (target.content == null)
                {
                    continue;
                }

                ImageTargetBehaviour observer;
                // Instant targets proved more reliable across Android devices. The source
                // textures are the exact images exported with the taller_3 database.
                if (target.image != null)
                {
                    observer = factory.CreateImageTarget(target.image, target.physicalWidth, target.content.name);
                }
                else if (!string.IsNullOrWhiteSpace(target.databaseName) &&
                         !string.IsNullOrWhiteSpace(target.databaseTargetName))
                {
                    observer = factory.CreateImageTarget(target.databaseName, target.databaseTargetName);
                }
                else
                {
                    continue;
                }

                if (observer == null)
                {
                    SetStatus($"NO SE PUDO CARGAR TARGET {target.databaseTargetName}");
                    continue;
                }

                target.content.transform.SetParent(observer.transform, false);
                target.content.transform.localPosition = Vector3.up * 0.01f;
                // Runtime Image Targets lie on XZ. Rotate the normalized Pokemon so its
                // vertical axis follows the printed card and its front faces the camera.
                target.content.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                target.content.transform.localScale = Vector3.one * Mathf.Max(0.001f, target.contentScale);

                VuforiaTargetStatusBridge bridge = observer.gameObject.AddComponent<VuforiaTargetStatusBridge>();
                string targetName = string.IsNullOrWhiteSpace(target.databaseTargetName)
                    ? target.content.name
                    : target.databaseTargetName;
                bridge.Configure(observer, target.content, targetName, SetStatus);
                createdTargetCount++;
            }

            SetStatus(createdTargetCount > 0
                ? "APUNTA A UNA DE LAS TRES CARTAS"
                : "NO SE CARGARON LOS TARGETS DE TALLER_3");
        }

        private void OnDestroy()
        {
            if (VuforiaApplication.Instance == null)
            {
                return;
            }

            VuforiaApplication.Instance.OnVuforiaInitialized -= HandleVuforiaInitialized;
            VuforiaApplication.Instance.OnVuforiaError -= HandleVuforiaError;
        }

        private void HandleVuforiaInitialized(VuforiaInitError error)
        {
            initializationError = error;
            initializationFinished = true;
        }

        private void HandleVuforiaError(VuforiaEngineError error)
        {
            runtimeError = error;
            hasRuntimeError = true;
            SetStatus($"ERROR VUFORIA: {error}");
        }

        private static string GetInitializationErrorMessage(VuforiaInitError error)
        {
            switch (error)
            {
                case VuforiaInitError.PERMISSION_ERROR:
                    return "ACTIVA EL PERMISO DE CAMARA EN AJUSTES";
                case VuforiaInitError.LICENSE_CONFIG_MISSING_KEY:
                case VuforiaInitError.LICENSE_CONFIG_INVALID_KEY:
                case VuforiaInitError.LICENSE_ERROR:
                    return "REVISA LA LICENCIA DE VUFORIA";
                case VuforiaInitError.DEVICE_NOT_SUPPORTED:
                    return "ESTE TELEFONO NO ES COMPATIBLE CON VUFORIA";
                default:
                    return $"ERROR AL INICIAR VUFORIA: {error}";
            }
        }

        private void SetStatus(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.text = message;
            }
        }

        private static void HideDemoEnvironment()
        {
            GameObject demoSurface = GameObject.Find("Demo Surface");
            if (demoSurface != null)
            {
                demoSurface.SetActive(false);
            }

            for (int index = 1; index <= 7; index++)
            {
                GameObject gridLine = GameObject.Find($"Grid Line {index}");
                if (gridLine != null)
                {
                    gridLine.SetActive(false);
                }
            }

            GameObject selector = GameObject.Find("Target Selector");
            if (selector != null)
            {
                selector.SetActive(false);
            }
        }
    }

    /// <summary>Forwards Vuforia tracking changes to the Pokemon presentation.</summary>
    public sealed class VuforiaTargetStatusBridge : MonoBehaviour
    {
        private static VuforiaTargetStatusBridge activeBridge;

        private ObserverBehaviour observer;
        private PokemonTargetController target;
        private string targetName;
        private Action<string> setStatus;
        private bool previousTracked;

        public void Configure(
            ObserverBehaviour observerBehaviour,
            PokemonTargetController targetController,
            string observerName,
            Action<string> statusCallback)
        {
            observer = observerBehaviour;
            target = targetController;
            targetName = observerName;
            setStatus = statusCallback;

            observer.OnTargetStatusChanged += HandleTargetStatusChanged;
            ApplyStatus(observer.TargetStatus);
        }

        private void OnDestroy()
        {
            if (observer != null)
            {
                observer.OnTargetStatusChanged -= HandleTargetStatusChanged;
            }

            if (activeBridge == this)
            {
                activeBridge = null;
            }
        }

        private void HandleTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
        {
            ApplyStatus(targetStatus);
        }

        private void ApplyStatus(TargetStatus targetStatus)
        {
            if (target == null)
            {
                return;
            }

            Status status = targetStatus.Status;
            bool tracked = status == Status.TRACKED;
            if (tracked == previousTracked)
            {
                return;
            }

            if (tracked)
            {
                if (activeBridge != null && activeBridge != this)
                {
                    activeBridge.ForceHide();
                }

                activeBridge = this;
                previousTracked = true;
                target.SetTracked(true);
                setStatus?.Invoke($"TARGET {targetName.ToUpperInvariant()} DETECTADO");
                return;
            }

            ForceHide();
            if (activeBridge == this)
            {
                activeBridge = null;
            }
        }

        private void ForceHide()
        {
            previousTracked = false;
            target?.SetTracked(false);
        }
    }
}
