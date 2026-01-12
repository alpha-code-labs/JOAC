using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System;



public class NumberAnimation : MonoBehaviour
{
    public TextMeshProUGUI numberText; // Assign in Inspector
    public AudioSource tickSound; // Assign a ticking sound effect in the Inspector
    public AudioSource bookFlipSound;
    public Button FlipBookButton;
    public float minDuration = .5f;
    public float maxDuration = 3f;
    public Color startColor = new Color(1, 1, 1, 0); // Transparent
    public Color endColor = new Color(1, 1, 1, 1); // Fully visible
    public Action<int> OnAnimationComplete;

    public int maxBallsToBall;
    public int ballsBalled;
    public int targetScore;
    public int currentScore;

    private bool didWin;
    private bool didLoose;

    private int targetNumber;
    private float animationDuration;

    public Animator BookAnimator;
   
    void Start()
    {
        FlipBookButton.onClick.AddListener(StartRandomNumberAnimation);
        //StartRandomNumberAnimation();
    }

    public void StartRandomNumberAnimation()
    {
        targetNumber = UnityEngine.Random.Range(0, 601); // Pick a random number
        
        float normalizedValue = (float)targetNumber / 600f; // Normalize between 0 and 1
        animationDuration = Mathf.Lerp(minDuration, maxDuration, normalizedValue);

        bookFlipSound.Play();
        BookAnimator.SetBool("Flipping", true);
        StartCoroutine(AnimateNumbers(targetNumber, animationDuration));
    }

    void Update()
    {

    }

    IEnumerator AnimateNumbers(int target, float duration)
    { 
        float elapsedTime = 0f;
        int currentNumber = 0;
        FlipBookButton.interactable = false;
        numberText.color = startColor; // Start transparent

        // Fade-in effect
        while (elapsedTime < 0.5f)
        {
            numberText.color = Color.Lerp(startColor, endColor, elapsedTime / 0.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float progress = Mathf.Pow(elapsedTime / duration, 2f); //Ease-in effect
            currentNumber = Mathf.RoundToInt(Mathf.Lerp(0, target, progress));
            numberText.text = currentNumber.ToString();

            if (tickSound) tickSound.Play(); // Play ticking sound

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        numberText.text = target.ToString(); // Final number

        // Fade-out effect
        elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
            numberText.color = Color.Lerp(endColor, startColor, elapsedTime / 0.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        bookFlipSound.Stop();
        BookAnimator.SetBool("Flipping", false);
        FlipBookButton.interactable = true;
        OnAnimationComplete?.Invoke(target);
        
    }

}
