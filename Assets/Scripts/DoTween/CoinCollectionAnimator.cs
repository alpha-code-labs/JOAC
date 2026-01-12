using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;

public class CoinCollectionAnimator : MonoBehaviour
{
    [Header("UI References")]
    public Image coinImage;
    public TextMeshProUGUI coinText;

    [Header("Flying Coins")]
    public GameObject coinPrefab; // Drag a coin UI prefab here
    public Transform coinSpawnArea; // Area where coins spawn from
    public Transform coinPrefabContainer; // Container where flying coins will be instantiated
    public int numberOfFlyingCoins = 5;
    public float coinSpawnRadius = 200f;
    public float coinFlyDuration = 0.8f;
    public float coinSpawnDelay = 0.1f; // Delay between each coin spawn

    [Header("Animation Settings")]
    public float coinAnimationDelay = 0.5f;
    public float coinBounceScale = 1.01f;
    public float coinAnimationDuration = 0.8f;
    public float counterAnimationDuration = 1.2f;
    public int punchCount = 3; // Number of bounces

    [Header("Effects")]
    public ParticleSystem coinParticles; // Optional particle effect
    public AudioSource coinSound; // Optional sound effect

    public void PlayCoinCollectionAnimation(int coinsEarned)
    {
        StartCoroutine(AnimateCoinCollection(coinsEarned));
    }

    IEnumerator SpawnFlyingCoins()
    {
        if (coinPrefab == null)
        {
            yield break;
        }

        if (coinImage == null)
        {
            yield break;
        }

        // Find the Canvas - try multiple methods
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        if (canvas == null)
        {
            Debug.LogError("No Canvas found! Make sure CoinCollectionAnimator is in the Canvas hierarchy.");
            yield break;
        }

        // Get target position once before the loop
        RectTransform targetRect = coinImage.GetComponent<RectTransform>();
        if (targetRect == null)
        {
            Debug.LogError("Coin Image doesn't have RectTransform!");
            yield break;
        }

        // Use world position for more reliable movement
        Vector3 targetWorldPos = targetRect.position;

        for (int i = 0; i < numberOfFlyingCoins; i++)
        {

            // Create flying coin - make sure it's created as child of the correct container
            Transform parentTransform = coinPrefabContainer != null ? coinPrefabContainer : canvas.transform;
            GameObject flyingCoin = Instantiate(coinPrefab, parentTransform);
            RectTransform coinRect = flyingCoin.GetComponent<RectTransform>();

            if (coinRect == null)
            {
                Debug.LogError("Flying coin prefab doesn't have RectTransform! Make sure your prefab is a UI element.");
                Destroy(flyingCoin);
                continue;
            }

            // Calculate spawn position using world coordinates for more reliability
            Vector3 spawnWorldPos;

            if (coinSpawnArea != null)
            {
                RectTransform spawnRect = coinSpawnArea.GetComponent<RectTransform>();
                if (spawnRect != null)
                {
                    spawnWorldPos = spawnRect.position;
                    // Add random offset in world space
                    Vector2 randomOffset = Random.insideUnitCircle * coinSpawnRadius;
                    spawnWorldPos += new Vector3(randomOffset.x, randomOffset.y, 0);
                }
                else
                {
                    Debug.LogWarning("Spawn area doesn't have RectTransform, using center with offset");
                    Vector2 randomOffset = Random.insideUnitCircle * coinSpawnRadius;
                    spawnWorldPos = canvas.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
                }
            }
            else
            {
                Debug.LogWarning("Coin Spawn Area is null, using random position");
                Vector2 randomOffset = Random.insideUnitCircle * coinSpawnRadius;
                spawnWorldPos = canvas.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            }


            // Set initial position using world coordinates
            coinRect.position = spawnWorldPos;
            coinRect.localScale = Vector3.zero;

            // Make sure the coin is visible and interactable
            Image coinImage = flyingCoin.GetComponent<Image>();
            if (coinImage != null)
            {
                coinImage.raycastTarget = false; // Prevent UI blocking
            }

            // Animate coin appearing
            coinRect.DOScale(1f, 0.1f).SetEase(Ease.OutBack).SetUpdate(true);

            // Wait a bit, then fly to target
            yield return new WaitForSecondsRealtime(0.1f);

            // Capture variables for closure before starting animation
            int coinIndex = i + 1;
            GameObject capturedCoin = flyingCoin;

            // Use world position movement instead of anchored position for more reliable movement
            Tween moveTween = coinRect.DOMove(targetWorldPos, coinFlyDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true);

            Tween scaleTween = coinRect.DOScale(0.6f, coinFlyDuration)
                .SetUpdate(true);

            // Add completion callback to the move tween
            moveTween.OnComplete(() =>
            {

                // Punch the main coin image - ensure it returns to scale of 1
                if (this.coinImage != null && this.coinImage.transform != null)
                {
                    this.coinImage.transform.DOPunchScale(Vector3.one * 0.15f, 0.1f, 1, 0f)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            // Force scale back to 1,1,1
                            this.coinImage.transform.localScale = Vector3.one;
                        });
                }

                // Play sound
                if (coinSound != null)
                {
                    coinSound.Play();
                }

                // Destroy the flying coin
                if (capturedCoin != null)
                {
                    Destroy(capturedCoin);
                }
                else
                {
                    Debug.LogWarning($"Captured coin {coinIndex} is null!");
                }
            });

            // Add update callback to track progress
            moveTween.OnUpdate(() =>
            {
                if (coinIndex == 1) // Only log for first coin to avoid spam
                {
                    float progress = moveTween.ElapsedPercentage();
                }
            });

            yield return new WaitForSecondsRealtime(coinSpawnDelay);
        }

    }

    IEnumerator AnimateCoinCollection(int coinsEarned)
    {
        // Start with coin and text invisible
        if (coinImage != null)
        {
            coinImage.transform.localScale = Vector3.zero;
            coinImage.color = new Color(coinImage.color.r, coinImage.color.g, coinImage.color.b, 0f);
        }

        if (coinText != null)
        {
            coinText.transform.localScale = Vector3.zero;
            coinText.color = new Color(coinText.color.r, coinText.color.g, coinText.color.b, 0f);
            coinText.text = "0";
        }

        // Wait for initial delay
        yield return new WaitForSecondsRealtime(coinAnimationDelay);

        // Start flying coins animation
        StartCoroutine(SpawnFlyingCoins());

        // Animate main coin image appearing with bounce
        if (coinImage != null)
        {
            Sequence coinSequence = DOTween.Sequence();
            coinSequence.Append(coinImage.transform.DOScale(coinBounceScale, coinAnimationDuration * 0.6f)
                .SetEase(Ease.OutBack).SetUpdate(true));
            coinSequence.Join(coinImage.DOFade(1f, coinAnimationDuration * 0.4f).SetUpdate(true));
            coinSequence.Append(coinImage.transform.DOPunchScale(Vector3.one * 0.02f, coinAnimationDuration * 0.2f, punchCount)
                .SetUpdate(true));
            coinSequence.Append(coinImage.transform.DOScale(1f, 0.02f).SetUpdate(true));

            // Play sound effect if available
            if (coinSound != null)
                coinSound.Play();

            // Play particle effect if available
            if (coinParticles != null)
                coinParticles.Play();
        }

        // Wait a bit, then animate text appearing
        yield return new WaitForSecondsRealtime(0.3f);

        if (coinText != null)
        {
            // Text appears with scale
            coinText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
            coinText.DOFade(1f, 0.3f).SetUpdate(true);

            // Animate the counter from 0 to earned amount
            float elapsedTime = 0f;
            int startValue = 0;
            float lastPunchTime = 0f;

            while (elapsedTime < counterAnimationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / counterAnimationDuration;
                progress = Mathf.Clamp01(progress);

                // Use easing for smooth counter animation
                float easedProgress = DOVirtual.EasedValue(0f, 1f, progress, Ease.OutQuart);
                int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, coinsEarned, easedProgress));

                coinText.text = "+" + currentValue.ToString();

                // Add periodic punch effects that don't affect final scale (very subtle)
                if (progress > 0.2f && progress < 0.8f && elapsedTime - lastPunchTime > 0.2f)
                {
                    // Very subtle punch that returns to original scale
                    coinText.transform.DOPunchScale(Vector3.one * 0.03f, 0.08f, 1, 0f).SetUpdate(true);
                    lastPunchTime = elapsedTime;
                }

                yield return null;
            }

            // Ensure final value is exact
            coinText.text = "+" + coinsEarned.ToString();

            // Final celebration effect - very subtle
            coinText.transform.DOPunchScale(Vector3.one * 0.06f, 0.4f, 1).SetUpdate(true);

            // Optional: Flash the text color
            Color originalColor = coinText.color;
            coinText.DOColor(Color.yellow, 0.2f).SetUpdate(true)
                .OnComplete(() => coinText.DOColor(originalColor, 0.3f).SetUpdate(true));
        }
    }

    // Call this method to reset the animation elements
    public void ResetCoinAnimation()
    {
        if (coinImage != null)
        {
            coinImage.transform.localScale = Vector3.zero;
            coinImage.color = new Color(coinImage.color.r, coinImage.color.g, coinImage.color.b, 0f);
        }

        if (coinText != null)
        {
            coinText.transform.localScale = Vector3.zero;
            coinText.color = new Color(coinText.color.r, coinText.color.g, coinText.color.b, 0f);
            coinText.text = "0";
        }
    }
}