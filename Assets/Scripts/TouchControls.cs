using UnityEngine;

public class TouchControls : MonoBehaviour
{
   void Awake()
{
    bool showTouch = Application.isMobilePlatform || Input.touchSupported;
#if UNITY_EDITOR
    showTouch = true;   // always visible in the editor for testing
#endif
    gameObject.SetActive(showTouch);
}
}