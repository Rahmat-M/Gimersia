using TMPro;
using UnityEngine;

namespace Littale {
    public class WaveUIHandler : MonoBehaviour {

        [SerializeField] TMP_Text waveNumberText;
        [SerializeField] TMP_Text waveTimeText;

        public string prefix = "Wave ";

        void Start() {
            UpdateWaveNumber(0);
            UpdateWaveTime(0);
        }

        public void UpdateWaveNumber(int waveNumber) {
            string textToShow = prefix + (waveNumber + 1).ToString();

            if (SpawnManager.instance == null) {
                waveNumberText.text = textToShow;
                return;
            }

            if (SpawnManager.instance.state == SpawnManager.WaveState.Spawning) {
                waveNumberText.text = textToShow;
            } else {
                waveNumberText.text = textToShow + " (Rest)";
            }
        }

        public void UpdateWaveTime(float time) {
            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time - minutes * 60);

            waveTimeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }

    }
}