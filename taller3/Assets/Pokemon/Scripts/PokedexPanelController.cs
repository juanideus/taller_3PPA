using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PokedexAR
{
    /// <summary>
    /// Presents the selected Pokemon information and animates the panel in and out.
    /// </summary>
    public sealed class PokedexPanelController : MonoBehaviour
    {
        [SerializeField, Tooltip("Canvas group that controls the complete information surface.")]
        private CanvasGroup panelGroup;

        [SerializeField, Tooltip("Text label used for the Pokemon name.")]
        private Text nameLabel;

        [SerializeField, Tooltip("Text label used for the Pokemon type.")]
        private Text typeLabel;

        [SerializeField, Tooltip("Text label used for the Pokemon description.")]
        private Text descriptionLabel;

        [SerializeField, Tooltip("Decorative stripe tinted with the current Pokemon accent.")]
        private Image accentStripe;

        [SerializeField, Tooltip("Button that closes the Pokedex panel.")]
        private Button closeButton;

        private Coroutine transition;
        private bool isVisible;

        /// <summary>Returns true while the information panel is open.</summary>
        public bool IsVisible => isVisible;

        /// <summary>Assigns UI references created by the editor scene builder.</summary>
        public void Configure(
            CanvasGroup group,
            Text pokemonName,
            Text pokemonType,
            Text pokemonDescription,
            Image stripe,
            Button close)
        {
            panelGroup = group;
            nameLabel = pokemonName;
            typeLabel = pokemonType;
            descriptionLabel = pokemonDescription;
            accentStripe = stripe;
            closeButton = close;
        }

        private void Awake()
        {
            closeButton.onClick.AddListener(Hide);
            SetImmediate(false);
        }

        /// <summary>Shows information for the supplied evolution stage.</summary>
        public void Show(PokemonStage stage)
        {
            nameLabel.text = stage.displayName.ToUpperInvariant();
            typeLabel.text = $"TIPO  {stage.type.ToUpperInvariant()}";
            descriptionLabel.text = stage.description;
            accentStripe.color = stage.accentColor;
            isVisible = true;
            BeginTransition(1f);
        }

        /// <summary>Closes the panel while keeping the AR experience active.</summary>
        public void Hide()
        {
            isVisible = false;
            BeginTransition(0f);
        }

        private void BeginTransition(float targetAlpha)
        {
            if (transition != null)
            {
                StopCoroutine(transition);
            }

            gameObject.SetActive(true);
            transition = StartCoroutine(FadeTo(targetAlpha));
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            float startAlpha = panelGroup.alpha;
            float elapsed = 0f;
            const float duration = 0.18f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3f);
                panelGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            panelGroup.alpha = targetAlpha;
            panelGroup.blocksRaycasts = targetAlpha > 0.99f;
            panelGroup.interactable = targetAlpha > 0.99f;
            transition = null;
        }

        private void SetImmediate(bool visible)
        {
            isVisible = visible;
            panelGroup.alpha = visible ? 1f : 0f;
            panelGroup.blocksRaycasts = visible;
            panelGroup.interactable = visible;
        }
    }
}
