using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;

namespace Littale {
    public class SpawnManager : MonoBehaviour {

        public UnityEvent<float> OnTimerTick;
        public UnityEvent<int> OnWaveUpdated;
        public UnityEvent OnIntermezzoStart;
        public UnityEvent OnIntermezzoEnd;

        public enum WaveState { Spawning, Intermezzo, Finished }

        [Header("Game State")]
        public WaveState state = WaveState.Spawning;
        public int currentWaveIndex; //The index of the current wave [Remember, a list starts from 0]

        [Header("Intermezzo Settings")]
        public float intermezzoDuration = 30f; // Durasi istirahat (30 detik)
        [HideInInspector] public float currentIntermezzoTimer;

        [Header("Wave Data")]
        public WaveData[] data;
        public Camera referenceCamera;

        [Tooltip("If there are more than this number of enemies, stop spawning any more. For performance.")]
        public int maximumEnemyCount = 300;

        [Header("Results UI")]
        [SerializeField] PopupPanel resultsPanel;
        [SerializeField] TMP_Text headerText;
        [SerializeField] TMP_Text waveText;
        [SerializeField] TMP_Text coinsText;

        int currentWaveSpawnCount = 0; // Tracks how many enemies current wave has spawned.
        List<GameObject> existingSpawns = new List<GameObject>();
        float spawnTimer; // Timer used to determine when to spawn the next group of enemy.
        float currentWaveDuration = 0f;
        public bool boostedByCurse = true;

        public static SpawnManager instance;
        bool waveEnded = false;

        void Start() {
            if (instance) Debug.LogWarning("There is more than 1 Spawn Manager in the Scene! Please remove the extras.");
            instance = this;
            state = WaveState.Spawning;
        }

        void Update() {
            if (waveEnded) return;

            switch (state) {
                case WaveState.Spawning:
                    HandleSpawningState();
                    OnTimerTick?.Invoke(currentWaveDuration);
                    break;
                case WaveState.Intermezzo:
                    HandleIntermezzoState();
                    OnTimerTick?.Invoke(currentIntermezzoTimer);
                    break;
                case WaveState.Finished:
                    resultsPanel.Open();
                    Time.timeScale = 0f;
                    if (currentWaveIndex >= data.Length - 1) {
                        waveText.text = "10";
                        headerText.text = "Victory!";
                        coinsText.text = "Total Coins: " + GameManager.Instance.characterCollector.GetCoins();
                    } else {
                        waveText.text = (currentWaveIndex + 1).ToString();
                        headerText.text = "Defeat!";
                        coinsText.text = "Total Coins: " + GameManager.Instance.characterCollector.GetCoins();
                    }
                    waveEnded = true;
                    break;
            }
        }

        void HandleSpawningState() {
            spawnTimer -= Time.deltaTime;
            currentWaveDuration += Time.deltaTime;

            if (spawnTimer <= 0) {
                if (HasWaveEnded()) {
                    EndCurrentWave();
                    return;
                }

                if (!CanSpawn()) {
                    ActivateCooldown();
                    return;
                }

                SpawnEnemies();
                ActivateCooldown();
            }
        }

        void HandleIntermezzoState() {
            currentIntermezzoTimer -= Time.deltaTime;

            if (currentIntermezzoTimer <= 0) StartNextWave();
        }

        void EndCurrentWave() {
            // existingSpawns.ForEach(x => { if(x) Destroy(x); });
            // existingSpawns.Clear();

            // Check if we have gone through all the waves.
            if (currentWaveIndex >= data.Length - 1) {
                state = WaveState.Finished;
                GameManager.Instance.GameOver();
                enabled = false;
                return;
            }

            state = WaveState.Intermezzo;
            SoundManager.Instance.PlayUnique("IntermezzoTheme");
            currentIntermezzoTimer = intermezzoDuration;
            OnWaveUpdated?.Invoke(currentWaveIndex);
            OnIntermezzoStart?.Invoke();
        }

        void StartNextWave() {
            currentWaveIndex++;

            currentWaveDuration = 0f;
            currentWaveSpawnCount = 0;

            state = WaveState.Spawning;
            SoundManager.Instance.PlayUnique("GameplayTheme");
            OnWaveUpdated?.Invoke(currentWaveIndex);
            OnIntermezzoEnd?.Invoke();
        }

        void SpawnEnemies() {
            // Get the array of enemies that we are spawning for this tick.
            GameObject[] spawns = data[currentWaveIndex].GetSpawns(EnemyStats.count);

            foreach (GameObject prefab in spawns) {
                if (!CanSpawn()) continue;
                existingSpawns.Add(Instantiate(prefab, GeneratePosition(), Quaternion.identity));
                currentWaveSpawnCount++;
            }
        }

        public void SkipIntermezzo() {
            if (state != WaveState.Intermezzo) return;
            currentIntermezzoTimer = 0f;
            StartNextWave();
        }

        // Resets the spawn interval.
        public void ActivateCooldown() {
            // float curseBoost = boostedByCurse ? GameManager.GetCumulativeCurse() : 1;
            float curseBoost = 1; // TODO: Disable curse boost for now.
            spawnTimer += data[currentWaveIndex].GetSpawnInterval() / curseBoost;
        }

        // Do we meet the conditions to be able to continue spawning?
        public bool CanSpawn() {
            // Don't spawn anymore if we exceed the max limit.
            if (HasExceededMaxEnemies()) return false;

            // Don't spawn if we exceeded the max spawns for the wave.
            if (instance.currentWaveSpawnCount >= instance.data[instance.currentWaveIndex].totalSpawns) return false;

            // Don't spawn if we exceeded the wave's duration.
            if (instance.currentWaveDuration > instance.data[instance.currentWaveIndex].duration) return false;
            return true;
        }

        // Allows other scripts to check if we have exceeded the maximum number of enemies.
        public static bool HasExceededMaxEnemies() {
            if (!instance) return false; // If there is no spawn manager, don't limit max enemies.
            if (EnemyStats.count > instance.maximumEnemyCount) return true;
            return false;
        }

        public bool HasWaveEnded() {
            WaveData currentWave = data[currentWaveIndex];

            // If waveDuration is one of the exit conditions, check how long the wave has been running.
            // If current wave duration is not greater than wave duration, do not exit yet.
            if ((currentWave.exitConditions & WaveData.ExitCondition.waveDuration) > 0)
                if (currentWaveDuration < currentWave.duration) return false;

            // If reachedTotalSpawns is one of the exit conditions, check if we have spawned enough
            // enemies. If not, return false.
            if ((currentWave.exitConditions & WaveData.ExitCondition.reachedTotalSpawns) > 0)
                if (currentWaveSpawnCount < currentWave.totalSpawns) return false;

            // Otherwise, if kill all is checked, we have to make sure there are no more enemies first.
            existingSpawns.RemoveAll(item => item == null);
            if (currentWave.mustKillAll && existingSpawns.Count > 0)
                return false;

            return true;
        }

        public float GetTimer() {
            switch (state) {
                case WaveState.Spawning:
                    return spawnTimer;
                case WaveState.Intermezzo:
                    return currentIntermezzoTimer;
                default:
                    return 0f;
            }
        }

        void Reset() {
            referenceCamera = Camera.main;
        }

        // Creates a new location where we can place the enemy at.
        public static Vector3 GeneratePosition() {
            // If there is no reference camera, then get one.
            if (!instance.referenceCamera) instance.referenceCamera = Camera.main;

            // Give a warning if the camera is not orthographic.
            if (!instance.referenceCamera.orthographic)
                Debug.LogWarning("The reference camera is not orthographic! This will cause enemy spawns to sometimes appear within camera boundaries!");

            // Generate a position outside of camera boundaries using 2 random numbers.
            float x = Random.Range(0f, 1f), y = Random.Range(0f, 1f);

            // Create a viewport position with Z explicitly set to the camera's near clip plane or 0
            Vector3 viewportPos;

            switch (Random.Range(0, 2)) {
                case 0:
                default:
                    viewportPos = new Vector3(Mathf.Round(x), y, instance.referenceCamera.nearClipPlane);
                    break;
                case 1:
                    viewportPos = new Vector3(x, Mathf.Round(y), instance.referenceCamera.nearClipPlane);
                    break;
            }

            // Convert to world point
            Vector3 worldPos = instance.referenceCamera.ViewportToWorldPoint(viewportPos);

            // Force Z to 0
            worldPos.z = 0;
            return worldPos;
        }

        // Checking if the enemy is within the camera's boundaries.
        public static bool IsWithinBoundaries(Transform checkedObject) {
            // Get the camera to check if we are within boundaries.
            Camera c = instance && instance.referenceCamera ? instance.referenceCamera : Camera.main;

            Vector2 viewport = c.WorldToViewportPoint(checkedObject.position);
            if (viewport.x < 0f || viewport.x > 1f) return false;
            if (viewport.y < 0f || viewport.y > 1f) return false;
            return true;
        }
    }
}