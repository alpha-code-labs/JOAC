

// QuizManager.cs - Main game controller
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;


// QuizQuestion.cs - Data structure for questions
[System.Serializable]
public class QuizQuestion
{
    public string question;
    public string[] options = new string[4];
    public int correctAnswerIndex;
    public string subject;
}

public class QuizManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button[] optionButtons = new Button[4];
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI questionCounterText;
    public TextMeshProUGUI scoreText;
    public Image timerFillImage;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    public Button continueButton;

    [Header("Game Settings")]
    public float questionTime = 5f;

    private List<QuizQuestion> allQuestions;
    private List<QuizQuestion> gameQuestions;
    private int currentQuestionIndex = 0;
    private int score = 0;
    private float currentTimer;
    private bool isAnswering = false;
    private bool gameEnded = false;

    public GameObject winPanel;
    public GameObject losePanel;

    void Start()
    {
        InitializeQuestions();
        SetupGame();
        //restartButton.onClick.AddListener(RestartGame);
        continueButton.onClick.AddListener(ContinueGame);
    }

    void InitializeQuestions()
    {
        allQuestions = new List<QuizQuestion>
        {
            // Science Questions
            new QuizQuestion { question = "What is the chemical symbol for gold?", options = new string[] {"Au", "Ag", "Fe", "Cu"}, correctAnswerIndex = 0, subject = "Science" },
            new QuizQuestion { question = "How many bones are in an adult human body?", options = new string[] {"196", "206", "216", "186"}, correctAnswerIndex = 1, subject = "Science" },
            new QuizQuestion { question = "What planet is known as the Red Planet?", options = new string[] {"Venus", "Jupiter", "Mars", "Saturn"}, correctAnswerIndex = 2, subject = "Science" },
            new QuizQuestion { question = "What gas makes up most of Earth's atmosphere?", options = new string[] {"Oxygen", "Carbon Dioxide", "Hydrogen", "Nitrogen"}, correctAnswerIndex = 3, subject = "Science" },
            
            // History Questions
            new QuizQuestion { question = "In which year did World War II end?", options = new string[] {"1944", "1945", "1946", "1947"}, correctAnswerIndex = 1, subject = "History" },
            new QuizQuestion { question = "Who was the first person to walk on the moon?", options = new string[] {"Buzz Aldrin", "Neil Armstrong", "John Glenn", "Alan Shepard"}, correctAnswerIndex = 1, subject = "History" },
            new QuizQuestion { question = "Which ancient wonder was located in Alexandria?", options = new string[] {"Colossus of Rhodes", "Hanging Gardens", "Lighthouse", "Statue of Zeus"}, correctAnswerIndex = 2, subject = "History" },
            new QuizQuestion { question = "The Berlin Wall fell in which year?", options = new string[] {"1987", "1988", "1989", "1990"}, correctAnswerIndex = 2, subject = "History" },
            
            // Geography Questions
            new QuizQuestion { question = "What is the capital of Australia?", options = new string[] {"Sydney", "Melbourne", "Canberra", "Perth"}, correctAnswerIndex = 2, subject = "Geography" },
            new QuizQuestion { question = "Which is the longest river in the world?", options = new string[] {"Amazon", "Nile", "Mississippi", "Yangtze"}, correctAnswerIndex = 1, subject = "Geography" },
            new QuizQuestion { question = "Mount Everest is located in which mountain range?", options = new string[] {"Andes", "Alps", "Rockies", "Himalayas"}, correctAnswerIndex = 3, subject = "Geography" },
            new QuizQuestion { question = "Which country has the most time zones?", options = new string[] {"Russia", "USA", "China", "France"}, correctAnswerIndex = 3, subject = "Geography" },
            
            // Sports Questions
            new QuizQuestion { question = "How many players are on a basketball team on court?", options = new string[] {"4", "5", "6", "7"}, correctAnswerIndex = 1, subject = "Sports" },
            new QuizQuestion { question = "In tennis, what score comes after 30?", options = new string[] {"35", "40", "45", "50"}, correctAnswerIndex = 1, subject = "Sports" },
            new QuizQuestion { question = "Which sport uses a shuttlecock?", options = new string[] {"Tennis", "Squash", "Badminton", "Table Tennis"}, correctAnswerIndex = 2, subject = "Sports" },
            new QuizQuestion { question = "How often are the Summer Olympics held?", options = new string[] {"Every 2 years", "Every 3 years", "Every 4 years", "Every 5 years"}, correctAnswerIndex = 2, subject = "Sports" },
            
            // Literature Questions
            new QuizQuestion { question = "Who wrote 'Romeo and Juliet'?", options = new string[] {"Charles Dickens", "William Shakespeare", "Jane Austen", "Mark Twain"}, correctAnswerIndex = 1, subject = "Literature" },
            new QuizQuestion { question = "Which book begins with 'Call me Ishmael'?", options = new string[] {"The Great Gatsby", "Moby Dick", "To Kill a Mockingbird", "1984"}, correctAnswerIndex = 1, subject = "Literature" },
            new QuizQuestion { question = "Who wrote '1984'?", options = new string[] {"Aldous Huxley", "Ray Bradbury", "George Orwell", "H.G. Wells"}, correctAnswerIndex = 2, subject = "Literature" },
            new QuizQuestion { question = "What is the first book in the Harry Potter series?", options = new string[] {"Chamber of Secrets", "Philosopher's Stone", "Prisoner of Azkaban", "Goblet of Fire"}, correctAnswerIndex = 1, subject = "Literature" },
            
            // Mathematics Questions
            new QuizQuestion { question = "What is the value of π (pi) to 2 decimal places?", options = new string[] {"3.14", "3.15", "3.16", "3.17"}, correctAnswerIndex = 0, subject = "Mathematics" },
            new QuizQuestion { question = "What is 15% of 200?", options = new string[] {"25", "30", "35", "40"}, correctAnswerIndex = 1, subject = "Mathematics" },
            new QuizQuestion { question = "What is the square root of 144?", options = new string[] {"11", "12", "13", "14"}, correctAnswerIndex = 1, subject = "Mathematics" },
            new QuizQuestion { question = "If x + 5 = 12, what is x?", options = new string[] {"6", "7", "8", "9"}, correctAnswerIndex = 1, subject = "Mathematics" },
            
            // Technology Questions
            new QuizQuestion { question = "What does 'WWW' stand for?", options = new string[] {"World Wide Web", "World Web Wide", "Wide World Web", "Web World Wide"}, correctAnswerIndex = 0, subject = "Technology" },
            new QuizQuestion { question = "Which company created the iPhone?", options = new string[] {"Google", "Microsoft", "Apple", "Samsung"}, correctAnswerIndex = 2, subject = "Technology" },
            new QuizQuestion { question = "What does 'HTML' stand for?", options = new string[] {"Hyper Text Markup Language", "High Tech Modern Language", "Home Tool Markup Language", "Hyper Transfer Markup Language"}, correctAnswerIndex = 0, subject = "Technology" },
            new QuizQuestion { question = "In computing, what does 'RAM' stand for?", options = new string[] {"Random Access Memory", "Rapid Access Memory", "Read Access Memory", "Real Access Memory"}, correctAnswerIndex = 0, subject = "Technology" },
            
            // Movies Questions
            new QuizQuestion { question = "Which movie won the Academy Award for Best Picture in 2020?", options = new string[] {"1917", "Joker", "Parasite", "Once Upon a Time in Hollywood"}, correctAnswerIndex = 2, subject = "Movies" },
            new QuizQuestion { question = "Who directed the movie 'Jaws'?", options = new string[] {"George Lucas", "Steven Spielberg", "Martin Scorsese", "Francis Ford Coppola"}, correctAnswerIndex = 1, subject = "Movies" },
            new QuizQuestion { question = "In which movie does the character Jack say 'I'm the king of the world!'?", options = new string[] {"Avatar", "Titanic", "The Revenant", "Catch Me If You Can"}, correctAnswerIndex = 1, subject = "Movies" },
            new QuizQuestion { question = "Which animated movie features the song 'Let It Go'?", options = new string[] {"Moana", "Tangled", "Frozen", "The Little Mermaid"}, correctAnswerIndex = 2, subject = "Movies" },
            
            // Music Questions
            new QuizQuestion { question = "How many strings does a standard guitar have?", options = new string[] {"4", "5", "6", "7"}, correctAnswerIndex = 2, subject = "Music" },
            new QuizQuestion { question = "Which instrument has 88 keys?", options = new string[] {"Organ", "Piano", "Keyboard", "Harpsichord"}, correctAnswerIndex = 1, subject = "Music" },
            new QuizQuestion { question = "What does 'forte' mean in music?", options = new string[] {"Soft", "Loud", "Fast", "Slow"}, correctAnswerIndex = 1, subject = "Music" },
            new QuizQuestion { question = "Which band released the album 'Abbey Road'?", options = new string[] {"The Rolling Stones", "Led Zeppelin", "The Beatles", "Pink Floyd"}, correctAnswerIndex = 2, subject = "Music" },
            
            // Food Questions
            new QuizQuestion { question = "Which spice is derived from the Crocus flower?", options = new string[] {"Turmeric", "Saffron", "Paprika", "Cinnamon"}, correctAnswerIndex = 1, subject = "Food" },
            new QuizQuestion { question = "What is the main ingredient in guacamole?", options = new string[] {"Tomato", "Onion", "Avocado", "Pepper"}, correctAnswerIndex = 2, subject = "Food" },
            new QuizQuestion { question = "Which country is famous for inventing pizza?", options = new string[] {"France", "Spain", "Greece", "Italy"}, correctAnswerIndex = 3, subject = "Food" },
            new QuizQuestion { question = "What type of pastry is used to make profiteroles?", options = new string[] {"Puff pastry", "Shortcrust pastry", "Choux pastry", "Filo pastry"}, correctAnswerIndex = 2, subject = "Food" }
        };
    }

    void SetupGame()
    {
        // Randomly select 36 questions
        gameQuestions = allQuestions.OrderBy(x => Random.Range(0f, 1f)).Take(36).ToList();

        currentQuestionIndex = 0;
        score = 0;
        gameEnded = false;
        gameOverPanel.SetActive(false);

        DisplayQuestion();
        SetupOptionButtons();
    }

    void SetupOptionButtons()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int buttonIndex = i; // Capture for closure
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => SelectAnswer(buttonIndex));
        }
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex >= gameQuestions.Count)
        {
            EndGame();
            return;
        }

        var currentQuestion = gameQuestions[currentQuestionIndex];
        questionText.text = currentQuestion.question;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.options[i];
            optionButtons[i].interactable = true;
            optionButtons[i].image.color = Color.white;
        }

        questionCounterText.text = $"{currentQuestionIndex + 1}/36";
        scoreText.text = $"{score}";

        StartTimer();
    }

    void StartTimer()
    {
        currentTimer = questionTime;
        isAnswering = true;
    }

    void Update()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
        if (isAnswering && !gameEnded)
        {
            currentTimer -= Time.deltaTime;

            // Update timer display
            timerText.text = Mathf.Ceil(currentTimer).ToString();
            timerFillImage.fillAmount = currentTimer / questionTime;

            // Change color based on time remaining
            if (currentTimer <= 2f)
                timerFillImage.color = Color.red;
            else if (currentTimer <= 3f)
                timerFillImage.color = Color.yellow;
            else
                timerFillImage.color = Color.green;

            if (currentTimer <= 0)
            {
                TimeUp();
            }
        }
    }

    void SelectAnswer(int selectedIndex)
    {
        if (!isAnswering || gameEnded) return;

        isAnswering = false;
        var currentQuestion = gameQuestions[currentQuestionIndex];

        // Disable all buttons
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = false;
        }

        // Show correct answer
        optionButtons[currentQuestion.correctAnswerIndex].image.color = Color.green;

        if (selectedIndex == currentQuestion.correctAnswerIndex)
        {
            // Correct answer
            score += 10; // 10 points per correct answer
        }
        else
        {
            // Wrong answer - highlight the wrong choice
            optionButtons[selectedIndex].image.color = Color.red;
        }

        // Wait a moment then move to next question
        Invoke("NextQuestion", 1.5f);
    }

    void TimeUp()
    {
        if (!isAnswering || gameEnded) return;

        isAnswering = false;
        var currentQuestion = gameQuestions[currentQuestionIndex];

        // Disable all buttons and show correct answer
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = false;
        }

        optionButtons[currentQuestion.correctAnswerIndex].image.color = Color.green;

        Invoke("NextQuestion", 1.5f);
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        DisplayQuestion();
    }

    void EndGame()
    {
        gameEnded = true;
        gameOverPanel.SetActive(true);
        if (score >= 270)
        {
            losePanel.SetActive(false);
            winPanel.SetActive(true);
            SaveManager.SaveChapterCompleted(true);
        }
        else
        {
            winPanel.SetActive(false);
            losePanel.SetActive(true);
        }
        finalScoreText.text = $"{score}";
    }

    string GetScoreRating()
    {
        float percentage = (float)score / 360f * 100f;

        if (percentage >= 90f) return "Excellent!";
        else if (percentage >= 80f) return "Great Job!";
        else if (percentage >= 70f) return "Good Work!";
        else if (percentage >= 60f) return "Not Bad!";
        else if (percentage >= 50f) return "Keep Practicing!";
        else return "Better Luck Next Time!";
    }

    public void RestartGame()
    {
        SetupGame();
    }

    public void ContinueGame()
    {

    }
}

// Instructions for Unity Setup:
/*
1. Create a new Unity 2D project
2. Create an empty GameObject and name it "QuizManager"
3. Attach the QuizManager script to this GameObject

4. Create the UI Canvas with the following hierarchy:
   Canvas
   ├── QuestionPanel
   │   ├── QuestionText (TextMeshPro)
   │   ├── OptionsPanel
   │   │   ├── Option1Button (Button with TextMeshPro child)
   │   │   ├── Option2Button (Button with TextMeshPro child)
   │   │   ├── Option3Button (Button with TextMeshPro child)
   │   │   └── Option4Button (Button with TextMeshPro child)
   │   ├── TimerPanel
   │   │   ├── TimerBackground (Image)
   │   │   ├── TimerFill (Image with Image Type: Filled)
   │   │   └── TimerText (TextMeshPro)
   │   ├── QuestionCounter (TextMeshPro)
   │   └── ScoreText (TextMeshPro)
   └── GameOverPanel
       ├── FinalScoreText (TextMeshPro)
       └── RestartButton (Button)

5. Assign all UI references in the QuizManager script inspector
6. Set the GameOverPanel to inactive by default
7. Configure the TimerFill image:
   - Set Image Type to "Filled"
   - Set Fill Method to "Horizontal"
   - Set Fill Amount to 1

8. Style the UI as desired with colors, fonts, and layouts
9. Build and run!

Features included:
- 36 random questions from multiple subjects
- 5-second timer per question with visual feedback
- Color-coded timer (green/yellow/red)
- Score tracking (10 points per correct answer)
- Visual feedback for correct/wrong answers
- Game over screen with performance rating
- Restart functionality
- Question counter display
*/