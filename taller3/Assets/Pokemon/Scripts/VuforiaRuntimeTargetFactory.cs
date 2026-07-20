using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace PokedexAR
{
    /// <summary>
    /// Creates Vuforia 11 instant Image Targets through reflection when the optional SDK is present.
    /// The project therefore remains compilable before the account-bound Vuforia package is imported.
    /// </summary>
    public sealed class VuforiaRuntimeTargetFactory : MonoBehaviour
    {
        [Serializable]
        public sealed class InstantTarget
        {
            [Tooltip("Readable texture used by Vuforia as an instant Image Target.")]
            public Texture2D image;

            [Tooltip("Real-world printed width of the target in metres.")]
            public float physicalWidth = 0.12f;

            [Tooltip("Evolution content that is parented to the detected target.")]
            public PokemonTargetController content;
        }

        [SerializeField, Tooltip("AR camera that receives VuforiaBehaviour when the SDK is installed.")]
        private Camera arCamera;

        [SerializeField, Tooltip("Two printable targets and their evolution content.")]
        private InstantTarget[] targets;

        [SerializeField, Tooltip("Status label that explains whether live tracking or preview mode is active.")]
        private UnityEngine.UI.Text statusLabel;

        /// <summary>Assigns target textures and their corresponding content roots.</summary>
        public void Configure(Camera camera, InstantTarget[] instantTargets, UnityEngine.UI.Text status)
        {
            arCamera = camera;
            targets = instantTargets;
            statusLabel = status;
        }

        private IEnumerator Start()
        {
            Type vuforiaBehaviourType = FindType("Vuforia.VuforiaBehaviour");
            if (vuforiaBehaviourType == null)
            {
                statusLabel.text = "MODO DEMO  |  Instala Vuforia 11.4.4 para seguimiento AR";
                yield break;
            }

            Component behaviour = arCamera.GetComponent(vuforiaBehaviourType);
            if (behaviour == null)
            {
                behaviour = arCamera.gameObject.AddComponent(vuforiaBehaviourType);
            }

            statusLabel.text = "INICIANDO CÁMARA AR...";
            object instance = null;
            object observerFactory = null;
            float timeout = Time.realtimeSinceStartup + 12f;

            while (observerFactory == null && Time.realtimeSinceStartup < timeout)
            {
                PropertyInfo instanceProperty = vuforiaBehaviourType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                instance = instanceProperty?.GetValue(null);
                observerFactory = instance?.GetType().GetProperty("ObserverFactory")?.GetValue(instance);
                yield return new WaitForSecondsRealtime(0.25f);
            }

            if (observerFactory == null)
            {
                statusLabel.text = "Vuforia requiere una clave de licencia válida";
                yield break;
            }

            MethodInfo createTarget = FindTextureTargetMethod(observerFactory.GetType());
            if (createTarget == null)
            {
                statusLabel.text = "La versión instalada de Vuforia no admite Instant Targets";
                yield break;
            }

            foreach (InstantTarget target in targets)
            {
                object observer = createTarget.Invoke(observerFactory, new object[]
                {
                    target.image,
                    target.physicalWidth,
                    target.content.name
                });

                if (observer is Component observerComponent)
                {
                    target.content.EnableVuforiaMode();
                    target.content.transform.SetParent(observerComponent.transform, false);
                    target.content.transform.localPosition = Vector3.zero;
                    target.content.transform.localRotation = Quaternion.identity;
                    VuforiaTargetStatusBridge bridge = observerComponent.gameObject.AddComponent<VuforiaTargetStatusBridge>();
                    bridge.Configure(observerComponent, target.content);
                }
            }

            statusLabel.text = "APUNTA A UNA DE LAS DOS CARTAS";
        }

        private static MethodInfo FindTextureTargetMethod(Type factoryType)
        {
            foreach (MethodInfo method in factoryType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (method.Name == "CreateImageTarget" &&
                    parameters.Length == 3 &&
                    parameters[0].ParameterType == typeof(Texture2D) &&
                    parameters[1].ParameterType == typeof(float) &&
                    parameters[2].ParameterType == typeof(string))
                {
                    return method;
                }
            }

            return null;
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }

    /// <summary>Polls a reflected Vuforia observer and forwards its tracked state to gameplay.</summary>
    public sealed class VuforiaTargetStatusBridge : MonoBehaviour
    {
        private Component observer;
        private PokemonTargetController target;
        private PropertyInfo targetStatusProperty;
        private bool previousTracked;

        /// <summary>Connects a Vuforia observer component with its Pokemon content.</summary>
        public void Configure(Component observerComponent, PokemonTargetController targetController)
        {
            observer = observerComponent;
            target = targetController;
            targetStatusProperty = observer.GetType().GetProperty("TargetStatus");
        }

        private void Update()
        {
            object targetStatus = targetStatusProperty?.GetValue(observer);
            object status = targetStatus?.GetType().GetProperty("Status")?.GetValue(targetStatus);
            string statusName = status?.ToString() ?? string.Empty;
            bool tracked = statusName == "TRACKED" || statusName == "EXTENDED_TRACKED" || statusName == "LIMITED";

            if (tracked != previousTracked)
            {
                previousTracked = tracked;
                target.SetTracked(tracked);
            }
        }
    }
}
