using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn mobil traffic di depan pemain (ala Traffic Rider) dengan object pooling.
/// Asumsi: jalan memanjang di sumbu Z dan pemain bergerak ke +Z.
/// </summary>
public class TrafficSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Lane
    {
        public float xPosition;
        public bool oncoming;
        public Vector2 speedRange = new Vector2(10f, 18f);
        [HideInInspector] public float speed;
    }

    [Header("Referensi")]
    [SerializeField] Transform player;
    [SerializeField] TrafficCar[] carPrefabs;
    [Header("Lane")]
    [SerializeField] Lane[] lanes;
   
    [SerializeField] bool useSameYAsPlayer = true;
    [SerializeField] float spawnY = 0f;

    [Header("Jarak Spawn / Despawn")]
    [SerializeField] float spawnDistanceMin = 120f;
    [SerializeField] float spawnDistanceMax = 160f;
    
    [SerializeField] float oncomingExtraDistance = 30f;
    [SerializeField] float despawnBehindPlayer = 30f;
    
    [SerializeField] float despawnAheadOfPlayer = 220f;

    [Header("Pengaturan Spawn")]
    [SerializeField] Vector2 spawnInterval = new Vector2(0.8f, 2f);
    [SerializeField] int maxActiveCars = 15;
    
    [SerializeField] float minGapInLane = 25f;
   
    [SerializeField] float blockCheckRange = 20f;
    [SerializeField] int prewarmPerPrefab = 3;

    readonly List<TrafficCar> activeCars = new List<TrafficCar>();
    Queue<TrafficCar>[] pools;
    float timer;

    void Awake()
    {
        TrafficCar.UnfreezeAll();

        pools = new Queue<TrafficCar>[carPrefabs.Length];
        for (int i = 0; i < carPrefabs.Length; i++)
        {
            pools[i] = new Queue<TrafficCar>();
            for (int j = 0; j < prewarmPerPrefab; j++)
            {
                TrafficCar car = Instantiate(carPrefabs[i], transform);
                car.prefabIndex = i;
                car.gameObject.SetActive(false);
                pools[i].Enqueue(car);
            }
        }

        foreach (Lane lane in lanes)
            lane.speed = Random.Range(lane.speedRange.x, lane.speedRange.y);
    }

    void OnEnable()
    {
        CarHandler.OnPlayerCrashed += TrafficCar.FreezeAll;
    }

    void OnDisable()
    {
        CarHandler.OnPlayerCrashed -= TrafficCar.FreezeAll;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else Debug.LogError("TrafficSpawner: Player tidak ditemukan. Pastikan mobil pemain ber-tag 'Player'.");
        }

        if (useSameYAsPlayer && player != null)
            spawnY = player.position.y;

        ResetTimer();
    }

    void Update()
    {
        if (player == null || carPrefabs.Length == 0 || lanes.Length == 0)
            return;

        if (TrafficCar.IsFrozen)
            return;

        HandleDespawn();

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            TrySpawn();
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(spawnInterval.x, spawnInterval.y);
    }

    void TrySpawn()
    {
        if (activeCars.Count >= maxActiveCars)
            return;

        float baseZ = player.position.z + Random.Range(spawnDistanceMin, spawnDistanceMax);

        int startLane = Random.Range(0, lanes.Length);
        for (int i = 0; i < lanes.Length; i++)
        {
            int laneIndex = (startLane + i) % lanes.Length;
            float z = baseZ + (lanes[laneIndex].oncoming ? oncomingExtraDistance : 0f);

            if (IsLaneFree(laneIndex, z))
            {
                SpawnCar(laneIndex, z);
                return;
            }
        }
    }

    bool IsLaneFree(int laneIndex, float z)
    {
        int blockedOtherLanes = 0;
        bool[] laneCounted = new bool[lanes.Length];

        foreach (TrafficCar car in activeCars)
        {
            float dz = Mathf.Abs(car.transform.position.z - z);

            if (car.laneIndex == laneIndex && dz < minGapInLane)
                return false;

            if (car.laneIndex != laneIndex && dz < blockCheckRange && !laneCounted[car.laneIndex])
            {
                laneCounted[car.laneIndex] = true;
                blockedOtherLanes++;
            }
        }

        return blockedOtherLanes < lanes.Length - 1;
    }

    void SpawnCar(int laneIndex, float z)
    {
        int prefabIndex = Random.Range(0, carPrefabs.Length);
        TrafficCar car = GetFromPool(prefabIndex);
        Lane lane = lanes[laneIndex];

        car.transform.SetPositionAndRotation(
            new Vector3(lane.xPosition, spawnY, z),
            lane.oncoming ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity);

        car.laneIndex = laneIndex;
        car.gameObject.SetActive(true);
        car.Init(lane.speed, lane.oncoming);

        activeCars.Add(car);
    }

    void HandleDespawn()
    {
        float playerZ = player.position.z;

        for (int i = activeCars.Count - 1; i >= 0; i--)
        {
            TrafficCar car = activeCars[i];
            float z = car.transform.position.z;

            if (z < playerZ - despawnBehindPlayer || z > playerZ + despawnAheadOfPlayer + oncomingExtraDistance)
                ReturnToPool(car, i);
        }
    }

    TrafficCar GetFromPool(int prefabIndex)
    {
        if (pools[prefabIndex].Count > 0)
            return pools[prefabIndex].Dequeue();

        TrafficCar car = Instantiate(carPrefabs[prefabIndex], transform);
        car.prefabIndex = prefabIndex;
        return car;
    }

    void ReturnToPool(TrafficCar car, int activeIndex)
    {
        activeCars.RemoveAt(activeIndex);
        car.gameObject.SetActive(false);
        pools[car.prefabIndex].Enqueue(car);
    }

    void OnDrawGizmosSelected()
    {
        if (lanes == null) return;
        float z = player != null ? player.position.z : transform.position.z;
        foreach (Lane lane in lanes)
        {
            Gizmos.color = lane.oncoming ? Color.red : Color.yellow;
            Gizmos.DrawLine(
                new Vector3(lane.xPosition, spawnY, z),
                new Vector3(lane.xPosition, spawnY, z + spawnDistanceMax));
        }
    }
}
