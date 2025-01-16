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
    public struct timedActivity
    {
        public timedActivity(float _triggerTimeMilliSeconds, int _triggerIndex, timedActivityTrigger _action)
        {
            currentTime = 0;
            triggerTime = _triggerTimeMilliSeconds;
            action = _action;
            triggerIndex = _triggerIndex;
        }

        void OnUpdate()
        {
            currentTime += (Time.deltaTime * 1000f);
            if (currentTime >= triggerTime)
            {
                action(triggerIndex);
                Reset();
            }
        }

        void Reset()
        {
            currentTime = 0;
        }

        float GetProgress()
        {
            return currentTime / triggerTime;
        }

        private int triggerIndex;
        private float currentTime;
        private float triggerTime;
        private timedActivityTrigger action;
    }

    [SerializeField] private List<GameObject> toySpawnGameObjects;
    [SerializeField] private List<GameObject> toyPrefabs;
    [SerializeField] private int minToySpawnLocations = 12; // Minimum number of locations to select
    [SerializeField] private int maxToySpawnLocations = 20; // Maximum number of locations to select

    private List<Vector3> toySpawnLocations;
    void Start()
    {
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

            //Select random toy prefab
            //Spawn toy at location
        }

    }


    void Update()
    {
        
    }
}
