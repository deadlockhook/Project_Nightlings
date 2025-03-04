using System.Collections;
using System.Collections.Generic;
//using UnityEditor.PackageManager.UI;
using UnityEngine;

public class ActivityDirector : MonoBehaviour
{
    public delegate void timedActivityTrigger(int val);
    public class playedSoundAtTrigger
    {
        public playedSoundAtTrigger(float _percentage, AudioSource _source)
        {
            percentage = _percentage;
            played = false;
            source = _source;
        }

        public bool ShouldPlay(float _percentage)
        {
            if (played)
                return false;

            if (_percentage >= percentage)
            {
                played = true;
                return true;
            }

            return false;
        }

        private AudioSource source;
        private bool played;
        private float percentage;
    }
    public class timedActivity
    {
        public timedActivity(float _triggerTimeSeconds, int _triggerIndex, timedActivityTrigger _actionStart, timedActivityTrigger _actionEnd, timedActivityTrigger _actionOnUpdate)
        {
            currentTime = 0;
            triggerTime = _triggerTimeSeconds;
            actionStart = _actionStart;
            actionEnd = _actionEnd;
            actionOnUpdate = _actionOnUpdate;
            triggerIndex = _triggerIndex;
            active = false;
        }

        public void RemoveProgressPercentage(float progressToRemove)
        {
            currentTime -= triggerTime * progressToRemove;

            if (currentTime < 0)
                currentTime = 0;
        }
        public void OnUpdate()
        {
            if (!active)
                return;

            currentTime += Time.deltaTime;

            if (actionOnUpdate != null)
                actionOnUpdate(triggerIndex);

            if (currentTime >= triggerTime)
            {
                if (actionEnd != null)
                    actionEnd(triggerIndex);

                Reset();
            }
        }
        public void Activate(List<timedActivity> activites)
        {
            activites.Add(this);
            active = true;

            currentTime = 0;

            if (actionStart != null)
                actionStart(triggerIndex);
        }
        public void Deactivate(List<timedActivity> activites)
        {
            active = false;
            activites.Remove(this);
        }

        public void Reset()
        {
            currentTime = 0;
        }

        public float GetProgress()
        {
            return currentTime / triggerTime;
        }

        public bool IsActive()
        {
            return active;
        }

        private bool active;
        private int triggerIndex;
        private float currentTime;
        private float triggerTime;
        private timedActivityTrigger actionStart;
        private timedActivityTrigger actionEnd;
        private timedActivityTrigger actionOnUpdate;
    }
    public class activityTrigger
    {
        public activityTrigger(GameObject _gameObj, float triggerTimeSeconds, int triggerIndex, timedActivityTrigger actionStart, timedActivityTrigger actionEnd, timedActivityTrigger actionOnUpdate)
        {
            gameObj = _gameObj;
            eventTime = new timedActivity(triggerTimeSeconds, triggerIndex, actionStart, actionEnd, actionOnUpdate);
        }

        public GameObject gameObj;
        public timedActivity eventTime;
    }

    private SoundManager soundManager;
    private UIManager uiManager;
    private PlayerController playerController;
    private List<timedActivity> activeActivites;


   // private AudioSource rainAndThunder;
   // private Vector3 rainAndThunderInitialPosition;

    public List<GameObject> toyPrefabs;
    [SerializeField] private int minToySpawnLocations = 12;
    [SerializeField] private int maxToySpawnLocations = 20;

    private float triggerWindowsActivityLogicRangeStart = 30.0f;
    private float triggerWindowsActivityLogicRangeEnd = 45.0f;
    private float windowsActivityTimeLimit = 30.0f;

    private float triggerPetdoorActivityLogicRangeStart = 50.0f;
    private float triggerPetdoorActivityLogicRangeEnd = 60.0f;
    private float petdoorActivityTimeLimit = 35.0f;

    private float triggerBasementActivityLogicRangeStart = 55.0f;
    private float triggerBasementActivityLogicRangeEnd = 75.0f;
    private float basementHatchActivityTimeLimit = 35.0f;

    private float triggerFireplaceActivityLogicRangeStart = 0.0f;
    private float triggerFireplaceActivityLogicRangeEnd = 0.0f;
    private float fireplaceActivityTimeLimit = 90.0f; // 1 minute 30 seconds

    private float triggerSkylightActivityLogicRangeStart = 70.0f;
    private float triggerSkylightActivityLogicRangeEnd = 80.0f;
    private float skylightActivityTimeLimit = 30.0f;

    private float toiletActivityLogicRangeStart = 50.0f;
    private float toiletActivityLogicRangeEnd = 70.0f;
    private float toiletActivityTimeLimit = 30.0f;

    private List<activityTrigger> windowEventObjects;
    private activityTrigger petdoorEventObject;
    private activityTrigger basementHatchEventObject;
    private activityTrigger fireplaceEventObject;
    private activityTrigger skylightEventObject;
    private activityTrigger toiletEventObject;

    private List<Vector3> toySpawnLocations;

    private float currentDeltaTime = 0;
    private float lastDeltaTimeForWindowEvents = 0;
    private float lastDeltaTimeForPetDoorEvents = 0;
    private float lastDeltaTimeForBasementHatchEvent = 0;
    private float lastDeltaTimeForFireplaceEvent = 0;
    private float lastDeltaTimeForSkylightEvent = 0;
    private float lastDeltaTimeForToiletEvent = 0;


    private timedActivity[] nightActivity;
    private int activeNight = 0;

    private bool nightHasStarted = false;

    private timedActivity deathTrigger;

    public GameObject iconPrefab;

    private string deathCause = "Unknown";
    public float DeathTime { get; private set; }

    void Start()
    {
        //rainAndThunder = GameObject.Find("DynamicAimbienceSource").GetComponent<AudioSource>();
       // rainAndThunderInitialPosition = rainAndThunder.transform.position;

        uiManager = FindObjectOfType<UIManager>();
        playerController = FindObjectOfType<PlayerController>();
        soundManager = FindObjectOfType<SoundManager>();
        activeActivites = new List<timedActivity>();
        windowEventObjects = new List<activityTrigger>();
        lastDeltaTimeForWindowEvents = Time.deltaTime;
        lastDeltaTimeForPetDoorEvents = lastDeltaTimeForWindowEvents;

        nightActivity = new timedActivity[3];
        //  420 seconds = 7 minutes
        nightActivity[0] = new timedActivity(420.0f, 0, OnNightStart, OnWin, null);
        nightActivity[1] = new timedActivity(420.0f, 1, OnNightStart, OnWin, null);
        nightActivity[2] = new timedActivity(420.0f, 2, OnNightStart, OnWin, null);

        deathTrigger = new timedActivity(10.0f, 0, null, OnDeath, null);

        toySpawnLocations = new List<Vector3>();

        GameObject[] toySpawns = GameObject.FindGameObjectsWithTag("ToySpawn");

        for (int i = 0; i < toySpawns.Length; i++)
        {
            toySpawnLocations.Add(toySpawns[i].transform.position);
            Destroy(toySpawns[i]);
        }

        WindowsActivity[] windows = GameObject.FindObjectsOfType<WindowsActivity>();

        for (int currentIndex = 0; currentIndex < windows.Length; currentIndex++)
        {
            GameObject obj = windows[currentIndex].gameObject;
            windowEventObjects.Add(new activityTrigger(obj, windowsActivityTimeLimit, currentIndex, OnWindowActivityStart, OnWindowActivityFinished, OnWindowActivityUpdate));
        }

        if (GameObject.FindObjectOfType<PetDoorActivity>() != null)
            petdoorEventObject = new activityTrigger(GameObject.FindObjectOfType<PetDoorActivity>().gameObject, petdoorActivityTimeLimit, 0, OnPetDoorActivityStart, OnPetDoorActivityFinished, OnPetDoorActivityUpdate);

        if (GameObject.FindObjectOfType<BasementHatch>() != null)
            basementHatchEventObject = new activityTrigger(GameObject.FindObjectOfType<BasementHatch>().gameObject, basementHatchActivityTimeLimit, 0, OnBasementHatchActivityStart, OnBasementHatchActivityFinished, OnBasementHatchActivityUpdate);

        if (GameObject.FindObjectOfType<FireplaceActivity>() != null)
            fireplaceEventObject = new activityTrigger(GameObject.FindObjectOfType<FireplaceActivity>().gameObject, fireplaceActivityTimeLimit, 0, OnFireplaceActivityStart, OnFireplaceActivityFinished, OnFireplaceActivityUpdate);

        if (GameObject.FindObjectOfType<SkylightActivity>() != null)
            skylightEventObject = new activityTrigger(GameObject.FindObjectOfType<SkylightActivity>().gameObject, skylightActivityTimeLimit, 0, OnSkylightActivityStart, OnSkylightActivityFinished, OnSkylightActivityUpdate);

        if (GameObject.FindObjectOfType<ToiletActivity>() != null)
            toiletEventObject = new activityTrigger(GameObject.FindObjectOfType<ToiletActivity>().gameObject, toiletActivityTimeLimit, 0, OnToiletActivityStart, OnToiletActivityFinished, OnToiletActivityUpdate);

        //nightActivity[0].Activate(activeActivites);
    }

    private bool petdoorActivityFinished = false;
    private bool windowActivityFinished = false;
    private bool basementHatchActivityFinished = false;
    private bool fireplaceActivityFinished = false;
    private bool skylightActivityFinished = false;
    private bool toiletActivityFinished = false;

    private void OnPetDoorActivityStart(int activityIndex)
    {
        petdoorEventObject.gameObj.GetComponent<PetDoorActivity>().ActivityTriggerStart();
        IconManager.Instance.RegisterIcon(0, petdoorEventObject.gameObj.transform.position, IconType.Door);
        HintManager.Instance.DisplayGameHint(HintType.PetDoor);
    }

    private void OnPetDoorActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForPetDoorEvents = currentDeltaTime;

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
        petdoorActivityFinished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you hear the door bell, find a toy and feed it through the pet door to repel the nightling!.";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnBasementHatchActivityStart(int activityIndex)
    {
        basementHatchEventObject.gameObj.GetComponent<BasementHatch>().ActivityTriggerStart();
        IconManager.Instance.RegisterIcon(1, basementHatchEventObject.gameObj.transform.position, IconType.Basement);
        HintManager.Instance.DisplayGameHint(HintType.BasementHatch);
    }

    private void OnBasementHatchActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForBasementHatchEvent = currentDeltaTime;

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
        basementHatchActivityFinished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you hear banging in the basement, rush down and close the hatch before they can open it!";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnWindowActivityStart(int activityIndex)
    {
        windowEventObjects[activityIndex].gameObj.GetComponent<WindowsActivity>().ActivityTriggerStart();
        IconManager.Instance.RegisterIcon(5, windowEventObjects[activityIndex].gameObj.transform.position, IconType.Window);
        HintManager.Instance.DisplayGameHint(HintType.Window);
    }

    private void OnWindowActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForWindowEvents = currentDeltaTime;

        activityTrigger activityObject = windowEventObjects[activityIndex];
        if (activityObject.gameObj.GetComponent<WindowsActivity>().OnActivityUpdate(activityObject.eventTime.GetProgress()))
        {
            activityObject.eventTime.Deactivate(activeActivites);
            activityObject.eventTime.Reset();
            IconManager.Instance.UnregisterIcon(5);
        }
    }

    private void OnWindowActivityFinished(int activityIndex)
    {
        activityTrigger activityObject = windowEventObjects[activityIndex];
        activityObject.eventTime.Deactivate(activeActivites);
        activityObject.gameObj.GetComponent<WindowsActivity>().ActivityTriggerEnd();
        windowActivityFinished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you hear a window creaking open, find it and close it before they can open it!";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnFireplaceActivityStart(int activityIndex)
    {
        fireplaceEventObject.gameObj.GetComponent<FireplaceActivity>().ActivityTriggerStart(fireplaceEventObject.eventTime);
    }

    private void OnFireplaceActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForFireplaceEvent = currentDeltaTime;

        float progress = fireplaceEventObject.eventTime.GetProgress();

        if (progress >= 0.5f)
        {
            if (!IconManager.Instance.IsIconRegistered(2))
            {
                IconManager.Instance.RegisterIcon(2, fireplaceEventObject.gameObj.transform.position, IconType.Fireplace);
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
        fireplaceActivityFinished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you see the fireplace is almost out, grab wood from the basement and throw it in to keep the fire going!";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnSkylightActivityStart(int activityIndex)
    {
        skylightEventObject.gameObj.GetComponent<SkylightActivity>().ActivityTriggerStart();
        IconManager.Instance.RegisterIcon(3, skylightEventObject.gameObj.transform.position, IconType.Window);
        HintManager.Instance.DisplayGameHint(HintType.Skylight);
    }
    private void OnSkylightActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForSkylightEvent = currentDeltaTime;
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
        skylightActivityFinished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "If you hear tapping on the sky light, use the remote in the kitchen to close it before they can get in!";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnToiletActivityStart(int activityIndex)
    {
        toiletEventObject.gameObj.GetComponent<ToiletActivity>().ActivityTriggerStart();
        IconManager.Instance.RegisterIcon(4, toiletEventObject.gameObj.transform.position, IconType.Toilet);
        HintManager.Instance.DisplayGameHint(HintType.Toilet);
    }

    private void OnToiletActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForToiletEvent = currentDeltaTime;
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

    public void SpawnToys()
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
            Instantiate(toyPrefabs[Random.Range(0, toyPrefabs.Count)], spawnLocations[i], Quaternion.Euler(Random.Range(-90.0f, 90.0f), Random.Range(180, -180), Random.Range(180, -180)));
        }
    }

    void DispatchWindowEvents()
    {
        if (windowActivityFinished)
            return;

        if (currentDeltaTime - lastDeltaTimeForWindowEvents >= Random.Range(triggerWindowsActivityLogicRangeStart, triggerWindowsActivityLogicRangeEnd) && windowEventObjects.Count > 0)
        {
            activityTrigger activity = windowEventObjects[Mathf.Clamp(Random.Range(0, windowEventObjects.Count), 0, windowEventObjects.Count)];

            if (!activity.eventTime.IsActive())
                activity.eventTime.Activate(activeActivites);

            lastDeltaTimeForWindowEvents = currentDeltaTime;
        }
    }

    void DispatchPetdoorEvent()
    {
        if (petdoorActivityFinished)
            return;

        if (petdoorEventObject.gameObj && currentDeltaTime - lastDeltaTimeForPetDoorEvents >= Random.Range(triggerPetdoorActivityLogicRangeStart, triggerPetdoorActivityLogicRangeEnd) && !petdoorEventObject.eventTime.IsActive())
        {
            petdoorEventObject.eventTime.Activate(activeActivites);
            lastDeltaTimeForPetDoorEvents = currentDeltaTime;
        }
    }

    private void DispatchBasementHatchEvent()
    {
        if (basementHatchActivityFinished)
            return;

        if (basementHatchEventObject != null && basementHatchEventObject.gameObj && currentDeltaTime - lastDeltaTimeForBasementHatchEvent >= Random.Range(triggerBasementActivityLogicRangeStart, triggerBasementActivityLogicRangeEnd) && !basementHatchEventObject.eventTime.IsActive())
        {
            basementHatchEventObject.eventTime.Activate(activeActivites);
            lastDeltaTimeForBasementHatchEvent = currentDeltaTime;
        }
    }

    private void DispatchFireplaceEvent()
    {
        if (fireplaceActivityFinished)
            return;

        if (fireplaceEventObject != null && fireplaceEventObject.gameObj && currentDeltaTime - lastDeltaTimeForFireplaceEvent >= Random.Range(triggerFireplaceActivityLogicRangeStart, triggerFireplaceActivityLogicRangeEnd) && !fireplaceEventObject.eventTime.IsActive())
        {
            fireplaceEventObject.eventTime.Activate(activeActivites);
            lastDeltaTimeForFireplaceEvent = currentDeltaTime;
        }
    }

    private void DispatchSkylightEvent()
    {
        if (skylightActivityFinished)
            return;

        if (skylightEventObject != null && skylightEventObject.gameObj && currentDeltaTime - lastDeltaTimeForSkylightEvent >= Random.Range(triggerSkylightActivityLogicRangeStart, triggerSkylightActivityLogicRangeEnd) && !skylightEventObject.eventTime.IsActive())
        {
            skylightEventObject.eventTime.Activate(activeActivites);
            lastDeltaTimeForSkylightEvent = currentDeltaTime;
        }
    }

    private void DispatchToiletEvent()
    {
        if (toiletActivityFinished)
            return;

        if (toiletEventObject != null && toiletEventObject.gameObj && currentDeltaTime - lastDeltaTimeForToiletEvent >= Random.Range(toiletActivityLogicRangeStart, toiletActivityLogicRangeEnd) && !toiletEventObject.eventTime.IsActive())
        {
            toiletEventObject.eventTime.Activate(activeActivites);
            lastDeltaTimeForToiletEvent = currentDeltaTime;
        }
    }
    private void OnNightStart(int activityIndex)
    {
        SpawnToys();
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

        currentDeltaTime = 0f;
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

        DeathTime = currentDeltaTime;
        playerController.Die();
        uiManager.LoseGame(deathCause);
        stopActivityDirector = true;
    }

    void ControlRainAndThunderSpatialAudio()
    {
        GameObject closestOpeningPoint = null;
        float last_distance = float.MaxValue;

        foreach (var window in windowEventObjects)
        {
            AudioSource audioSource = window.gameObj.GetComponent<AudioSource>();

            if (window.eventTime.IsActive())
                audioSource.volume =  soundManager.musicVolume;
            else
                audioSource.volume = 0.5f * soundManager.musicVolume;
        }

        AudioSource audioSourcePetDoor = petdoorEventObject.gameObj.GetComponent<AudioSource>();

        if (petdoorEventObject.eventTime.IsActive())
            audioSourcePetDoor.volume = soundManager.musicVolume;
        else
            audioSourcePetDoor.volume = 0.5f * soundManager.musicVolume;

        AudioSource audioSourceSkylight = skylightEventObject.gameObj.GetComponent<AudioSource>();

        if (skylightEventObject.eventTime.IsActive())
            audioSourceSkylight.volume = soundManager.musicVolume;
        else
            audioSourceSkylight.volume = 0.5f * soundManager.musicVolume;

    }

    void Update()
    {
        if (!nightHasStarted || stopActivityDirector)
            return;

        currentDeltaTime += Time.deltaTime;

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
}
