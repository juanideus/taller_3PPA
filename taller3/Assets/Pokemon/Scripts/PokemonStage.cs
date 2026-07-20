using System;
using UnityEngine;

namespace PokedexAR
{
    /// <summary>
    /// Describes one creature in an evolution line and the scene object that represents it.
    /// </summary>
    [Serializable]
    public sealed class PokemonStage
    {
        [Tooltip("Name shown in the Pokedex.")]
        public string displayName;

        [Tooltip("Pokemon type or type combination.")]
        public string type;

        [TextArea(3, 6)]
        [Tooltip("Short description shown in the Pokedex.")]
        public string description;

        [Tooltip("Scene object containing this stage's imported 3D model.")]
        public GameObject modelObject;

        [Tooltip("Accent used by the information panel and generated cry.")]
        public Color accentColor = Color.white;

        [Range(0.5f, 2f)]
        [Tooltip("Pitch multiplier used by the procedural cry.")]
        public float cryPitch = 1f;
    }
}
