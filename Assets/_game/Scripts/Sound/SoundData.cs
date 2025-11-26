using UnityEngine;
using UnityEngine.Audio;

namespace Littale {
    [CreateAssetMenu(fileName = "New Sound Data", menuName = "Littale/New Sound")]
    public class SoundData : ScriptableObject {
        [Header("Audio Clips")]
        [Tooltip("You can add multiple clips for random selection. otherwise, just add one clip.")]
        public AudioClip[] clips;
        public AudioMixerGroup mixerGroup;

        [Header("Properties")]
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;
        public bool useRandomPitch = true;
        [Range(-0.5f, 0.5f)] public float randomPitchRange = 0.2f;
        [Range(0f, 1f)] public float spatialBlend = 0f; // 0 = 2D, 1 = 3D

        public AudioClip GetAudioClip() {
            if (clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }
    }
}