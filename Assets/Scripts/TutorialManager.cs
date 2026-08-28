using UnityEngine;
using TMPro;

// A simple step machine: show one prompt, wait for the player to do it, advance.
public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private CanvasGroup canvasGroup;   // fades the whole tutorial UI in/out
    [SerializeField] private float fadeSpeed = 2f;

    [Header("Refs for step conditions")]
    [SerializeField] private PlayerInteract player;
    [SerializeField] private FoodPile foodPile;
    [SerializeField] private WoodPile woodPile;
    [SerializeField] private RecruitPoint recruitPoint;
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private Bonfire bonfire;
    [SerializeField] private WreckedShip ship;
    [SerializeField] private int recruitCost = 3;

    [Header("Highlight arrow")]
    [SerializeField] private GameObject highlight;      // arrow that points down at the current target
    [SerializeField] private float aboveTarget = 1.2f;  // how high the arrow floats above it
    [SerializeField] private float bobHeight = 0.25f;   // how far it bobs up/down
    [SerializeField] private float bobSpeed = 2f;       // how fast it bobs

    private enum Step
    {
        Move,
        CollectBerry,
        DepositFood,
        Recruit,
        WatchGather,
        GrabWood,
        FeedFire,
        RepairShip,
        SurviveToNight,
        AlienIntro,
        Done
    }

    private Step step = Step.Move;
    private int npcBaseline;
    private int woodBaseline;
    private int fedBaseline;
    private bool wDone, aDone, sDone, dDone;

    [SerializeField] private float alienIntroDuration = 10f;   // how long the alien warning stays up
    private float alienTimer;

    void Start()
    {
        npcBaseline  = NPCGatherer.Count;
        woodBaseline = woodPile != null ? woodPile.Count : 0;
        if (cycle != null) cycle.SetPaused(true);   // hold daytime until we have wood
        ShowPrompt();
    }

    void Update()
    {
        if (step == Step.Move)   // record which movement keys have been pressed
        {
            if (Input.GetKey(KeyCode.W)) wDone = true;
            if (Input.GetKey(KeyCode.A)) aDone = true;
            if (Input.GetKey(KeyCode.S)) sDone = true;
            if (Input.GetKey(KeyCode.D)) dDone = true;
        }

        // Hold daytime until the crew has stocked at least 1 wood, then flip to night
        if (cycle != null && cycle.IsPaused && woodPile != null && woodPile.Count >= 1)
            cycle.ForceNight();

        if (step == Step.AlienIntro) alienTimer -= Time.deltaTime;

        if (step != Step.Done && IsStepComplete()) Advance();

        UpdateHighlight();
        UpdateFade();
    }

    void UpdateFade()
    {
        if (canvasGroup == null) return;

        // Hide during passive waits (waiting for night 2) and once finished/skipped
        bool show = step != Step.Done && step != Step.SurviveToNight;
        float target = show ? 1f : 0f;

        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, fadeSpeed * Time.deltaTime);
        canvasGroup.interactable   = show;   // invisible buttons don't intercept clicks
        canvasGroup.blocksRaycasts = show;
    }

    void UpdateHighlight()
    {
        if (highlight == null) return;

        Transform target = CurrentTarget();
        highlight.SetActive(target != null);
        if (target != null)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            highlight.transform.position = target.position + Vector3.up * (aboveTarget + bob);
        }
    }

    // Where the yellow circle should sit this step (null = hide it)
    Transform CurrentTarget()
    {
        switch (step)
        {
            case Step.CollectBerry: return NearestBush();
            case Step.DepositFood:  return foodPile != null ? foodPile.transform : null;
            case Step.Recruit:      return recruitPoint != null ? recruitPoint.transform : null;
            case Step.GrabWood:     return woodPile != null ? woodPile.transform : null;
            case Step.FeedFire:     return bonfire != null ? bonfire.transform : null;
            case Step.RepairShip:   return ship != null ? ship.transform : null;
            default:                return null;   // Move / WatchGather / AlienIntro / Done → no arrow
        }
    }

    Transform NearestBush()
    {
        Vector3 from = player != null ? player.transform.position : transform.position;
        Transform best = null;
        float bestD = Mathf.Infinity;

        foreach (BerryBush b in FindObjectsByType<BerryBush>(FindObjectsSortMode.None))
        {
            if (!b.HasBerries) continue;                       // skip depleted/hidden ones
            float d = Vector2.Distance(from, b.transform.position);
            if (d < bestD) { bestD = d; best = b.transform; }
        }
        return best;
    }

    // Hook to a "Next" button — jumps to the next step
    public void SkipStep()
    {
        if (step == Step.Done) return;
        Advance();
        if (cycle != null && cycle.IsPaused && step >= Step.GrabWood)
            cycle.ForceNight();   // if we skipped into the night portion, roll to night
    }

    // Hook to a "Skip tutorial" button — ends it and hands off to free play
    public void SkipTutorial()
    {
        if (cycle != null) cycle.SetPaused(false);   // let day/night run normally
        step = Step.Done;
        ShowPrompt();                                // clears the prompt
        if (highlight != null) highlight.SetActive(false);
    }

    bool IsStepComplete()
    {
        switch (step)
        {
            case Step.Move:         return wDone && aDone && sDone && dDone;
            case Step.CollectBerry: return player != null && player.CarriedFood > 0;
            case Step.DepositFood:  return foodPile != null && foodPile.Count >= recruitCost;
            case Step.Recruit:      return NPCGatherer.Count > npcBaseline;
            case Step.WatchGather:  return woodPile != null && woodPile.Count > woodBaseline;
            case Step.GrabWood:     return player != null && player.CarriedWood > 0;
            case Step.FeedFire:     return bonfire != null && bonfire.TimesFed > fedBaseline;
            case Step.RepairShip:     return ship != null && ship.WoodDeposited > 0;
            case Step.SurviveToNight: return cycle != null && cycle.IsNight && cycle.DayNumber >= 2;
            case Step.AlienIntro:     return alienTimer <= 0f;
            default: return false;
        }
    }

    void Advance()
    {
        // snapshot baselines the moment a step begins so the NEXT step measures a change
        if (step == Step.DepositFood) npcBaseline  = NPCGatherer.Count;
        if (step == Step.Recruit)     woodBaseline = woodPile != null ? woodPile.Count : 0;
        if (step == Step.GrabWood)    fedBaseline  = bonfire != null ? bonfire.TimesFed : 0;
        if (step == Step.FeedFire && woodPile != null) woodPile.AddWood(5);   // stock some wood for the ship
        if (step == Step.SurviveToNight) alienTimer = alienIntroDuration;     // alien warning shows on night 2

        step++;
        ShowPrompt();
    }

    void ShowPrompt()
    {
        if (promptText == null) return;

        switch (step)
        {
            case Step.Move:         promptText.text = "Use W A S D to move around"; break;
            case Step.CollectBerry: promptText.text = "Find a berry bush and press E to pick a berry"; break;
            case Step.DepositFood:  promptText.text = "Carry the food to the food pile and press E to drop it in"; break;
            case Step.Recruit:      promptText.text = "Go to the totem and press E to recruit a helper (costs food)"; break;
            case Step.WatchGather:  promptText.text = "Nice! Your helper gathers wood on its own — watch the wood pile grow"; break;
            case Step.GrabWood:     promptText.text = "Night is falling! Grab wood from the pile (press E)"; break;
            case Step.FeedFire:     promptText.text = "Take the wood to the bonfire and press E to keep it burning"; break;
            case Step.RepairShip:     promptText.text = "That shipwreck is your way out — bring wood to it to start repairs"; break;
            case Step.SurviveToNight: promptText.text = "Repair the ship by day, feed the fire by night. Survive until night falls..."; break;
            case Step.AlienIntro:     promptText.text = "An alien turned one of your crew into a thief! Hover over an NPC and LEFT-CLICK to eliminate them — you can eliminate ANYONE, so be careful!"; break;
            case Step.Done:           promptText.text = ""; break;
        }
    }
}
