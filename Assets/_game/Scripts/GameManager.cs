using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace Littale {
    public class GameManager : MonoBehaviour {
        public static GameManager Instance;

        // Define the different states of the game
        public enum GameState {
            Gameplay,
            Paused,
            GameOver,
            LevelUp,
            TreasureChest
        }

        // Store the current state of the game
        public GameState currentState;

        // Store the previous state of the game before it was paused
        public GameState previousState;

        [Header("Damage Text Settings")]
        public Canvas damageTextCanvas;
        public float textFontSize = 20;
        public TMP_FontAsset textFont;
        public Camera referenceCamera;

        [Header("Screens")]
        public GameObject pauseScreen;
        public GameObject resultsScreen;
        public LevelUpManager levelUpManager;
        int stackedLevelUps = 0; // If we try to StartLevelUp() multiple times.

        [Header("Results Screen Displays")]
        public Image chosenCharacterImage;
        public TMP_Text chosenCharacterName;
        public TMP_Text levelReachedDisplay;
        public TMP_Text timeSurvivedDisplay;

        private const float DEFAULT_TIME_LIMIT = 1800f;
        private const float DEFAULT_CLOCK_SPEED = 1f;
        // private float ClockSpeed => UILevelSelector.currentLevel?.clockSpeed ?? DEFAULT_CLOCK_SPEED;
        // private float TimeLimit => UILevelSelector.currentLevel?.timeLimit ?? DEFAULT_TIME_LIMIT;


        [Header("Stopwatch")]
        public float timeLimit; // The time limit in seconds
        float stopwatchTime; // The current time elapsed since the stopwatch started
        public TMP_Text stopwatchDisplay;

        bool levelEnded = false; // Has the time limit been reached?
        public GameObject reaperPrefab; // Spawns after time limit has been reached;

        PlayerStats[] players; // Tracks all players.

        // Getters for parity with older scripts.
        public bool isGameOver { get { return currentState == GameState.Paused; } }
        public bool choosingUpgrade { get { return currentState == GameState.LevelUp; } }

        // Gives us the time since the level has started.
        public float GetElapsedTime() { return stopwatchTime; }

        // Sums up the curse stat of all players and returns the value.
        public static float GetCumulativeCurse() {
            if (!Instance) return 1;

            float totalCurse = 0;
            foreach (PlayerStats p in Instance.players) {
                // totalCurse += p.Actual.curse; // TODO: Fix when Actual is back
            }
            return Mathf.Max(1, totalCurse);
        }

        // Sum up the levels of all players and returns the value.
        public static int GetCumulativeLevels() {
            if (!Instance) return 1;

            int totalLevel = 0;
            foreach (PlayerStats p in Instance.players) {
                totalLevel += p.level;
            }
            return Mathf.Max(1, totalLevel);
        }

        public PlayerCollector characterCollector;

        void Awake() {
            players = FindObjectsByType<PlayerStats>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            //Set the level's Time Limit
            // timeLimit = TimeLimit; // TODO: Fix when UILevelSelector is back

            //Warning check to see if there is another singleton of this kind already in the game
            if (Instance == null) {
                Instance = this;
            } else {
                Debug.LogWarning("EXTRA " + this + " DELETED");
                Destroy(gameObject);
            }

            DisableScreens();
        }

        void Start() {
            if (SoundManager.Instance != null) {
                SoundManager.Instance.Play("GameplayTheme");
            }
        }

        void Update() {
            switch (currentState) {
                case GameState.Gameplay:
                    // Code for the gameplay state
                    Time.timeScale = 1.0f;
                    CheckForPauseAndResume();
                    // UpdateStopwatch();
                    break;
                case GameState.Paused:
                    // Code for the paused state
                    CheckForPauseAndResume();
                    break;
                case GameState.GameOver:
                case GameState.TreasureChest:
                    Time.timeScale = 0;
                    break;
                case GameState.LevelUp:
                    break;
                default:
                    Debug.LogWarning("STATE DOES NOT EXIST");
                    break;
            }
        }

        IEnumerator GenerateFloatingTextCoroutine(string text, Transform target, float duration = 1f, float speed = 50f) {
            // Start generating the floating text.
            GameObject textObj = new GameObject("Damage Floating Text");
            RectTransform rect = textObj.AddComponent<RectTransform>();
            TextMeshProUGUI tmPro = textObj.AddComponent<TextMeshProUGUI>();
            tmPro.text = text;
            tmPro.horizontalAlignment = HorizontalAlignmentOptions.Center;
            tmPro.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmPro.fontSize = textFontSize;
            if (textFont) tmPro.font = textFont;
            rect.position = referenceCamera.WorldToScreenPoint(target.position);

            // Makes sure this is destroyed after the duration finishes.
            Destroy(textObj, duration);

            // Parent the generated text object to the canvas.
            textObj.transform.SetParent(Instance.damageTextCanvas.transform);
            textObj.transform.SetSiblingIndex(0);

            // Pan the text upwards and fade it away over time.
            WaitForEndOfFrame w = new WaitForEndOfFrame();
            float t = 0;
            float yOffset = 0;
            Vector3 lastKnownPosition = target.position;
            while (t < duration) {
                // If the RectTransform is missing for whatever reason, end this loop.
                if (!rect) break;

                // Fade the text to the right alpha value.
                tmPro.color = new Color(tmPro.color.r, tmPro.color.g, tmPro.color.b, 1 - t / duration);

                // Update the enemy's position if it is still around.
                if (target) lastKnownPosition = target.position;

                // Pan the text upwards.
                yOffset += speed * Time.deltaTime;
                rect.position = referenceCamera.WorldToScreenPoint(lastKnownPosition + new Vector3(0, yOffset));

                // Wait for a frame and update the time.
                yield return w;
                t += Time.deltaTime;
            }
        }

        public static void GenerateFloatingText(string text, Transform target, float duration = 1f, float speed = 1f) {
            // If the canvas is not set, end the function so we don't
            // generate any floating text.
            if (!Instance.damageTextCanvas) return;

            // Find a relevant camera that we can use to convert the world
            // position to a screen position.
            if (!Instance.referenceCamera) Instance.referenceCamera = Camera.main;

            Instance.StartCoroutine(Instance.GenerateFloatingTextCoroutine(
                text, target, duration, speed
            ));
        }

        // Define the method to change the state of the game
        public void ChangeState(GameState newState) {
            previousState = currentState;
            currentState = newState;
        }

        public void PauseGame() {
            if (currentState != GameState.Paused) {
                ChangeState(GameState.Paused);
                Time.timeScale = 0f; // Stop the game
                pauseScreen.SetActive(true); // Enable the pause screen

            }
        }

        public void ResumeGame() {
            if (currentState == GameState.Paused) {
                ChangeState(previousState);
                Time.timeScale = 1f; // Resume the game
                pauseScreen.SetActive(false); //Disable the pause screen
            }
        }

        // Define the method to check for pause and resume input
        void CheckForPauseAndResume() {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) {
                if (currentState == GameState.Paused) {
                    ResumeGame();
                } else {
                    PauseGame();
                }
            }
        }

        void DisableScreens() {
            pauseScreen.SetActive(false);
            resultsScreen.SetActive(false);
            levelUpManager.choiceWindow.gameObject.SetActive(false);
        }

        public void GameOver() {
            timeSurvivedDisplay.text = stopwatchDisplay.text;

            // Set the Game Over variables here.
            ChangeState(GameState.GameOver);
            Time.timeScale = 0f; //Stop the game entirely
            DisplayResults();

            // Save all the coins of all the players to the save file.
            foreach (PlayerStats p in players) {
                p.GetComponentInChildren<PlayerCollector>().SaveCoinsToStash();
            }
            // Add all players' coins to their save file, since the game has ended.
            foreach (PlayerStats p in players) {
                if (p.TryGetComponent(out PlayerCollector c)) {
                    c.SaveCoinsToStash();
                }
            }
        }

        void DisplayResults() {
            resultsScreen.SetActive(true);
        }

        public void AssignChosenCharacterUI(CharacterSO chosenCharacterData) {
            chosenCharacterImage.sprite = chosenCharacterData.Icon;
            chosenCharacterName.text = chosenCharacterData.Name;
        }

        public void AssignLevelReachedUI(int levelReachedData) {
            levelReachedDisplay.text = levelReachedData.ToString();
        }

        public Vector2 GetRandomPlayerLocation() {
            int chosenPlayer = Random.Range(0, players.Length);
            return new Vector2(players[chosenPlayer].transform.position.x, players[chosenPlayer].transform.position.y);
        }

        void UpdateStopwatch() {
            // stopwatchTime += Time.deltaTime * ClockSpeed; // TODO: Fix when UILevelSelector is back
            stopwatchTime += Time.deltaTime * DEFAULT_CLOCK_SPEED;
            UpdateStopwatchDisplay();

            if (stopwatchTime >= timeLimit && !levelEnded) {
                levelEnded = true;

                // Set the enemy/event spawner GameObject inactive to stop enemies from spawning and kill the remaining enemies onscreen.
                FindFirstObjectByType<SpawnManager>().gameObject.SetActive(false);
                foreach (EnemyStats e in FindObjectsByType<EnemyStats>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    e.SendMessage("Kill");

                // Spawn the Reaper off-camera
                Vector2 reaperOffset = Random.insideUnitCircle * 50f;
                Vector2 spawnPosition = GetRandomPlayerLocation() + reaperOffset;
                Instantiate(reaperPrefab, spawnPosition, Quaternion.identity);

            }
        }

        void UpdateStopwatchDisplay() {
            // Calculate the number of minutes and seconds that have elapsed
            int minutes = Mathf.FloorToInt(stopwatchTime / 60);
            int seconds = Mathf.FloorToInt(stopwatchTime % 60);

            // Update the stopwatch text to display the elapsed time
            stopwatchDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public void StartLevelUp() {
            ChangeState(GameState.LevelUp);

            // If the level up screen is already active, record it.
            if (levelUpManager.choiceWindow.gameObject.activeSelf) stackedLevelUps++;
            else {
                levelUpManager.ShowLevelUpOptions();
                Time.timeScale = 0f;    //Pause the game
            }
        }

        public void EndLevelUp() {
            Time.timeScale = 1f;    //Resume the game
            ChangeState(GameState.Gameplay);

            if (stackedLevelUps > 0) {
                stackedLevelUps--;
                StartLevelUp();
            }
        }
    }
}