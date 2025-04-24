using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityDirector : MonoBehaviour
{
    private timeManager gTime = new timeManager();
    private SoundManager soundManager;
    private UIManager uiManager;
    private PlayerController playerController;
    private List<timedActivity> activeActivites;


    public List<GameObject> toyPrefabs;
    public List<GameObject> candyPrefabs;
    [SerializeField] private int minToySpawnLocations = 12;
    [SerializeField] private int maxToySpawnLocations = 20;

    [SerializeField] private int maxCandySpawns = 3;

    // (start range) (end range) (time limit)

    private timeLimits windowsTimeLimits = new timeLimits(45.0f, 60.0f, 45.0f);
    private timeLimits petdoorTimeLimits = new timeLimits(50.0f, 65.0f, 45.0f);
    private timeLimits basementTimeLimits = new timeLimits(70.0f, 80.0f, 45.0f);
    private timeLimits fireplaceTimeLimits = new timeLimits(0.0f, 0.0f, 105.0f); // 1:45
    private timeLimits skylightTimeLimits = new timeLimits(85.0f, 95.0f, 45.0f);
    private timeLimits toiletTimeLimits = new timeLimits(60.0f, 90.0f, 45.0f);
    private timeLimits powerOutageTimeLimits = new timeLimits(0.0f, 0.0f, 300.0f);
    private timeLimits phoneRingTimeLimits = new timeLimits(0.0f, 0.0f, 500.0f); // for now

    private timedActivity[] nightActivity;
    private List<activityTrigger> windowEventObjects;
    private activityTrigger petdoorEventObject;
    private activityTrigger basementHatchEventObject;
    private activityTrigger fireplaceEventObject;
    private activityTrigger skylightEventObject;
    private activityTrigger toiletEventObject;
    private timedActivity powerOutageEventObject;
    private timedActivity phoneRingEventObject;

    private List<Vector3> toySpawnLocations;
    private List<Vector3> candySpawnLocations;

    public GameObject[] powerControlGameObjects;
    private GameObject[] thunderControlObjects;

    private int activeNight = 0;

    private bool nightHasStarted = false;

    private timedActivity deathTrigger;

    public GameObject iconPrefab;
    public AudioSource telephoneAudioSource;

    private string deathCause = "Unknown";
    public float DeathTime { get; private set; }


    private int lastActivatedWindow = -1;
    private int lastResetWindow = -1;

    private bool iconsEnabled = true;
    private Color originalAmbientLight;

    void Start()
    {
        originalAmbientLight = RenderSettings.ambientLight;
        uiManager = FindObjectOfType<UIManager>();
        playerController = FindObjectOfType<PlayerController>();
        soundManager = SoundManager.Instance;

        activeActivites = new List<timedActivity>();
        windowEventObjects = new List<activityTrigger>();

        windowsTimeLimits.lastUpdateTime = Time.deltaTime;
        petdoorTimeLimits.lastUpdateTime = windowsTimeLimits.lastUpdateTime;

        nightActivity = new timedActivity[3];
        nightActivity[0] = new timedActivity(420.0f, 0, OnNightStart, OnWin, null);
        nightActivity[1] = new timedActivity(420.0f, 1, OnNightStart, OnWin, null);
        nightActivity[2] = new timedActivity(420.0f, 2, OnNightStart, OnWin, null);

        powerOutageEventObject = new timedActivity(powerOutageTimeLimits.timeLimit, 0, null, TriggerPowerOutage, null);
        phoneRingEventObject = new timedActivity(phoneRingTimeLimits.timeLimit, 0, null, TriggerPhoneRing, null);

        deathTrigger = new timedActivity(3.0f, 0, null, OnDeath, null);

        GameObject[] toySpawns = GameObject.FindGameObjectsWithTag("ToySpawn");
        toySpawnLocations = new List<Vector3>();

        for (int i = 0; i < toySpawns.Length; i++)
        {
            toySpawnLocations.Add(toySpawns[i].transform.position);
            Destroy(toySpawns[i]);
        }

        GameObject[] candySpawns = GameObject.FindGameObjectsWithTag("CandySpawn");
        candySpawnLocations = new List<Vector3>();

        for (int i = 0; i < candySpawns.Length; i++)
        {
            candySpawnLocations.Add(candySpawns[i].transform.position);
            Destroy(candySpawns[i]);
        }

        WindowsActivity[] windows = GameObject.FindObjectsOfType<WindowsActivity>();

        for (int currentIndex = 0; currentIndex < windows.Length; currentIndex++)
        {
            GameObject obj = windows[currentIndex].gameObject;
            windowEventObjects.Add(new activityTrigger(obj, windowsTimeLimits.timeLimit, currentIndex, OnWindowActivityStart, OnWindowActivityFinished, OnWindowActivityUpdate));
        }

        if (GameObject.FindObjectOfType<PetDoorActivity>() != null)
            petdoorEventObject = new activityTrigger(GameObject.FindObjectOfType<PetDoorActivity>().gameObject, petdoorTimeLimits.timeLimit, 0, OnPetDoorActivityStart, OnPetDoorActivityFinished, OnPetDoorActivityUpdate);

        if (GameObject.FindObjectOfType<BasementHatch>() != null)
            basementHatchEventObject = new activityTrigger(GameObject.FindObjectOfType<BasementHatch>().gameObject, basementTimeLimits.timeLimit, 0, OnBasementHatchActivityStart, OnBasementHatchActivityFinished, OnBasementHatchActivityUpdate);

        if (GameObject.FindObjectOfType<FireplaceActivity>() != null)
            fireplaceEventObject = new activityTrigger(GameObject.FindObjectOfType<FireplaceActivity>().gameObject, fireplaceTimeLimits.timeLimit, 0, OnFireplaceActivityStart, OnFireplaceActivityFinished, OnFireplaceActivityUpdate);

        if (GameObject.FindObjectOfType<SkylightActivity>() != null)
            skylightEventObject = new activityTrigger(GameObject.FindObjectOfType<SkylightActivity>().gameObject, skylightTimeLimits.timeLimit, 0, OnSkylightActivityStart, OnSkylightActivityFinished, OnSkylightActivityUpdate);

        if (GameObject.FindObjectOfType<ToiletActivity>() != null)
            toiletEventObject = new activityTrigger(GameObject.FindObjectOfType<ToiletActivity>().gameObject, toiletTimeLimits.timeLimit, 0, OnToiletActivityStart, OnToiletActivityFinished, OnToiletActivityUpdate);

        thunderControlObjects = GameObject.FindGameObjectsWithTag("ThunderControl");
        telephoneAudioSource = GameObject.FindGameObjectWithTag("Telephone").GetComponent<AudioSource>();
        powerControlGameObjects = GameObject.FindGameObjectsWithTag("PowerControl");
    }

    public void SpawnToysAndCandys()
    {
        GameObject[] previousSpawnedToys = GameObject.FindGameObjectsWithTag("Interactable_Toy");

        for (int current = 0; current < previousSpawnedToys.Length; current++)
            Destroy(previousSpawnedToys[current]);

        int countToSelect = Mathf.Clamp(Random.Range(minToySpawnLocations, maxToySpawnLocations + 1), 0, toySpawnLocations.Count);

        List<Vector3> shuffledLocations = new List<Vector3>(toySpawnLocations);

        for (int i = shuffledLocations.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Vector3 temp = shuffledLocations[i];
            shuffledLocations[i] = shuffledLocations[randomIndex];
            shuffledLocations[randomIndex] = temp;
        }

        List<Vector3> spawnLocations = shuffledLocations.GetRange(0, countToSelect);

        for (int i = 0; i < spawnLocations.Count; i++)
        {
            Instantiate(toyPrefabs[Random.Range(0, toyPrefabs.Count)], spawnLocations[i], Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        }

        GameObject[] previousSpawnedCandys = GameObject.FindGameObjectsWithTag("Candy");

        for (int current = 0; current < previousSpawnedCandys.Length; current++)
            Destroy(previousSpawnedCandys[current]);

        countToSelect = Mathf.Clamp(maxCandySpawns, 0, candySpawnLocations.Count);

        shuffledLocations = new List<Vector3>(candySpawnLocations);

        for (int i = shuffledLocations.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Vector3 temp = shuffledLocations[i];
            shuffledLocations[i] = shuffledLocations[randomIndex];
            shuffledLocations[randomIndex] = temp;
        }

        spawnLocations = shuffledLocations.GetRange(0, countToSelect);

        for (int i = 0; i < spawnLocations.Count; i++)
        {
            Instantiate(candyPrefabs[Random.Range(0, candyPrefabs.Count)], spawnLocations[i], Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        }
    }

    private GameObject petDoorIcon;
    private GameObject basementHatchIcon;
    private GameObject windowIcon;
    private GameObject fireplaceIcon;
    private GameObject skylightIcon;
    private GameObject toiletIcon;

    private List<int> windowIconIDs = new List<int>();

    private void OnPetDoorActivityStart(int activityIndex)
    {
        petdoorTimeLimits.SelectRange();
        petdoorEventObject.gameObj.GetComponent<PetDoorActivity>().ActivityTriggerStart();
        int iconID = 0;
        if (iconsEnabled)
        {
            IconManager.Instance.RegisterIcon(iconID, petdoorEventObject.gameObj.transform.position, IconType.Door);
            petDoorIcon = IconManager.Instance.GetIcon(iconID);
        }
        HintManager.Instance.DisplayGameHint(HintType.PetDoor);
    }

    private void OnPetDoorActivityUpdate(int activityIndex)
    {
        petdoorTimeLimits.lastUpdateTime = gTime.currentTime;

        if (petDoorIcon != null)
        {
            IconFill iconFill = petDoorIcon.GetComponent<IconFill>();
            iconFill.Fill(petdoorEventObject.eventTime.GetProgress());
        }

        if (petdoorEventObject.gameObj.GetComponent<PetDoorActivity>().OnActivityUpdate(petdoorEventObject.eventTime.GetProgress()))
        {
            petdoorEventObject.eventTime.Deactivate(activeActivites);
            petdoorEventObject.eventTime.Reset();
            IconManager.Instance.UnregisterIcon(0);
        }
    }

    private void OnPetDoorActivityFinished(int activityIndex)
    {
        petdoorEventObject.eventTime.Deactivate(activeActivites);
        petdoorEventObject.gameObj.GetComponent<PetDoorActivity>().ActivityTriggerEnd();
        petdoorTimeLimits.finished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you hear the door bell, find a toy and feed it through the pet door to repel the nightling!";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnBasementHatchActivityStart(int activityIndex)
    {
        basementTimeLimits.SelectRange();
        basementHatchEventObject.gameObj.GetComponent<BasementHatch>().ActivityTriggerStart();
        int iconID = 1;
        if (iconsEnabled)
        {
            IconManager.Instance.RegisterIcon(iconID, basementHatchEventObject.gameObj.transform.position, IconType.Basement);
            basementHatchIcon = IconManager.Instance.GetIcon(iconID);
        }
        HintManager.Instance.DisplayGameHint(HintType.BasementHatch);
    }

    private void OnBasementHatchActivityUpdate(int activityIndex)
    {
        basementTimeLimits.lastUpdateTime = gTime.currentTime;

        if (basementHatchIcon != null)
        {
            IconFill iconFill = basementHatchIcon.GetComponent<IconFill>();
            iconFill.Fill(basementHatchEventObject.eventTime.GetProgress());
        }

        if (basementHatchEventObject.gameObj.GetComponent<BasementHatch>().OnActivityUpdate(basementHatchEventObject.eventTime.GetProgress()))
        {
            basementHatchEventObject.eventTime.Deactivate(activeActivites);
            basementHatchEventObject.eventTime.Reset();
            IconManager.Instance.UnregisterIcon(1);
        }
    }

    private void OnBasementHatchActivityFinished(int activityIndex)
    {
        basementHatchEventObject.eventTime.Deactivate(activeActivites);
        basementHatchEventObject.gameObj.GetComponent<BasementHatch>().ActivityTriggerEnd();
        basementTimeLimits.finished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you hear banging in the basement, rush down and close the hatch before they can open it!";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void EnsureWindowListsArePopulated(int activityIndex)
    {
        while (windowEventObjects.Count <= activityIndex)
        {
            GameObject _gameObj = new GameObject("WindowActivity_" + windowEventObjects.Count);
            float triggerTimeSeconds = 15f;
            int triggerIndex = windowEventObjects.Count;
            windowEventObjects.Add(new activityTrigger(_gameObj, triggerTimeSeconds, triggerIndex, OnWindowActivityStart, OnWindowActivityFinished, OnWindowActivityUpdate));
        }

        while (windowIconIDs.Count <= activityIndex)
        {
            windowIconIDs.Add(-1);
        }
    }

    private void OnWindowActivityStart(int activityIndex)
    {
        windowsTimeLimits.SelectRange();
        EnsureWindowListsArePopulated(activityIndex);
        windowEventObjects[activityIndex].gameObj.GetComponent<WindowsActivity>().ActivityTriggerStart();

        int iconID = 100 + activityIndex;
        if (iconsEnabled)
        {
            IconManager.Instance.RegisterIcon(iconID, windowEventObjects[activityIndex].gameObj.transform.position, IconType.Window);
            windowIconIDs[activityIndex] = iconID;
        }
        HintManager.Instance.DisplayGameHint(HintType.Window);

        windowsTimeLimits.lastUpdateTime = gTime.currentTime;
        //gTime.currentTime += (windowsTimeLimits.rangeStart / 2);
    }

    private void OnWindowActivityUpdate(int activityIndex)
    {
        EnsureWindowListsArePopulated(activityIndex);

        activityTrigger activityObject = windowEventObjects[activityIndex];
        int iconID = windowIconIDs[activityIndex];

        if (IconManager.Instance.IsIconRegistered(iconID))
        {
            GameObject icon = IconManager.Instance.GetIcon(iconID);
            IconFill iconFill = icon.GetComponent<IconFill>();
            iconFill.Fill(activityObject.eventTime.GetProgress());
        }

        if (activityObject.gameObj.GetComponent<WindowsActivity>().OnActivityUpdate(activityObject.eventTime.GetProgress()))
        {
            activityObject.eventTime.Deactivate(activeActivites);
            activityObject.eventTime.Reset();
            lastResetWindow = activityIndex;
            IconManager.Instance.UnregisterIcon(iconID);
        }
    }

    private void OnWindowActivityFinished(int activityIndex)
    {
        EnsureWindowListsArePopulated(activityIndex);

        activityTrigger activityObject = windowEventObjects[activityIndex];
        activityObject.eventTime.Deactivate(activeActivites);
        activityObject.gameObj.GetComponent<WindowsActivity>().ActivityTriggerEnd();

        int iconID = windowIconIDs[activityIndex];
        IconManager.Instance.UnregisterIcon(iconID);

        windowsTimeLimits.finished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you hear a window creaking open, find it and close it before they can open it!";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnFireplaceActivityStart(int activityIndex)
    {
        fireplaceTimeLimits.SelectRange();
        fireplaceEventObject.gameObj.GetComponent<FireplaceActivity>().ActivityTriggerStart(fireplaceEventObject.eventTime);
    }

    private void OnFireplaceActivityUpdate(int activityIndex)
    {
        fireplaceTimeLimits.SelectRange();
        if (UIManager.Instance == null || !UIManager.Instance.IsInGame())
        {
            if (IconManager.Instance.IsIconRegistered(2))
            {
                IconManager.Instance.UnregisterIcon(2);
            }
            return;
        }

        fireplaceTimeLimits.lastUpdateTime = gTime.currentTime;

        float progress = fireplaceEventObject.eventTime.GetProgress();

        if (fireplaceIcon != null)
        {
            IconFill iconFill = fireplaceIcon.GetComponent<IconFill>();
            iconFill.Fill(progress);
        }

        if (progress >= 0.5f)
        {
            if (iconsEnabled && !IconManager.Instance.IsIconRegistered(2))
            {
                IconManager.Instance.RegisterIcon(2, fireplaceEventObject.gameObj.transform.position, IconType.Fireplace);
                fireplaceIcon = IconManager.Instance.GetIcon(2);
            }
        }
        else
        {
            if (IconManager.Instance.IsIconRegistered(2))
            {
                IconManager.Instance.UnregisterIcon(2);
            }
        }

        if (fireplaceEventObject.gameObj.GetComponent<FireplaceActivity>().OnActivityUpdate(progress))
        {
            fireplaceEventObject.eventTime.Deactivate(activeActivites);
            fireplaceEventObject.eventTime.Reset();
            IconManager.Instance.UnregisterIcon(2);
        }
    }

    private void OnFireplaceActivityFinished(int activityIndex)
    {
        fireplaceEventObject.eventTime.Deactivate(activeActivites);
        fireplaceEventObject.gameObj.GetComponent<FireplaceActivity>().ActivityTriggerEnd();
        fireplaceTimeLimits.finished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you see the fireplace is almost out, grab wood from the basement and throw it in to keep the fire going!";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnSkylightActivityStart(int activityIndex)
    {
        skylightTimeLimits.SelectRange();
        skylightEventObject.gameObj.GetComponent<SkylightActivity>().ActivityTriggerStart();
        int iconID = 3;
        if (iconsEnabled)
        {
            IconManager.Instance.RegisterIcon(iconID, skylightEventObject.gameObj.transform.position, IconType.Window);
            skylightIcon = IconManager.Instance.GetIcon(iconID);
        }
        HintManager.Instance.DisplayGameHint(HintType.Skylight);
    }

    private void OnSkylightActivityUpdate(int activityIndex)
    {
        skylightTimeLimits.lastUpdateTime = gTime.currentTime;

        if (skylightIcon != null)
        {
            IconFill iconFill = skylightIcon.GetComponent<IconFill>();
            iconFill.Fill(skylightEventObject.eventTime.GetProgress());
        }

        if (skylightEventObject.gameObj.GetComponent<SkylightActivity>().OnActivityUpdate(skylightEventObject.eventTime.GetProgress()))
        {
            skylightEventObject.eventTime.Deactivate(activeActivites);
            skylightEventObject.eventTime.Reset();
            IconManager.Instance.UnregisterIcon(3);
        }
    }

    private void OnSkylightActivityFinished(int activityIndex)
    {
        skylightEventObject.eventTime.Deactivate(activeActivites);
        skylightEventObject.gameObj.GetComponent<SkylightActivity>().ActivityTriggerEnd();
        skylightTimeLimits.finished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you hear tapping on the skylight, use the remote in the kitchen to close it before they can get in!";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnToiletActivityStart(int activityIndex)
    {
        toiletTimeLimits.SelectRange();
        toiletEventObject.gameObj.GetComponent<ToiletActivity>().ActivityTriggerStart();
        int iconID = 4;
        if (iconsEnabled)
        {
            IconManager.Instance.RegisterIcon(iconID, toiletEventObject.gameObj.transform.position, IconType.Toilet);
            toiletIcon = IconManager.Instance.GetIcon(iconID);
        }
        HintManager.Instance.DisplayGameHint(HintType.Toilet);
    }

    private void OnToiletActivityUpdate(int activityIndex)
    {
        toiletTimeLimits.lastUpdateTime = gTime.currentTime;

        if (toiletIcon != null)
        {
            IconFill iconFill = toiletIcon.GetComponent<IconFill>();
            iconFill.Fill(toiletEventObject.eventTime.GetProgress());
        }

        if (toiletEventObject.gameObj.GetComponent<ToiletActivity>().OnActivityUpdate(toiletEventObject.eventTime.GetProgress()))
        {
            toiletEventObject.eventTime.Deactivate(activeActivites);
            toiletEventObject.eventTime.Reset();
            IconManager.Instance.UnregisterIcon(4);
        }
    }

    private void OnToiletActivityFinished(int activityIndex)
    {
        toiletEventObject.eventTime.Deactivate(activeActivites);
        toiletEventObject.gameObj.GetComponent<ToiletActivity>().ActivityTriggerEnd();

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you hear splashing sounds coming from the bathroom, flush the toilet to send the nightling away!";
            deathTrigger.Activate(activeActivites);
        }
    }
    void DispatchWindowEvents()
    {
        if (windowsTimeLimits.finished || windowEventObjects.Count <= 0)
            return;

        if (gTime.IsInLimit(windowsTimeLimits))
        {
            activityTrigger activity = windowEventObjects[Mathf.Clamp(Random.Range(0, windowEventObjects.Count), 0, windowEventObjects.Count)];
            int triggerIndex = activity.eventTime.GetTriggerIndex();

            if (lastActivatedWindow == triggerIndex
                || lastResetWindow == triggerIndex
                || activity.eventTime.IsActive())
                return;


            if (!activity.eventTime.IsActive())
            {
                activity.eventTime.Activate(activeActivites);
                lastActivatedWindow = triggerIndex;
            }

            windowsTimeLimits.lastUpdateTime = gTime.currentTime;
        }
    }

    void DispatchPetdoorEvent()
    {
        if (petdoorTimeLimits.finished)
            return;

        if (petdoorEventObject.gameObj && gTime.IsInLimit(petdoorTimeLimits) && !petdoorEventObject.eventTime.IsActive())
        {
            petdoorEventObject.eventTime.Activate(activeActivites);
            petdoorTimeLimits.lastUpdateTime = gTime.currentTime;
        }
    }

    private void DispatchBasementHatchEvent()
    {
        if (basementTimeLimits.finished)
            return;

        if (basementHatchEventObject != null && basementHatchEventObject.gameObj && gTime.IsInLimit(basementTimeLimits) && !basementHatchEventObject.eventTime.IsActive())
        {
            basementHatchEventObject.eventTime.Activate(activeActivites);
            basementTimeLimits.lastUpdateTime = gTime.currentTime;
        }
    }

    private void DispatchFireplaceEvent()
    {
        if (fireplaceTimeLimits.finished)
            return;

        if (fireplaceEventObject != null && fireplaceEventObject.gameObj && gTime.IsInLimit(fireplaceTimeLimits) && !fireplaceEventObject.eventTime.IsActive())
        {
            fireplaceEventObject.eventTime.Activate(activeActivites);
            fireplaceTimeLimits.lastUpdateTime = gTime.currentTime;
        }
    }

    private void DispatchSkylightEvent()
    {
        if (skylightTimeLimits.finished)
            return;

        if (skylightEventObject != null && skylightEventObject.gameObj && gTime.IsInLimit(skylightTimeLimits) && !skylightEventObject.eventTime.IsActive())
        {
            skylightEventObject.eventTime.Activate(activeActivites);
            skylightTimeLimits.lastUpdateTime = gTime.currentTime;
        }
    }

    private void DispatchToiletEvent()
    {
        if (toiletTimeLimits.finished)
            return;

        if (toiletEventObject != null && toiletEventObject.gameObj && gTime.IsInLimit(toiletTimeLimits) && !toiletEventObject.eventTime.IsActive())
        {
            toiletEventObject.eventTime.Activate(activeActivites);
            toiletTimeLimits.lastUpdateTime = gTime.currentTime;
        }
    }
    private void OnNightStart(int activityIndex)
    {
        powerOutageEventObject.Activate(activeActivites);
        phoneRingEventObject.Activate(activeActivites);
        SpawnToysAndCandys();
        activeNight = activityIndex;
    }
    private void OnProgressToNextNight(int activityIndex)
    {
        nightActivity[activityIndex].Deactivate(activeActivites);
        nightActivity[activityIndex + 1].Activate(activeActivites);
        playerController.Respawn();
    }

    public void StartNight(int nightIndex)
    {
        nightActivity[1].Deactivate(activeActivites);
        nightActivity[2].Deactivate(activeActivites);
        nightActivity[0].Deactivate(activeActivites);

        ApplyDifficultySettings(UIManager.SelectedDifficulty);

        gTime.currentTime = 0f;
        nightHasStarted = true;

        nightActivity[nightIndex].Activate(activeActivites);
        playerController.Respawn();

        if (IconManager.Instance != null)
        {
            IconManager.Instance.ClearAllIcons();
        }
    }

    private bool stopActivityDirector = false;

    private void OnWin(int activityIndex)
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.CompleteNight(activeNight);
        }

        if (IconManager.Instance != null)
        {
            IconManager.Instance.ClearAllIcons();
        }

        uiManager.WinGame();
        stopActivityDirector = true;
    }

    void OnDeath(int activityIndex)
    {
        if (IconManager.Instance != null)
        {
            IconManager.Instance.ClearAllIcons();
        }

        DeathTime = gTime.currentTime;
        playerController.Die();
        uiManager.LoseGame(deathCause);
        stopActivityDirector = true;
    }

    void ControlRainAndThunderSpatialAudio()
    {
        for (int i = 0; i < thunderControlObjects.Length; i++)
        {
            AudioSource audioSource = thunderControlObjects[i].GetComponent<AudioSource>();
            if (audioSource.isPlaying)
            {
                audioSource.volume = (soundManager.musicVolume * soundManager.masterVolume);
            }
            else
            {
                audioSource.volume = 0.5f * (soundManager.musicVolume * soundManager.masterVolume);
            }
        }
    }

    private bool powerOut = false;
    void TriggerPowerOutage(int activityIndex)
    {
        for (int i = 0; i < powerControlGameObjects.Length; i++)
            powerControlGameObjects[i].SetActive(false);

        powerOutageEventObject.Deactivate(activeActivites);
        powerOut = true;
        SoundManager.Instance.PlaySound("Breaker");
        HintManager.Instance.DisplayGameHint(HintType.PowerBox);
    }
    public void RestorePower()
    {
        if (!powerOut)
            return;

        for (int i = 0; i < powerControlGameObjects.Length; i++)
            powerControlGameObjects[i].SetActive(true);

        powerOutageEventObject.Activate(activeActivites);
        powerOut = false;
        SoundManager.Instance.PlaySound("Breaker");


        if (FindObjectOfType<FuseBoxAnimator>() != null)
            FindObjectOfType<FuseBoxAnimator>().TriggerAnimation();
    }

    void TriggerPhoneRing(int activityIndex)
    {
        telephoneAudioSource.Play();
        phoneRingEventObject.Deactivate(activeActivites);
    }

    public void StopPhoneRing()
    {
        if (!phoneRingEventObject.IsActive())
            return;

        telephoneAudioSource.Stop();
    }

    void Update()
    {
        if (!nightHasStarted || stopActivityDirector)
            return;

        gTime.OnUpdate();

        DispatchWindowEvents();
        DispatchPetdoorEvent();

        if (activeNight > 0)
        {
            DispatchBasementHatchEvent();
            DispatchFireplaceEvent();
        }

        if (activeNight > 1)
        {
            DispatchSkylightEvent();
            DispatchToiletEvent();
        }

        ControlRainAndThunderSpatialAudio();

        for (int i = 0; i < activeActivites.Count; i++)
        {
            activeActivites[i].OnUpdate();
        }
    }

    public int GetActiveNight()
    {
        return activeNight;
    }

    private void ApplyDifficultySettings(int difficulty)
    {
        iconsEnabled = difficulty < 3;
        float timeFactor = 1f;
        float toyFactor = 1f;
        float staminaFactor = 1f;
        bool candyZero = false;

        switch (difficulty)
        {
            case 1: // Hard
                timeFactor = 0.75f;
                break;
            case 2: // Very Hard
                timeFactor = 0.6f;
                toyFactor = 0.8f;
                break;
            case 3: // Extreme
                timeFactor = 0.5f;
                toyFactor = 0.6f;
                staminaFactor = 0.7f;
                break;
            case 4: // Nightmare
                timeFactor = 0.4f;
                toyFactor = 0.5f;
                staminaFactor = 0.5f;
                candyZero = true;
                break;
            default:
                break;
        }

        windowsTimeLimits.rangeStart *= timeFactor;
        windowsTimeLimits.rangeEnd   *= timeFactor;
        windowsTimeLimits.timeLimit  *= timeFactor;

        petdoorTimeLimits.rangeStart *= timeFactor;
        petdoorTimeLimits.rangeEnd   *= timeFactor;
        petdoorTimeLimits.timeLimit  *= timeFactor;

        basementTimeLimits.rangeStart *= timeFactor;
        basementTimeLimits.rangeEnd   *= timeFactor;
        basementTimeLimits.timeLimit  *= timeFactor;

        fireplaceTimeLimits.rangeStart *= timeFactor;
        fireplaceTimeLimits.rangeEnd   *= timeFactor;
        fireplaceTimeLimits.timeLimit  *= timeFactor;

        skylightTimeLimits.rangeStart *= timeFactor;
        skylightTimeLimits.rangeEnd   *= timeFactor;
        skylightTimeLimits.timeLimit  *= timeFactor;

        toiletTimeLimits.rangeStart *= timeFactor;
        toiletTimeLimits.rangeEnd   *= timeFactor;
        toiletTimeLimits.timeLimit  *= timeFactor;

        minToySpawnLocations = Mathf.Max(0, Mathf.RoundToInt(minToySpawnLocations * toyFactor));
        maxToySpawnLocations = Mathf.Max(0, Mathf.RoundToInt(maxToySpawnLocations * toyFactor));

        if (candyZero)
            maxCandySpawns = 0;

        if (playerController != null)
        {
            playerController.maxStamina *= staminaFactor;
            playerController.ResetStaminaToMax();
        }

        if (difficulty == 4)
            RenderSettings.ambientLight = Color.black;
        else
            RenderSettings.ambientLight = originalAmbientLight;
    }
}
