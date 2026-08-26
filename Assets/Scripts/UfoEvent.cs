using System.Collections;
using UnityEngine;

public class UfoEvent : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private int spawnOnNight = 1;      // which night the UFO arrives

    [Header("Movement")]
    [SerializeField] private Transform offscreenPoint;  // where it starts / flies back to
    [SerializeField] private Transform stopPoint;       // where it hovers over camp
    [SerializeField] private float flySpeed = 6f;

    [Header("Beam & alien")]
    [SerializeField] private GameObject beam;           // beam light/sprite (starts OFF)
    [SerializeField] private GameObject alienPrefab;    // alien that drops out
    [SerializeField] private Transform campTarget;      // where the alien walks (the bonfire)
    [SerializeField] private float alienSpeed = 2f;
    [SerializeField] private float beamDuration = 2f;

    private bool triggered;

    void Start()
    {
        if (beam != null) beam.SetActive(false);
        if (offscreenPoint != null) transform.position = offscreenPoint.position;
        if (cycle != null) cycle.OnNightStart += OnNight;
    }

    void OnDestroy()
    {
        if (cycle != null) cycle.OnNightStart -= OnNight;   // clean up the subscription
    }

    void OnNight()
    {   
        Debug.Log("UFO OnNight fired, DayNumber = " + cycle.DayNumber);
        if (!triggered && cycle.DayNumber == spawnOnNight)
        {
            triggered = true;
            StartCoroutine(Sequence());
        }
    }

    IEnumerator Sequence()
    {
        yield return MoveTo(transform, stopPoint.position, flySpeed);   // 1. fly in

        if (beam != null) beam.SetActive(true);                         // 2. beam on
        yield return new WaitForSeconds(beamDuration);

        GameObject alien = null;                                        // 3. drop alien
        if (alienPrefab != null)
            alien = Instantiate(alienPrefab, stopPoint.position, Quaternion.identity);

        if (alien != null && campTarget != null)                       // 4. alien walks to camp
            yield return MoveTo(alien.transform, campTarget.position, alienSpeed);

        if (alien != null) Destroy(alien);                             // 5. alien "disguises" (joins)

        if (beam != null) beam.SetActive(false);                       // 6. beam off, fly away
        yield return MoveTo(transform, offscreenPoint.position, flySpeed);
    }

    IEnumerator MoveTo(Transform t, Vector3 target, float speed)
    {
        while (Vector3.Distance(t.position, target) > 0.05f)
        {
            t.position = Vector3.MoveTowards(t.position, target, speed * Time.deltaTime);
            yield return null;   // wait one frame, then keep moving
        }
    }
}