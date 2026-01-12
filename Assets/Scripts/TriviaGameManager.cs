using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TriviaGameManager : MonoBehaviour
{

    public PlayableDirector director;
    public GameObject GameLoopPanel;

    [Header("Monday")]
    public GameObject MondayContainer;
    public GameObject MondayOptionsContainer;
    public Button MondayStudyButton;
    public Button MondayPlayButton;
    public GameObject MondayQuestionnaireContainer;
    public Button MondayNextButton;
    [Header("Tuesday")]
    public GameObject TuesdayContainer;
    public GameObject TuesdayOptionsContainer;
    public Button TuesdayStudyButton;
    public Button TuesdayPlayButton;
    public GameObject TuesdayQuestionnaireContainer;
    public Button TuesdayNextButton;
    [Header("Wednesday")]
    public GameObject WednesdayContainer;
    public GameObject WednesdayOptionsContainer;
    public Button WednesdayStudyButton;
    public Button WednesdayPlayButton;
    public GameObject WednesdayQuestionnaireContainer;
    public Button WednesdayNextButton;
    [Header("Thursday")]
    public GameObject ThursdayContainer;
    public GameObject ThursdayOptionsContainer;
    public Button ThursdayStudyButton;
    public Button ThursdayPlayButton;
    public GameObject ThursdayQuestionnaireContainer;
    public Button ThursdayNextButton;
    [Header("Success Scene")]
    public GameObject SuccessScene;
    [Header("Retry Scene")]
    public GameObject RetryScene;
    public float optionsContainerDelay = 3f;

    int studyDaysCount = 0;

    void Start()
    {
        //set all days to inactive
        MondayContainer.SetActive(true);
        TuesdayContainer.SetActive(false);
        WednesdayContainer.SetActive(false);
        ThursdayContainer.SetActive(false);

        //set all questionnaires to inactive
        MondayQuestionnaireContainer.SetActive(false);
        TuesdayQuestionnaireContainer.SetActive(false);
        WednesdayQuestionnaireContainer.SetActive(false);
        ThursdayQuestionnaireContainer.SetActive(false);

        TuesdayOptionsContainer.SetActive(false);
        WednesdayOptionsContainer.SetActive(false);
        ThursdayOptionsContainer.SetActive(false);


        MondayStudyButton.onClick.AddListener(OnMondayStudyButtonClicked);
        MondayPlayButton.onClick.AddListener(OnMondayPlayButtonClicked);
        TuesdayStudyButton.onClick.AddListener(OnTuesdayStudyButtonClicked);
        TuesdayPlayButton.onClick.AddListener(OnTuesdayPlayButtonClicked);
        WednesdayStudyButton.onClick.AddListener(OnWednesdayStudyButtonClicked);
        WednesdayPlayButton.onClick.AddListener(OnWednesdayPlayButtonClicked);
        ThursdayStudyButton.onClick.AddListener(OnThursdayStudyButtonClicked);
        ThursdayPlayButton.onClick.AddListener(OnThursdayPlayButtonClicked);
        MondayNextButton.onClick.AddListener(OnMondayNextButtonClicked);
        TuesdayNextButton.onClick.AddListener(OnTuesdayNextButtonClicked);
        WednesdayNextButton.onClick.AddListener(OnWednesdayNextButtonClicked);
        ThursdayNextButton.onClick.AddListener(OnThursdayNextButtonClicked);
    }

    void Update()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
    }
    void OnMondayStudyButtonClicked()
    {
        studyDaysCount++;
        MondayQuestionnaireContainer.SetActive(true);
        MondayOptionsContainer.SetActive(false);
    }

    void OnMondayPlayButtonClicked()
    {
        MondayContainer.SetActive(false);
        TuesdayContainer.SetActive(true);
        StartCoroutine(SetTuesdayOptionsToActive());
    }

    void OnTuesdayStudyButtonClicked()
    {
        studyDaysCount++;
        TuesdayQuestionnaireContainer.SetActive(true);
        TuesdayOptionsContainer.SetActive(false);
    }

    void OnTuesdayPlayButtonClicked()
    {
        TuesdayContainer.SetActive(false);
        WednesdayContainer.SetActive(true);
        StartCoroutine(SetWednesdayOptionsToActive());
    }

    void OnWednesdayStudyButtonClicked()
    {
        studyDaysCount++;
        WednesdayQuestionnaireContainer.SetActive(true);
        WednesdayOptionsContainer.SetActive(false);
    }

    void OnWednesdayPlayButtonClicked()
    {
        WednesdayContainer.SetActive(false);
        ThursdayContainer.SetActive(true);
        StartCoroutine(SetThursdayOptionsToActive());
    }

    void OnThursdayStudyButtonClicked()
    {
        studyDaysCount++;
        ThursdayQuestionnaireContainer.SetActive(true);
        ThursdayOptionsContainer.SetActive(false);
    }

    void OnThursdayPlayButtonClicked()
    {
        ThursdayContainer.SetActive(false);
        if (studyDaysCount >= 3)
        {
            SuccessScene.SetActive(true);
            GameLoopPanel.SetActive(false);
        }
        else
        {
            RetryScene.SetActive(true);
            GameLoopPanel.SetActive(false);
        }
    }

    void OnMondayNextButtonClicked()
    {
        MondayContainer.SetActive(false);
        TuesdayContainer.SetActive(true);
        StartCoroutine(SetTuesdayOptionsToActive());
    }

    void OnTuesdayNextButtonClicked()
    {
        TuesdayContainer.SetActive(false);
        WednesdayContainer.SetActive(true);
        StartCoroutine(SetWednesdayOptionsToActive());
    }

    void OnWednesdayNextButtonClicked()
    {
        WednesdayContainer.SetActive(false);
        ThursdayContainer.SetActive(true);
        StartCoroutine(SetThursdayOptionsToActive());
    }

    void OnThursdayNextButtonClicked()
    {
        ThursdayContainer.SetActive(false);
        if (studyDaysCount >= 3)
        {
            SuccessScene.SetActive(true);
            GameLoopPanel.SetActive(false);
        }
        else
        {
            RetryScene.SetActive(true);
            GameLoopPanel.SetActive(false);
        }
    }


    IEnumerator SetMondayOptionsToActive()
    {
        yield return new WaitForSeconds(optionsContainerDelay);
        MondayOptionsContainer.SetActive(true);
    }

    IEnumerator SetTuesdayOptionsToActive()
    {
        yield return new WaitForSeconds(optionsContainerDelay);
        TuesdayOptionsContainer.SetActive(true);
    }

    IEnumerator SetWednesdayOptionsToActive()
    {
        yield return new WaitForSeconds(optionsContainerDelay);
        WednesdayOptionsContainer.SetActive(true);
    }

    IEnumerator SetThursdayOptionsToActive()
    {
        yield return new WaitForSeconds(optionsContainerDelay);
        ThursdayOptionsContainer.SetActive(true);
    }

}
