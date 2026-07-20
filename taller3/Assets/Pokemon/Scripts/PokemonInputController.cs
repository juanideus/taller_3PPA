using UnityEngine;
using UnityEngine.EventSystems;

namespace PokedexAR
{
    /// <summary>
    /// Distinguishes horizontal swipe navigation from short raycast taps on 3D models.
    /// </summary>
    public sealed class PokemonInputController : MonoBehaviour
    {
        [SerializeField, Tooltip("Camera used to cast rays into the AR scene.")]
        private Camera interactionCamera;

        [SerializeField, Tooltip("Minimum horizontal movement in pixels at 160 DPI.")]
        private float swipeThreshold = 85f;

        private Vector2 pointerStart;
        private bool pointerStartedOverUi;

        /// <summary>Assigns the camera used by touch and mouse interactions.</summary>
        public void Configure(Camera cameraForRaycasts)
        {
            interactionCamera = cameraForRaycasts;
        }

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                ProcessTouch(Input.GetTouch(0));
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            ProcessMousePreview();
#endif
        }

        private void ProcessTouch(Touch touch)
        {
            if (touch.phase == TouchPhase.Began)
            {
                BeginPointer(touch.position, EventSystem.current.IsPointerOverGameObject(touch.fingerId));
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                EndPointer(touch.position);
            }
        }

        private void ProcessMousePreview()
        {
            if (Input.GetMouseButtonDown(0))
            {
                BeginPointer(Input.mousePosition, EventSystem.current.IsPointerOverGameObject());
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndPointer(Input.mousePosition);
            }
        }

        private void BeginPointer(Vector2 screenPosition, bool isOverUi)
        {
            pointerStart = screenPosition;
            pointerStartedOverUi = isOverUi;
        }

        private void EndPointer(Vector2 screenPosition)
        {
            if (pointerStartedOverUi)
            {
                return;
            }

            Vector2 delta = screenPosition - pointerStart;
            float dpiScale = Screen.dpi > 0f ? Screen.dpi / 160f : 1f;
            float requiredDistance = swipeThreshold * dpiScale;

            if (Mathf.Abs(delta.x) >= requiredDistance && Mathf.Abs(delta.x) > Mathf.Abs(delta.y) * 1.25f)
            {
                PokemonTargetController.CurrentTarget?.Navigate(delta.x < 0f ? 1 : -1);
                return;
            }

            Ray ray = interactionCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                PokemonTargetController target = hit.collider.GetComponentInParent<PokemonTargetController>();
                target?.InspectCurrent();
            }
        }
    }
}
