using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
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
        public timedActivity(float _triggerTimeMilliSeconds, int _triggerIndex, timedActivityTrigger _actionStart, timedActivityTrigger _actionEnd, timedActivityTrigger _actionOnUpdate)
        {
            currentTime = 0;
            triggerTime = _triggerTimeMilliSeconds;
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

            currentTime += (Time.deltaTime * 1000f);

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
        public activityTrigger(GameObject _gameObj, float triggerTimeMilliSeconds, int triggerIndex, timedActivityTrigger actionStart, timedActivityTrigger actionEnd, timedActivityTrigger actionOnUpdate)
        {
            gameObj = _gameObj;
            eventTime = new timedActivity(triggerTimeMilliSeconds, triggerIndex, actionStart, actionEnd, actionOnUpdate);
        }

        public GameObject gameObj;
        public timedActivity eventTime;
    }

    private SoundManager soundManager;
    private UIManager uiManager;
    private PlayerController playerController;
    private List<timedActivity> activeActivites;

    private AudioSource rainAndThunder;

    public List<GameObject> toyPrefabs;
    [SerializeField] private int minToySpawnLocations = 12;
    [SerializeField] private int maxToySpawnLocations = 20;

    private float triggerWindowsActivityLogicRangeStart = 35000.0f;
    private float triggerWindowsActivityLogicRangeEnd = 45000.0f;
    private float windowsActivityTimeLimit = 30000.0f;

    private float triggerPetdoorActivityLogicRangeStart = 45000.0f;
    private float triggerPetdoorActivityLogicRangeEnd = 55000.0f;
    private float petdoorActivityTimeLimit = 28000.0f;

    private float triggerBasementActivityLogicRangeStart = 25000.0f;
    private float triggerBasementActivityLogicRangeEnd = 33000.0f;
    private float basementHatchActivityTimeLimit = 35000.0f;

    private float triggerFireplaceActivityLogicRangeStart = 0.0f;
    private float triggerFireplaceActivityLogicRangeEnd = 0.0f;
    private float fireplaceActivityTimeLimit = 60000.0f;

    private float triggerSkylightActivityLogicRangeStart = 70000.0f;
    private float triggerSkylightActivityLogicRangeEnd = 80000.0f;
    private float skylightActivityTimeLimit = 20000.0f;

    private float toiletActivityLogicRangeStart = 50000.0f;
    private float toiletActivityLogicRangeEnd = 60000.0f;
    private float toiletActivityTimeLimit = 25000.0f;


    //  private float toiletActivityLogicRangeStart = 500.0f;
    //  private float toiletActivityLogicRangeEnd = 600.0f;
    //  private float toiletActivityTimeLimit = 15000.0f;

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

    private timedActivity deathTrigger;

    public GameObject iconPrefab;

    private string deathCause = "Unknown";

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        playerController = FindObjectOfType<PlayerController>();
        soundManager = FindObjectOfType<SoundManager>();
        activeActivites = new List<timedActivity>();
        windowEventObjects = new List<activityTrigger>();
        lastDeltaTimeForWindowEvents = Time.deltaTime * 1000.0f;
        lastDeltaTimeForPetDoorEvents = lastDeltaTimeForWindowEvents;

        nightActivity = new timedActivity[3];
        //  420000
        nightActivity[0] = new timedActivity(420000, 0, OnNightStart, OnProgressToNextNight, null);
        nightActivity[1] = new timedActivity(420000, 1, OnNightStart, OnProgressToNextNight, null);
        nightActivity[2] = new timedActivity(420000, 2, OnNightStart, OnWin, null);

        deathTrigger = new timedActivity(10000, 0, null, OnDeath, null);

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

        nightActivity[0].Activate(activeActivites);
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
        uiManager.ShowIcon(iconPrefab, petdoorEventObject.gameObj.transform.position, 0);
        HintManager.Instance.DisplayGameHint(HintType.PetDoor);
    }

    private void OnPetDoorActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForPetDoorEvents = currentDeltaTime;

        if (petdoorEventObject.gameObj.GetComponent<PetDoorActivity>().OnActivityUpdate(petdoorEventObject.eventTime.GetProgress()))
        {
            petdoorEventObject.eventTime.Deactivate(activeActivites);
            petdoorEventObject.eventTime.Reset();
            uiManager.HideIcon(0);
        }
    }

    private void OnPetDoorActivityFinished(int activityIndex)
    {
        petdoorEventObject.eventTime.Deactivate(activeActivites);
        petdoorEventObject.gameObj.GetComponent<PetDoorActivity>().ActivityTriggerEnd();
        petdoorActivityFinished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "Pet Door";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnBasementHatchActivityStart(int activityIndex)
    {
        basementHatchEventObject.gameObj.GetComponent<BasementHatch>().ActivityTriggerStart();
        uiManager.ShowIcon(iconPrefab, basementHatchEventObject.gameObj.transform.position, 1);
        HintManager.Instance.DisplayGameHint(HintType.BasementHatch);
    }

    private void OnBasementHatchActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForBasementHatchEvent = currentDeltaTime;

        if (basementHatchEventObject.gameObj.GetComponent<BasementHatch>().OnActivityUpdate(basementHatchEventObject.eventTime.GetProgress()))
        {
            basementHatchEventObject.eventTime.Deactivate(activeActivites);
            basementHatchEventObject.eventTime.Reset();
            uiManager.HideIcon(1);
        }
    }

    private void OnBasementHatchActivityFinished(int activityIndex)
    {
        basementHatchEventObject.eventTime.Deactivate(activeActivites);
        basementHatchEventObject.gameObj.GetComponent<BasementHatch>().ActivityTriggerEnd();
        basementHatchActivityFinished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "Basement Hatch";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnWindowActivityStart(int activityIndex)
    {
        windowEventObjects[activityIndex].gameObj.GetComponent<WindowsActivity>().ActivityTriggerStart();
        uiManager.ShowIcon(iconPrefab, windowEventObjects[activityIndex].gameObj.transform.position, activityIndex);
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
            uiManager.HideIcon(activityIndex);
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
            deathCause = "Window";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnFireplaceActivityStart(int activityIndex)
    {
        fireplaceEventObject.gameObj.GetComponent<FireplaceActivity>().ActivityTriggerStart(fireplaceEventObject.eventTime);
        uiManager.ShowIcon(iconPrefab, fireplaceEventObject.gameObj.transform.position, 2);
    }

    private void OnFireplaceActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForFireplaceEvent = currentDeltaTime;
        if (fireplaceEventObject.gameObj.GetComponent<FireplaceActivity>().OnActivityUpdate(fireplaceEventObject.eventTime.GetProgress()))
        {
            fireplaceEventObject.eventTime.Deactivate(activeActivites);
            fireplaceEventObject.eventTime.Reset();
            uiManager.HideIcon(2);
        }
    }
    private void OnFireplaceActivityFinished(int activityIndex)
    {
        fireplaceEventObject.eventTime.Deactivate(activeActivites);
        fireplaceEventObject.gameObj.GetComponent<FireplaceActivity>().ActivityTriggerEnd();
        fireplaceActivityFinished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "Fireplace";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnSkylightActivityStart(int activityIndex)
    {
        skylightEventObject.gameObj.GetComponent<SkylightActivity>().ActivityTriggerStart();
        uiManager.ShowIcon(iconPrefab, skylightEventObject.gameObj.transform.position, 3);
        HintManager.Instance.DisplayGameHint(HintType.Skylight);
    }
    private void OnSkylightActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForSkylightEvent = currentDeltaTime;
        if (skylightEventObject.gameObj.GetComponent<SkylightActivity>().OnActivityUpdate(skylightEventObject.eventTime.GetProgress()))
        {
            skylightEventObject.eventTime.Deactivate(activeActivites);
            skylightEventObject.eventTime.Reset();
            uiManager.HideIcon(3);
        }
    }
    private void OnSkylightActivityFinished(int activityIndex)
    {
        skylightEventObject.eventTime.Deactivate(activeActivites);
        skylightEventObject.gameObj.GetComponent<SkylightActivity>().ActivityTriggerEnd();
        skylightActivityFinished = true;

        if (!deathTrigger.IsActive())
        {
            deathCause = "Skylight";
            deathTrigger.Activate(activeActivites);
        }
    }

    private void OnToiletActivityStart(int activityIndex)
    {
        toiletEventObject.gameObj.GetComponent<ToiletActivity>().ActivityTriggerStart();
        uiManager.ShowIcon(iconPrefab, toiletEventObject.gameObj.transform.position, 4);
        HintManager.Instance.DisplayGameHint(HintType.Toilet);
    }

    private void OnToiletActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForToiletEvent = currentDeltaTime;
        if (toiletEventObject.gameObj.GetComponent<ToiletActivity>().OnActivityUpdate(toiletEventObject.eventTime.GetProgress()))
        {
            toiletEventObject.eventTime.Deactivate(activeActivites);
            toiletEventObject.eventTime.Reset();
            uiManager.HideIcon(4);
        }
    }
    private void OnToiletActivityFinished(int activityIndex)
    {
        toiletEventObject.eventTime.Deactivate(activeActivites);
        toiletEventObject.gameObj.GetComponent<ToiletActivity>().ActivityTriggerEnd();

        if (!deathTrigger.IsActive())
        {
            deathCause = "Toilet";
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
        nightActivity[nightIndex].Activate(activeActivites);
        playerController.Respawn();
    }

    private bool stopActivityDirector = false;
    private void OnWin(int activityIndex)
    {
        uiManager.WinGame();
        stopActivityDirector = true;
    }

    void OnDeath(int activityIndex)
    {
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
            //  if (window.eventTime.IsActive())
            //   {
            if (Physics.Raycast(playerController.transform.position, window.gameObj.transform.position - playerController.transform.position, out RaycastHit hit_a, 1000))
            {
                if (hit_a.collider.gameObject == window.gameObj && hit_a.distance < last_distance)
                {
                    last_distance = hit_a.distance;
                    closestOpeningPoint = window.gameObj;
                    break;
                }
            }
            //  }
        }

        // if (petdoorEventObject.eventTime.IsActive())
        // {
        //compare with a trigger collider instead of petdoor
        if (Physics.Raycast(playerController.transform.position, petdoorEventObject.gameObj.transform.position - playerController.transform.position, out RaycastHit hit_b, 1000))
        {
            if (hit_b.collider.gameObject == petdoorEventObject.gameObj && hit_b.distance < last_distance)
            {
                last_distance = hit_b.distance;
                closestOpeningPoint = petdoorEventObject.gameObj;
            }
        }
        //  }


        // if (skylightEventObject.eventTime.IsActive())
        // {
        if (Physics.Raycast(playerController.transform.position, skylightEventObject.gameObj.transform.position - playerController.transform.position, out RaycastHit hit_c, 1000))
        {
            if (hit_c.collider.gameObject == skylightEventObject.gameObj && hit_c.distance < last_distance)
            {
                last_distance = hit_c.distance;
                closestOpeningPoint = skylightEventObject.gameObj;
            }
        }
   // }

        if (closestOpeningPoint != null)
        {
            //rainAndThunderAudioSource.local_position = closestOpeningPoint != null ? closestOpeningPoint.transform.localPosition : playerController.transform.localPosition;

            //change 2d to 3d audio
        }
        else
        {
          //  rainAndThunderAudioSource.local_position = playerController.transform.localPosition;
            //change 3d to 2d audio
        }

    }

    void Update()
    {
        if (stopActivityDirector)
            return;

        currentDeltaTime += Time.deltaTime * 1000f;

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

        for (int currentIndex = 0; currentIndex < activeActivites.Count; currentIndex++)
            activeActivites[currentIndex].OnUpdate();
    }
}
