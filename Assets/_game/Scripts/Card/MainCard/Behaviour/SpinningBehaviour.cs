using PrimeTween;
using UnityEngine;

namespace Littale {
    public class SpinningBehaviour : MeleeBehaviour {

        public Transform orbitTransform;

        public float brushSpinSpeed = 1080f;
        public Ease spinEase = Ease.OutSine;
        public float spinDuration = 0.5f;

        Tween orbitTween;
        Tween brushSpinTween;

        protected override void Start() {
            StartSpinAttack(brushSpinSpeed, spinDuration, spinEase);
        }

        public void StartSpinAttack(float brushSpinSpeed, float spinDuration, Ease spinEase) {
            if (orbitTransform == null) {
                Debug.LogError("orbitTransform belum di-assign di prefab SpinningAttack!", this);
                Destroy(gameObject);
                return;
            }

            // (Opsional) Mainkan SFX, partikel, aktifkan collider di sini
            // AudioManager.Instance.Play("PlayerSpinAttack");
            // brushVisualTransform.GetComponent<ParticleSystem>().Play();
            // brushVisualTransform.GetComponent<Collider2D>().enabled = true;

            orbitTween = Tween.EulerAngles(orbitTransform, Vector3.zero, new Vector3(0, 0, 360),
                                                spinDuration,
                                                spinEase,
                                                cycleMode: CycleMode.Restart);

            brushSpinTween = Tween.EulerAngles(transform, Vector3.zero,
                                                    new Vector3(0, 0, brushSpinSpeed * spinDuration),
                                                    spinDuration,
                                                    Ease.Linear);


            Destroy(orbitTransform.gameObject, spinDuration);
        }

    }
}