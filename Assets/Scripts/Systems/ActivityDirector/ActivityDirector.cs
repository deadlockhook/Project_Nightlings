using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityDirector : MonoBehaviour
{
    private ActivityDirector directorInstance = null;
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

    private List<timedActivity> activeActivites;
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

        private bool active;
        private int triggerIndex;
        private float currentTime;
        private float triggerTime;
        private timedActivityTrigger actionStart;
        private timedActivityTrigger actionEnd;
        private timedActivityTrigger actionOnUpdate;
    }
    public class windowActivity
    {
        public windowActivity(GameObject _gameObj, float triggerTimeMilliSeconds, int triggerIndex, timedActivityTrigger actionStart, timedActivityTrigger actionEnd, timedActivityTrigger actionOnUpdate)
        {
            gameObj = _gameObj;
            eventTime = new timedActivity(triggerTimeMilliSeconds, triggerIndex, actionStart, actionEnd, actionOnUpdate);
        }

        public GameObject gameObj;
        public timedActivity eventTime;
    }

    [SerializeField] private List<GameObject> toySpawnGameObjects;
    [SerializeField] private List<GameObject> toyPrefabs;
    [SerializeField] private int minToySpawnLocations = 12; 
    [SerializeField] private int maxToySpawnLocations = 20;

    private float triggerWindowsActivityLogic = 2000.0f;
    private float windowsActivityTimeLimit = 500.0f;

    private List<windowActivity> windowEventObjects;

    private List<Vector3> toySpawnLocations;

    private float currentDeltaTime = 0;
    private float lastDeltaTime = 0;
    void Start()
    {
        activeActivites = new List<timedActivity>();
        windowEventObjects = new List<windowActivity>();
        lastDeltaTime = Time.deltaTime * 1000.0f;

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
            windowEventObjects.Add(new windowActivity(obj, windowsActivityTimeLimit, currentIndex, OnWindowActivityStart, OnWindowActivityFinished, OnWindowActivityUpdate));
        }

    }
    private void OnWindowActivityStart(int activityIndex)
    {
        windowActivity activityObject = windowEventObjects[activityIndex];

        activityObject.gameObj.GetComponent<WindowsActivity>().ActivityTriggerStart();
        Debug.Log("Activity Start");
      
    }

    private void OnWindowActivityUpdate(int activityIndex)
    {
        windowActivity activityObject = windowEventObjects[activityIndex];
        if (activityObject.gameObj.GetComponent<WindowsActivity>().OnActivityUpdate(activityObject.eventTime.GetProgress()))
        {
            activityObject.eventTime.Deactivate(activeActivites);
            activityObject.eventTime.Reset();
        }
    }
    private void OnWindowActivityFinished(int activityIndex)
    {
        windowActivity activityObject = windowEventObjects[activityIndex];
        activityObject.eventTime.Deactivate( activeActivites);
        activityObject.gameObj.GetComponent<WindowsActivity>().ActivityTriggerEnd();
        Debug.Log("Activity End");
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
    void Update()
    {
        currentDeltaTime += Time.deltaTime * 1000f;

        if (currentDeltaTime - lastDeltaTime >= triggerWindowsActivityLogic )
        {
            windowEventObjects[Mathf.Clamp(Random.Range(0, windowEventObjects.Count), 0, windowEventObjects.Count)].eventTime.Activate(activeActivites);
            lastDeltaTime = currentDeltaTime;
        }

        for (int currentIndex = 0;currentIndex  < activeActivites.Count;currentIndex++)
            activeActivites[currentIndex].OnUpdate();
    }
}
