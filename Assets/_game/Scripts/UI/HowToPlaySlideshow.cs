using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Gimersia {
    public class HowToPlaySlideshow : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Transform slideContainer;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private Ease animationEase = Ease.OutQuad;
        [SerializeField] private float slideOffset = 1000f; // Distance to slide in/out

        private int currentIndex = 0;
        private List<GameObject> slides = new List<GameObject>();
        private bool isAnimating = false;

        private void Start() {
            InitializeSlides();

            if (nextButton != null)
                nextButton.onClick.AddListener(OnNext);

            if (prevButton != null)
                prevButton.onClick.AddListener(OnPrev);

            UpdateButtons();
        }

        private void InitializeSlides() {
            slides.Clear();
            if (slideContainer == null) {
                Debug.LogError("Slide Container is not assigned in HowToPlaySlideshow!");
                return;
            }

            foreach (Transform child in slideContainer) {
                slides.Add(child.gameObject);
                // Ensure RectTransform and CanvasGroup exist for animation
                if (child.GetComponent<RectTransform>() == null) child.gameObject.AddComponent<RectTransform>();
                if (child.GetComponent<CanvasGroup>() == null) child.gameObject.AddComponent<CanvasGroup>();

                child.gameObject.SetActive(false);
            }

            if (slides.Count > 0) {
                currentIndex = 0;
                GameObject firstSlide = slides[0];
                firstSlide.SetActive(true);
                ResetSlidePosition(firstSlide);
            }
        }

        private void ResetSlidePosition(GameObject slide) {
            RectTransform rect = slide.GetComponent<RectTransform>();
            CanvasGroup cg = slide.GetComponent<CanvasGroup>();
            cg.alpha = 1f;
        }

        private void UpdateButtons() {
            if (prevButton != null)
                prevButton.interactable = !isAnimating && currentIndex > 0;

            if (nextButton != null)
                nextButton.interactable = !isAnimating && currentIndex < slides.Count - 1;
        }

        private void OnNext() {
            if (isAnimating || currentIndex >= slides.Count - 1) return;
            ShowSlide(currentIndex + 1, true);
        }

        private void OnPrev() {
            if (isAnimating || currentIndex <= 0) return;
            ShowSlide(currentIndex - 1, false);
        }

        private void ShowSlide(int newIndex, bool isNext) {
            isAnimating = true;
            UpdateButtons();

            GameObject currentSlide = slides[currentIndex];
            GameObject nextSlide = slides[newIndex];

            RectTransform currentRect = currentSlide.GetComponent<RectTransform>();
            CanvasGroup currentCg = currentSlide.GetComponent<CanvasGroup>();

            RectTransform nextRect = nextSlide.GetComponent<RectTransform>();
            CanvasGroup nextCg = nextSlide.GetComponent<CanvasGroup>();

            // Setup next slide
            nextSlide.SetActive(true);
            float startX = isNext ? slideOffset : -slideOffset;
            float endX = isNext ? -slideOffset : slideOffset;

            nextRect.anchoredPosition = new Vector2(startX, nextRect.anchoredPosition.y);
            nextCg.alpha = 0f;

            // Animate Current Slide Out
            Tween.UIAnchoredPositionX(currentRect, endX, animationDuration, animationEase);
            Tween.Alpha(currentCg, 0f, animationDuration, animationEase)
                .OnComplete(() => currentSlide.SetActive(false));

            // Animate Next Slide In
            Tween.UIAnchoredPositionX(nextRect, 0f, animationDuration, animationEase);
            Tween.Alpha(nextCg, 1f, animationDuration, animationEase)
                .OnComplete(() => {
                    isAnimating = false;
                    currentIndex = newIndex;
                    UpdateButtons();
                });
        }

        private void OnDestroy() {
            if (nextButton != null)
                nextButton.onClick.RemoveListener(OnNext);

            if (prevButton != null)
                prevButton.onClick.RemoveListener(OnPrev);
        }
    }
}
