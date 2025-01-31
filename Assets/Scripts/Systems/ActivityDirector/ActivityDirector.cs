using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityDirector : MonoBehaviour
{
    public static ActivityDirector directorInstance { get; private set; }
    private void Awake()
    {
        if (directorInstance == null)
        {
            directorInstance = this;
            DontDestroyOnLoad(directorInstance);
        }
        else
            Destroy(this);
    }

    public delegate void timedActivityTrigger(int val);

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
            actionOnUpdate(triggerIndex);

            if (currentTime >= triggerTime)
            {
                actionEnd(triggerIndex);
                Reset();
            }
        }
        public void Activate(List<timedActivity> activites) {
            activites.Add(this);
            active = true;
            actionStart(triggerIndex);
        }
        public void Deactivate(List<timedActivity> activites) { 
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

    private List<timedActivity> activeActivites;

    [SerializeField] private List<GameObject> toySpawnGameObjects;
    [SerializeField] private List<GameObject> toyPrefabs;
    [SerializeField] private int minToySpawnLocations = 12; 
    [SerializeField] private int maxToySpawnLocations = 20;

    private float triggerWindowsActivityLogic = 4000.0f;
    private float windowsActivityTimeLimit = 2000.0f;

    private float triggerPetdoorActivityLogic = 4000.0f;
    private float petdoorActivityTimeLimit = 2000.0f;

    private List<activityTrigger> windowEventObjects;
    private activityTrigger petdoorEventObject;

    private List<Vector3> toySpawnLocations;

    private float currentDeltaTime = 0;
    private float lastDeltaTimeForWindowEvents = 0;
    private float lastDeltaTimeForPetDoorEvents = 0;
    void Start()
    {
        activeActivites = new List<timedActivity>();
        windowEventObjects = new List<activityTrigger>();
        lastDeltaTimeForWindowEvents = Time.deltaTime * 1000.0f;
        lastDeltaTimeForPetDoorEvents = lastDeltaTimeForWindowEvents;

        toySpawnLocations = new List<Vector3>();

        if (toySpawnGameObjects == null)
            toySpawnGameObjects = new List<GameObject>();
       
        if (toyPrefabs == null)
            toyPrefabs = new List<GameObject>();

        for (int i = 0; i < toySpawnGameObjects.Count; i++)
        {
            toySpawnLocations.Add(toySpawnGameObjects[i].transform.position);
            Destroy(toySpawnGameObjects[i]);
        }

        GameObject[] windows = GameObject.FindGameObjectsWithTag("Activity_Window");

        for (int currentIndex = 0;currentIndex < windows.Length;currentIndex++)
        {
            GameObject obj = windows[currentIndex];
            windowEventObjects.Add(new activityTrigger(obj, windowsActivityTimeLimit, currentIndex, OnWindowActivityStart, OnWindowActivityFinished, OnWindowActivityUpdate));
        }

        petdoorEventObject = new activityTrigger(GameObject.FindGameObjectWithTag("Activity_PetDoor"), petdoorActivityTimeLimit, 0, OnPetDoorActivityStart, OnPetDoorActivityFinished, OnPetDoorActivityUpdate);

    }
    private void OnPetDoorActivityStart(int activityIndex)
    {
        petdoorEventObject.gameObj.GetComponent<PetDoorActivity>().ActivityTriggerStart();
    }

    private void OnPetDoorActivityUpdate(int activityIndex)
    {
        if (petdoorEventObject.gameObj.GetComponent<PetDoorActivity>().OnActivityUpdate(petdoorEventObject.eventTime.GetProgress()))
        {
            petdoorEventObject.eventTime.Deactivate(activeActivites);
            petdoorEventObject.eventTime.Reset();
        }
    }
    private void OnPetDoorActivityFinished(int activityIndex)
    {
        petdoorEventObject.eventTime.Deactivate(activeActivites);
        petdoorEventObject.gameObj.GetComponent<PetDoorActivity>().ActivityTriggerEnd();
    }
    private void OnWindowActivityStart(int activityIndex)
    {
        windowEventObjects[activityIndex].gameObj.GetComponent<WindowsActivity>().ActivityTriggerStart();
    }

    private void OnWindowActivityUpdate(int activityIndex)
    {
        activityTrigger activityObject = windowEventObjects[activityIndex];
        if (activityObject.gameObj.GetComponent<WindowsActivity>().OnActivityUpdate(activityObject.eventTime.GetProgress()))
        {
            activityObject.eventTime.Deactivate(activeActivites);
            activityObject.eventTime.Reset();
        }
    }
    private void OnWindowActivityFinished(int activityIndex)
    {
        activityTrigger activityObject = windowEventObjects[activityIndex];
        activityObject.eventTime.Deactivate( activeActivites);
        activityObject.gameObj.GetComponent<WindowsActivity>().ActivityTriggerEnd();
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
            GameObject selectedPrefab = toyPrefabs[Random.Range(0, toyPrefabs.Count)];
            Instantiate(selectedPrefab, spawnLocations[i], Quaternion.identity);
        }
    }

    void DispatchWindowEvents()
    {
        if (currentDeltaTime - lastDeltaTimeForWindowEvents >= triggerWindowsActivityLogic && windowEventObjects.Count > 0 )
        {
            activityTrigger activity = windowEventObjects[Mathf.Clamp(Random.Range(0, windowEventObjects.Count), 0, windowEventObjects.Count)];

            if (!activity.eventTime.IsActive())
                 activity.eventTime.Activate(activeActivites);

            lastDeltaTimeForWindowEvents = currentDeltaTime;
        }
    }

    void DispatchPetdoorEvent()
    {
        if (petdoorEventObject.gameObj && currentDeltaTime - lastDeltaTimeForPetDoorEvents >= triggerPetdoorActivityLogic && !petdoorEventObject.eventTime.IsActive())
        {
            petdoorEventObject.eventTime.Activate(activeActivites);
            lastDeltaTimeForPetDoorEvents = currentDeltaTime;
        }
    }
    void Update()
    {
        currentDeltaTime += Time.deltaTime * 1000f;

        DispatchWindowEvents();
        DispatchPetdoorEvent();

        for (int currentIndex = 0;currentIndex  < activeActivites.Count;currentIndex++)
            activeActivites[currentIndex].OnUpdate();
    }
}
