using System.Collections;
using System.Collections.Generic;
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
                Debug.Log(_percentage);
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

    public List<GameObject> toyPrefabs;
    [SerializeField] private int minToySpawnLocations = 12;
    [SerializeField] private int maxToySpawnLocations = 20;

    private float triggerWindowsActivityLogicRangeStart = 35000.0f;
    private float triggerWindowsActivityLogicRangeEnd = 45000.0f;
    private float windowsActivityTimeLimit = 25000.0f;

    private float triggerPetdoorActivityLogicRangeStart = 45000.0f;
    private float triggerPetdoorActivityLogicRangeEnd = 55000.0f;
    private float petdoorActivityTimeLimit = 20000.0f;

    private float triggerBasementActivityLogicRangeStart = 3000.0f;
    private float triggerBasementActivityLogicRangeEnd = 5000.0f;
    private float basementHatchActivityTimeLimit = 2000.0f;

    private List<activityTrigger> windowEventObjects;
    private activityTrigger petdoorEventObject;
    private activityTrigger basementHatchEventObject;

    private List<Vector3> toySpawnLocations;

    private float currentDeltaTime = 0;
    private float lastDeltaTimeForWindowEvents = 0;
    private float lastDeltaTimeForPetDoorEvents = 0;
    private float lastDeltaTimeForBasementHatchEvent = 0;

    private timedActivity nightActivity;
    private timedActivity deathTrigger;

    public GameObject iconPrefab;

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        playerController = FindObjectOfType<PlayerController>();
        soundManager = FindObjectOfType<SoundManager>();
        activeActivites = new List<timedActivity>();
        windowEventObjects = new List<activityTrigger>();
        lastDeltaTimeForWindowEvents = Time.deltaTime * 1000.0f;
        lastDeltaTimeForPetDoorEvents = lastDeltaTimeForWindowEvents;

        nightActivity = new timedActivity(420000, 0, null, OnWin, null);
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

        SpawnToys();
        nightActivity.Activate(activeActivites);
    }

    private bool petdoorActivityFinished = false;
    private bool windowActivityFinished = false;
    private bool basementHatchActivityFinished = false;

    private void OnPetDoorActivityStart(int activityIndex)
    {
        petdoorEventObject.gameObj.GetComponent<PetDoorActivity>().ActivityTriggerStart();
        uiManager.ShowIcon(iconPrefab, petdoorEventObject.gameObj.transform.position, 0);
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
        deathTrigger.Activate(activeActivites);
    }

    private void OnBasementHatchActivityStart(int activityIndex)
    {
        basementHatchEventObject.gameObj.GetComponent<BasementHatch>().ActivityTriggerStart();
        uiManager.ShowIcon(iconPrefab, basementHatchEventObject.gameObj.transform.position, 0);
    }

    private void OnBasementHatchActivityUpdate(int activityIndex)
    {
        lastDeltaTimeForBasementHatchEvent = currentDeltaTime;

        if (basementHatchEventObject.gameObj.GetComponent<BasementHatch>().OnActivityUpdate(basementHatchEventObject.eventTime.GetProgress()))
        {
            basementHatchEventObject.eventTime.Deactivate(activeActivites);
            basementHatchEventObject.eventTime.Reset();
            uiManager.HideIcon(0);
        }
    }

    private void OnBasementHatchActivityFinished(int activityIndex)
    {
        basementHatchEventObject.eventTime.Deactivate(activeActivites);
        basementHatchEventObject.gameObj.GetComponent<BasementHatch>().ActivityTriggerEnd();
        basementHatchActivityFinished = true;
        deathTrigger.Activate(activeActivites);
    }

    private void OnWindowActivityStart(int activityIndex)
    {
        windowEventObjects[activityIndex].gameObj.GetComponent<WindowsActivity>().ActivityTriggerStart();
        uiManager.ShowIcon(iconPrefab, windowEventObjects[activityIndex].gameObj.transform.position, activityIndex);
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
        deathTrigger.Activate(activeActivites);  
    }

    public void SpawnToys()
    {
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

        if (basementHatchEventObject.gameObj && currentDeltaTime - lastDeltaTimeForBasementHatchEvent >= Random.Range(triggerBasementActivityLogicRangeStart, triggerBasementActivityLogicRangeEnd) && !basementHatchEventObject.eventTime.IsActive())
        {
            basementHatchEventObject.eventTime.Activate(activeActivites);
            lastDeltaTimeForBasementHatchEvent = currentDeltaTime;
        }
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
        uiManager.LoseGame();
        stopActivityDirector = true;
    }

    void Update()
    {
        if (stopActivityDirector)
            return;

        currentDeltaTime += Time.deltaTime * 1000f;

        //Dispatch on night 1
        DispatchWindowEvents();
        DispatchPetdoorEvent();

        //Dispatch on night 2
        DispatchBasementHatchEvent();

        for (int currentIndex = 0; currentIndex < activeActivites.Count; currentIndex++)
            activeActivites[currentIndex].OnUpdate();
    }
}