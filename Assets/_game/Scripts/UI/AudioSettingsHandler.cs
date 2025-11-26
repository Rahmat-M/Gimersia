using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Littale {
    public class AudioSettingsManager : MonoBehaviour {
        [Header("Components")]
        public AudioMixer mainMixer;

        [Header("Sliders")]
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider sfxSlider;

        [Header("Name of Mixer Parameters")]
        public string masterVolumeParam = "MasterVolume";
        public string musicVolumeParam = "MusicVolume";
        public string sfxVolumeParam = "SFXVolume";

        [Header("Audio Feedback")]
        public string sliderMoveSfx = "ButtonHover";
        public float sfxCooldown = 0.15f;

        private const string MASTER_KEY = "MasterVolume";
        private const string MUSIC_KEY = "MusicVolume";
        private const string SFX_KEY = "SFXVolume";

        private float lastSfxPlayTime;

        void Start() {
            float masterVol = PlayerPrefs.GetFloat(MASTER_KEY, 0.5f);
            float musicVol = PlayerPrefs.GetFloat(MUSIC_KEY, 0.5f);
            float sfxVol = PlayerPrefs.GetFloat(SFX_KEY, 0.5f);

            masterSlider.SetValueWithoutNotify(masterVol);
            musicSlider.SetValueWithoutNotify(musicVol);
            sfxSlider.SetValueWithoutNotify(sfxVol);

            ApplyVolumeToMixer(masterVolumeParam, masterVol);
            ApplyVolumeToMixer(musicVolumeParam, musicVol);
            ApplyVolumeToMixer(sfxVolumeParam, sfxVol);

            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        public void SetMasterVolume(float volume) {
            ApplyAndSave(masterVolumeParam, MASTER_KEY, volume);
            PlaySliderSfx();
        }

        public void SetMusicVolume(float volume) {
            ApplyAndSave(musicVolumeParam, MUSIC_KEY, volume);
            PlaySliderSfx();
        }

        public void SetSFXVolume(float volume) {
            ApplyAndSave(sfxVolumeParam, SFX_KEY, volume);
            PlaySliderSfx();
        }

        private void ApplyAndSave(string mixerParam, string prefsKey, float volume) {
            ApplyVolumeToMixer(mixerParam, volume);

            PlayerPrefs.SetFloat(prefsKey, volume);
        }

        private void ApplyVolumeToMixer(string mixerParam, float linearVolume) {
            float dbVolume = (linearVolume > 0.001f) ? Mathf.Log10(linearVolume) * 20 : -80f;

            mainMixer.SetFloat(mixerParam, dbVolume);
        }

        private void PlaySliderSfx() {
            if (Time.unscaledTime - lastSfxPlayTime > sfxCooldown) {
                // if (!string.IsNullOrEmpty(sliderMoveSfx) && SoundManager.Instance != null) {
                //     SoundManager.Instance.Play(sliderMoveSfx);
                //     lastSfxPlayTime = Time.unscaledTime;
                // }
            }
        }

        void OnDestroy() {
            PlayerPrefs.Save();
        }
    }
}