using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ufo : MonoBehaviour
{
    [Header("Beam & alien")]
    [SerializeField] private GameObject beam;          // child of the UFO, starts off
    [SerializeField] private Transform beamBottom;     // empty child at the base of the beam
    [SerializeField] private GameObject alienPrefab;   // project asset — fine on a prefab
    [SerializeField] private float flySpeed = 6f;
    [SerializeField] private float beamDuration = 2f;       // beam glows before the alien appears
    [SerializeField] private float dispatchDuration = 1.5f; // alien held in the beam before it fades
    [SerializeField] private float fadeDuration = 1f;       // how long the alien takes to fade out
    [SerializeField] private float stayDuration = 2f;       // how long the ship hovers after, then leaves

    private Vector3 stopPos, exitPos;

    // Manager calls this right after spawning, handing in the scene positions
    public void Launch(Vector3 stop, Vector3 exit)
    {
        stopPos = stop;
        exitPos = exit;
        if (beam != null) beam.SetActive(false);
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        yield return MoveTo(transform, stopPos, flySpeed);          // fly in

        if (beam != null) beam.SetActive(true);                    // beam on
        yield return new WaitForSeconds(beamDuration);             // ...glows for this long

        GameObject alien = null;                                   // drop alien at the beam's base
        if (alienPrefab != null)
        {
            Vector3 spawnPos = beamBottom != null ? beamBottom.position : stopPos;
            alien = Instantiate(alienPrefab, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(dispatchDuration);         // alien sits in the beam
        if (beam != null) beam.SetActive(false);                   // beam off

        // Alien fades out where it dropped, then a random crew member is possessed
        if (alien != null)
        {
            yield return FadeOut(alien);
            Destroy(alien);
        }
        PossessRandomInnocent();

        yield return new WaitForSeconds(stayDuration);             // ship hovers this long...
        yield return MoveTo(transform, exitPos, flySpeed);         // ...then flies away

        Destroy(gameObject);                                       // clean up the UFO
    }

    IEnumerator FadeOut(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color c = sr.color;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            sr.color = c;
            yield return null;
        }
    }

    void PossessRandomInnocent()
    {
        List<NPCGatherer> innocents = new List<NPCGatherer>();
        foreach (NPCGatherer npc in FindObjectsByType<NPCGatherer>(FindObjectsSortMode.None))
            if (!npc.IsImpostor)
                innocents.Add(npc);

        if (innocents.Count == 0)
        {
            Debug.Log("Alien found no innocent to possess.");
            return;
        }

        NPCGatherer victim = innocents[Random.Range(0, innocents.Count)];
        victim.SetImpostor(true);
        Debug.Log(victim.name + " has been possessed — now an impostor!");
    }

    IEnumerator MoveTo(Transform t, Vector3 target, float speed)
    {
        while (Vector3.Distance(t.position, target) > 0.05f)
        {
            t.position = Vector3.MoveTowards(t.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }
}
