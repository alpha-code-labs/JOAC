using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeeklyStudyManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject[] dayContainers; // 4 containers for Mon-Thu
    public TextMeshProUGUI[] dayIndicators; // Text components for day names
    public GameObject[] optionsContainers; // Containers with study/play buttons
    public GameObject[] questionnaireContainers; // Questionnaire containers
    public Button[] studyButtons; // "I want to study" buttons
    public Button[] playButtons; // "I want to play" buttons
    public Button[] nextButtons; // Next buttons for each day

    [Header("End Game UI")]
    public GameObject successScene;
    public GameObject retryPrompt;

    private int currentDay = 0;
    private Dictionary<int, bool> studyChoices = new Dictionary<int, bool>();
    private string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday" };

    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        ShowDay(0);
    }

    void InitializeUI()
    {
        // Set day indicator texts
        for (int i = 0; i < dayIndicators.Length; i++)
        {
            dayIndicators[i].text = dayNames[i];
        }

        // Hide all containers initially
        for (int i = 0; i < dayContainers.Length; i++)
        {
            dayContainers[i].SetActive(false);
            questionnaireContainers[i].SetActive(false);
        }

        // Hide end game UI
        if (successScene) successScene.SetActive(false);
        if (retryPrompt) retryPrompt.SetActive(false);
    }

    void SetupButtonListeners()
    {
        // Setup study buttons
        for (int i = 0; i < studyButtons.Length; i++)
        {
            int dayIndex = i; // Capture for closure
            studyButtons[i].onClick.AddListener(() => OnStudyChoice(dayIndex));
        }

        // Setup play buttons
        for (int i = 0; i < playButtons.Length; i++)
        {
            int dayIndex = i; // Capture for closure
            playButtons[i].onClick.AddListener(() => OnPlayChoice(dayIndex));
        }

        // Setup next buttons
        for (int i = 0; i < nextButtons.Length; i++)
        {
            int dayIndex = i; // Capture for closure
            nextButtons[i].onClick.AddListener(() => OnNextDay(dayIndex));
        }
    }

    void ShowDay(int dayIndex)
    {
        // Hide all days first
        for (int i = 0; i < dayContainers.Length; i++)
        {
            dayContainers[i].SetActive(false);
        }

        // Show current day
        if (dayIndex < dayContainers.Length)
        {
            dayContainers[dayIndex].SetActive(true);
            optionsContainers[dayIndex].SetActive(true);
            questionnaireContainers[dayIndex].SetActive(false);
        }
    }

    void OnStudyChoice(int dayIndex)
    {
        Debug.Log($"Study chosen for {dayNames[dayIndex]}");

        // Mark as study day
        studyChoices[dayIndex] = true;

        // Hide options, show questionnaire
        optionsContainers[dayIndex].SetActive(false);
        questionnaireContainers[dayIndex].SetActive(true);
    }

    void OnPlayChoice(int dayIndex)
    {
        Debug.Log($"Play chosen for {dayNames[dayIndex]}");

        // Mark as play day
        studyChoices[dayIndex] = false;

        // Move to next day immediately
        MoveToNextDay();
    }

    void OnNextDay(int dayIndex)
    {
        // This is called after completing questionnaire
        MoveToNextDay();
    }

    void MoveToNextDay()
    {
        currentDay++;

        if (currentDay >= dayNames.Length)
        {
            // Week complete - check results
            CheckWeekResults();
        }
        else
        {
            ShowDay(currentDay);
        }
    }

    void CheckWeekResults()
    {
        int studyCount = 0;

        foreach (var choice in studyChoices.Values)
        {
            if (choice) studyCount++;
        }

        Debug.Log($"Total study days: {studyCount}");

        if (studyCount >= 3)
        {
            // Success - move to different scene
            ShowSuccessScene();
        }
        else
        {
            // Failure - prompt to try again
            ShowRetryPrompt();
        }
    }

    void ShowSuccessScene()
    {
        Debug.Log("Success! Moving to next scene...");

        // Hide all day containers
        for (int i = 0; i < dayContainers.Length; i++)
        {
            dayContainers[i].SetActive(false);
        }

        // Show success scene
        if (successScene)
        {
            successScene.SetActive(true);
        }

        // Or load a new scene
        // SceneManager.LoadScene("NextScene");
    }

    void ShowRetryPrompt()
    {
        Debug.Log("Not enough study days. Try again!");

        // Hide all day containers
        for (int i = 0; i < dayContainers.Length; i++)
        {
            dayContainers[i].SetActive(false);
        }

        // Show retry prompt
        if (retryPrompt)
        {
            retryPrompt.SetActive(true);
        }
    }

    public void RestartWeek()
    {
        // Reset all data
        currentDay = 0;
        studyChoices.Clear();

        // Hide end game UI
        if (successScene) successScene.SetActive(false);
        if (retryPrompt) retryPrompt.SetActive(false);

        // Start over
        ShowDay(0);
    }
}

// Alternative: More modular approach with ScriptableObjects for data
[System.Serializable]
public class DayData
{
    public string dayName;
    public bool hasStudied;
    public QuestionData[] questions;
}

[System.Serializable]
public class QuestionData
{
    public string questionText;
    public string[] options;
    public int correctAnswer;
}

// Better approach: State machine pattern
public enum GameState
{
    ShowingDay,
    ShowingQuestionnaire,
    WeekComplete,
    GameOver
}

public class ImprovedWeeklyStudyManager : MonoBehaviour
{
    [Header("Game Data")]
    public DayData[] weekData = new DayData[4];

    private GameState currentState;
    private int currentDayIndex = 0;
    private int totalStudyDays = 0;

    void Start()
    {
        InitializeWeekData();
        SetState(GameState.ShowingDay);
    }

    void InitializeWeekData()
    {
        string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday" };
        for (int i = 0; i < weekData.Length; i++)
        {
            weekData[i] = new DayData { dayName = dayNames[i] };
        }
    }

    void SetState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.ShowingDay:
                ShowCurrentDay();
                break;
            case GameState.ShowingQuestionnaire:
                ShowQuestionnaire();
                break;
            case GameState.WeekComplete:
                CheckResults();
                break;
        }
    }

    void ShowCurrentDay()
    {
        // Implementation for showing current day
        Debug.Log($"Showing {weekData[currentDayIndex].dayName}");
    }

    void ShowQuestionnaire()
    {
        // Implementation for showing questionnaire
        Debug.Log("Showing questionnaire");
    }

    public void OnStudyChosen()
    {
        weekData[currentDayIndex].hasStudied = true;
        totalStudyDays++;
        SetState(GameState.ShowingQuestionnaire);
    }

    public void OnPlayChosen()
    {
        weekData[currentDayIndex].hasStudied = false;
        MoveToNextDay();
    }

    public void OnQuestionnaireComplete()
    {
        MoveToNextDay();
    }

    void MoveToNextDay()
    {
        currentDayIndex++;

        if (currentDayIndex >= weekData.Length)
        {
            SetState(GameState.WeekComplete);
        }
        else
        {
            SetState(GameState.ShowingDay);
        }
    }

    void CheckResults()
    {
        if (totalStudyDays >= 3)
        {
            Debug.Log("Success! Loading next scene...");
            // Load success scene
        }
        else
        {
            Debug.Log("Try again!");
            // Show retry prompt
        }
    }
}