using UnityEngine.Audio;
using System;
using UnityEngine;

namespace Littale {
    public class SoundManager : MonoBehaviour {
        public static SoundManager Instance;

        public AudioMixerGroup musicMixerGroup;
        public AudioMixerGroup sfxMixerGroup;

        public Sound[] musicSounds;
        public Sound[] sfxSounds;

        void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            foreach (Sound s in musicSounds) {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = musicMixerGroup;
            }

            foreach (Sound s in sfxSounds) {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = sfxMixerGroup;
            }
        }

        public void Play(string name) {
            Sound s = FindSound(name);
            if (s == null) {
                Debug.LogWarning("Suara: " + name + " tidak ditemukan!");
                return;
            }

            if (!s.loop) {
                s.source.pitch = s.pitch * UnityEngine.Random.Range(0.9f, 1.1f);
                s.source.PlayOneShot(s.source.clip, s.volume);
            } else {
                s.source.Play();
            }
        }

        public void Stop(string name) {
            Sound s = FindSound(name);
            if (s == null) {
                Debug.LogWarning("Suara: " + name + " tidak ditemukan!");
                return;
            }
            s.source.Stop();
        }

        private Sound FindSound(string name) {
            Sound s = Array.Find(musicSounds, sound => sound.name == name);
            if (s == null) {
                s = Array.Find(sfxSounds, sound => sound.name == name);
            }
            return s;
        }
    }
}