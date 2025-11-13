using UnityEngine;

namespace Littale {
    public class BobbingAnimator : MonoBehaviour {

        // To represent the bobbing animation of the object.
        [System.Serializable]
        public struct BobbingAnimation {
            public float frequency;
            public Vector2 direction;
        }
        public BobbingAnimation bobbingAnimation = new BobbingAnimation {
            frequency = 2f,
            direction = new Vector2(0, 0.3f)
        };

        float initialOffset;

        void Start() {
            initialOffset = Random.Range(0, bobbingAnimation.frequency);
        }

        void Update() {
            // Handle the animation of the object.
            transform.localPosition = bobbingAnimation.direction * Mathf.Sin((Time.time + initialOffset) * bobbingAnimation.frequency);
        }

    }
}