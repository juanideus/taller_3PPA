using UnityEngine;

namespace PokedexAR
{
    /// <summary>Keeps interface controls clear of notches and mobile gesture areas.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            Vector2 minimum = safeArea.position;
            Vector2 maximum = safeArea.position + safeArea.size;
            minimum.x /= Screen.width;
            minimum.y /= Screen.height;
            maximum.x /= Screen.width;
            maximum.y /= Screen.height;
            rectTransform.anchorMin = minimum;
            rectTransform.anchorMax = maximum;
        }
    }
}
