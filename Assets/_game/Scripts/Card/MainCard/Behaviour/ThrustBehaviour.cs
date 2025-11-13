using PrimeTween;
using UnityEngine;

namespace Littale {
    public class ThrustBehaviour : MeleeBehaviour {

        public float distance = 2f;
        public float duration = 0.7f;

        private Sequence thrustSequence;

        protected override void Start() {
            StartThrust(distance, duration);
        }

        public void StartThrust(float distance, float duration) {
            // TODO: SFX and VFX

            float outDuration = duration * 0.4f;
            float backDuration = duration * 0.6f;

            Vector3 targetPosition = direction.normalized * distance;

            thrustSequence = Sequence.Create()
                .Chain(Tween.LocalPosition(transform,
                                            targetPosition,
                                            outDuration,
                                            Ease.OutQuad))
                .Chain(Tween.LocalPosition(transform,
                                            Vector3.zero,
                                            backDuration,
                                            Ease.InSine))
                .OnComplete(() => {
                    Destroy(gameObject);
                });
        }

        void OnDestroy() {
            thrustSequence.Stop();
        }
    }
}