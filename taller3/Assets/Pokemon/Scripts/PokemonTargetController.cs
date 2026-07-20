using System.Collections;
using UnityEngine;

namespace PokedexAR
{
    /// <summary>
    /// Owns an Image Target's evolution line, switches visible models and opens the Pokedex.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class PokemonTargetController : MonoBehaviour
    {
        [SerializeField, Tooltip("Ordered Pokemon stages from base form to final evolution.")]
        private PokemonStage[] evolutionStages;

        [SerializeField, Tooltip("Shared responsive information panel.")]
        private PokedexPanelController pokedexPanel;

        [SerializeField, Tooltip("Printed card preview hidden when Vuforia supplies a physical target.")]
        private GameObject previewSurface;

        [SerializeField, Tooltip("Whether this target is available before a Vuforia tracking event.")]
        private bool simulateTracking = true;

        private AudioSource audioSource;
        private int currentStageIndex;
        private bool isTracked;

        /// <summary>The target that receives swipe navigation.</summary>
        public static PokemonTargetController CurrentTarget { get; private set; }

        /// <summary>Returns the currently visible evolution stage.</summary>
        public PokemonStage CurrentStage => evolutionStages[currentStageIndex];

        /// <summary>Assigns the data and references generated for this Image Target.</summary>
        public void Configure(
            PokemonStage[] stages,
            PokedexPanelController panel,
            GameObject cardPreview,
            bool enableSimulation)
        {
            evolutionStages = stages;
            pokedexPanel = panel;
            previewSurface = cardPreview;
            simulateTracking = enableSimulation;
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.15f;
            isTracked = simulateTracking;
            ApplyStage(false);
        }

        private void OnEnable()
        {
            if (CurrentTarget == null)
            {
                CurrentTarget = this;
            }
        }

        private void OnDisable()
        {
            if (CurrentTarget == this)
            {
                CurrentTarget = null;
            }
        }

        /// <summary>Makes this target the recipient of the next swipe gesture.</summary>
        public void SelectAsCurrent()
        {
            CurrentTarget = this;
        }

        /// <summary>Moves forward or backward through the evolution line.</summary>
        /// <param name="direction">Positive selects the next evolution; negative selects the previous.</param>
        public void Navigate(int direction)
        {
            if (!isTracked || evolutionStages == null || evolutionStages.Length == 0)
            {
                return;
            }

            SelectAsCurrent();
            int nextIndex = Mathf.Clamp(currentStageIndex + direction, 0, evolutionStages.Length - 1);
            if (nextIndex == currentStageIndex)
            {
                StartCoroutine(NudgeCurrentModel());
                return;
            }

            currentStageIndex = nextIndex;
            ApplyStage(pokedexPanel.IsVisible);
        }

        /// <summary>Handles a precise raycast tap on the active Pokemon.</summary>
        public void InspectCurrent()
        {
            if (!isTracked)
            {
                return;
            }

            SelectAsCurrent();
            PlayProceduralCry(CurrentStage);
            pokedexPanel.Show(CurrentStage);
        }

        /// <summary>Receives tracking state from Vuforia or the editor preview.</summary>
        public void SetTracked(bool tracked)
        {
            isTracked = tracked || simulateTracking;
            foreach (PokemonStage stage in evolutionStages)
            {
                if (stage.modelObject != null)
                {
                    stage.modelObject.SetActive(isTracked && stage == CurrentStage);
                }
            }

            if (tracked)
            {
                SelectAsCurrent();
            }
        }

        /// <summary>Switches from the printed-card preview to Vuforia's physical target.</summary>
        public void EnableVuforiaMode()
        {
            simulateTracking = false;
            isTracked = false;
            if (previewSurface != null)
            {
                previewSurface.SetActive(false);
            }

            SetTracked(false);
        }

        private void ApplyStage(bool refreshPanel)
        {
            if (evolutionStages == null || evolutionStages.Length == 0)
            {
                return;
            }

            for (int index = 0; index < evolutionStages.Length; index++)
            {
                GameObject stageObject = evolutionStages[index].modelObject;
                if (stageObject != null)
                {
                    stageObject.SetActive(isTracked && index == currentStageIndex);
                }
            }

            if (refreshPanel)
            {
                pokedexPanel.Show(CurrentStage);
            }
        }

        private void PlayProceduralCry(PokemonStage stage)
        {
            const int sampleRate = 22050;
            const float duration = 0.28f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float baseFrequency = 320f * stage.cryPitch;

            for (int index = 0; index < sampleCount; index++)
            {
                float time = (float)index / sampleRate;
                float envelope = Mathf.Pow(1f - (float)index / sampleCount, 2f);
                float chirp = baseFrequency + 620f * time;
                samples[index] = Mathf.Sin(2f * Mathf.PI * chirp * time) * envelope * 0.22f;
            }

            AudioClip cry = AudioClip.Create($"{stage.displayName}_Cry", sampleCount, 1, sampleRate, false);
            cry.SetData(samples, 0);
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(cry);
        }

        private IEnumerator NudgeCurrentModel()
        {
            Transform model = CurrentStage.modelObject.transform;
            Vector3 originalScale = model.localScale;
            float elapsed = 0f;

            while (elapsed < 0.18f)
            {
                elapsed += Time.deltaTime;
                float pulse = 1f + Mathf.Sin(elapsed / 0.18f * Mathf.PI) * 0.06f;
                model.localScale = originalScale * pulse;
                yield return null;
            }

            model.localScale = originalScale;
        }
    }
}
