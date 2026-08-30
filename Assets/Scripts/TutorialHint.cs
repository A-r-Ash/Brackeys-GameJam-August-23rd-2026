using System.Collections;
using UnityEngine;
using TMPro;

// A trigger zone that shows a one-time hint when the player enters.
// Reusable for any "explore to discover" lesson (traps, 2nd camp, etc.).
public class TutorialHint : MonoBehaviour
{
    [SerializeField, TextArea] private string message;
    [SerializeField] private TMP_Text hintText;   // shared screen-space hint label (starts disabled)
    [SerializeField] private float duration = 5f;
    [SerializeField] private bool once = true;

    private bool shown;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerInteract>() == null) return;   // players only
        if (once && shown) return;

        shown = true;
        StopAllCoroutines();
        StartCoroutine(ShowHint());
    }

    IEnumerator ShowHint()
    {
        if (hintText == null) yield break;

        hintText.text = message;
        hintText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        hintText.gameObject.SetActive(false);
    }
}
