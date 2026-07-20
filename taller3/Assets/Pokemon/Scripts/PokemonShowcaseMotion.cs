using UnityEngine;

namespace PokedexAR
{
    /// <summary>Adds a subtle idle hover and turn so static imported models feel alive.</summary>
    public sealed class PokemonShowcaseMotion : MonoBehaviour
    {
        [SerializeField, Tooltip("Vertical hover distance in local units.")]
        private float hoverAmplitude = 0.045f;

        [SerializeField, Tooltip("Hover cycles completed each second.")]
        private float hoverFrequency = 0.7f;

        [SerializeField, Tooltip("Slow rotation speed in degrees per second.")]
        private float rotationSpeed = 9f;

        private Vector3 initialLocalPosition;

        private void Awake()
        {
            initialLocalPosition = transform.localPosition;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
            transform.localPosition = initialLocalPosition + Vector3.up * offset;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
