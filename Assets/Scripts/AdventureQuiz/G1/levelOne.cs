using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

using Vuforia;

public class levelOne : MonoBehaviour
{
    public static levelOne instance;

    public List<levelOneQuestionAndAnswers> QnA;
    public InputField[] answerFields;
    public GameObject[] stars;
    public int currentQuestion;

    public GameObject Quizpanel;
    public GameObject GoPanel;
    public GameObject MenuContainer;
    public GameObject CorrectSound; // Reference to the GameObject with correct sound AudioSource
    public GameObject WrongSound; // Reference to the GameObject with wrong sound AudioSource
    public Button submitButton;

    public Text ScoreTxt;
    [SerializeField] private Text CountdownTxt;

    public int score;

    private float countdownTime = 40f;
    private bool isCountdownActive = false;
    private TimeSpan timeSpan;
    private bool isTimerPaused = false;


    public FirebaseApp firebaseApp;
    public FirebaseFirestore firestore;
    public FirebaseAuth auth;

    private FirebaseManager firebaseManager;

    private DefaultObserverEventHandler observerEventHandler;

    private void Start()
    {
        observerEventHandler = FindObjectOfType<DefaultObserverEventHandler>();

        observerEventHandler.OnTargetLost.AddListener(PauseTimer);
        observerEventHandler.OnTargetFound.AddListener(ResumeTimer);

        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                firebaseApp = FirebaseApp.DefaultInstance;
                InitializeFirestore();
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase with the error: " + dependencyStatus.ToString());
            }
        });

        GoPanel.SetActive(false);
        generateQuestion();
        submitButton.onClick.AddListener(CheckAnswer);
    }

    private void Awake()
    {
        firebaseManager = FirebaseManager.instance;
        DontDestroyOnLoad(gameObject);

        if (firebaseManager == null)
        {
            Debug.LogError("FireBaseManager object not found in the scene.");
        }

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
    }

    private void InitializeFirestore()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    public void retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GameOver()
    {
        // Quizpanel.SetActive(false);
        GoPanel.SetActive(true);
        ScoreTxt.text = score + "/" + QnA.Count;
        starsAcheived();
        AddScoreToLeaderboard();
    }

    public void starsAcheived()
    {
        float percentage = ((float)score / (float)QnA.Count) * 100f;
        if (percentage >= 67f)
        {
            stars[0].SetActive(true);
            stars[1].SetActive(true);
            stars[2].SetActive(true);
        }
        else if (percentage >= 34)
        {
            stars[0].SetActive(true);
            stars[1].SetActive(true);
        }
        else
        {
            stars[0].SetActive(true);
        }
    }

    void AddScoreToLeaderboard()
    {
        string currentUserId = firebaseManager.GetCurrentUserId(); // Set the player name or use a unique identifier

        // Retrieve the current user document from the "user" collection
        firestore.Collection("Users").Document(currentUserId)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    var documentSnapshot = task.Result;
                    if (documentSnapshot.Exists)
                    {
                        // Get the current user name, grade, and section from the user document
                        string playerName = documentSnapshot.GetValue<string>("UserName");
                        int playerGrade = documentSnapshot.GetValue<int>("UserGrade");
                        string sectionId = documentSnapshot.GetValue<string>("UserSection");

                        // Check if the player name already exists in the leaderboard for the specific grade, section, and user ID
                        firestore.Collection("AdventureLeaderboard")
                            .Document(playerGrade.ToString()) // Assuming "playerGrade" is an integer representing the grade
                            .Collection(sectionId.ToString()) // Assuming "sectionId" is an integer representing the section ID
                            .WhereEqualTo("playerName", playerName)
                            .GetSnapshotAsync()
                            .ContinueWithOnMainThread(queryTask =>
                            {
                                if (queryTask.IsCompleted)
                                {
                                    var querySnapshot = queryTask.Result;
                                    if (querySnapshot.Count > 0)
                                    {
                                        // Player name already exists, compare scores and update if necessary
                                        foreach (var docSnapshot in querySnapshot.Documents)
                                        {
                                            var leaderboardEntry = docSnapshot.ToDictionary();
                                            int existingScore = Convert.ToInt32(leaderboardEntry["score"]);

                                            // Update the existing leaderboard entry with the new score
                                            docSnapshot.Reference.UpdateAsync("score", score)
                                                .ContinueWithOnMainThread(updateTask =>
                                                {
                                                    if (updateTask.IsCompleted)
                                                    {
                                                        Debug.Log("Score updated in leaderboard!");
                                                    }
                                                    else if (updateTask.IsFaulted)
                                                    {
                                                        Debug.LogError("Failed to update score in leaderboard with error: " + updateTask.Exception);
                                                    }
                                                });
                                        }
                                    }
                                    else
                                    {
                                        // Player name does not exist, add a new entry to the leaderboard
                                        Dictionary<string, object> leaderboardEntry = new Dictionary<string, object>
                                        {
                                            { "score", score },
                                            { "playerName", playerName },
                                            { "totalQuestions", QnA.Count}
                                        };

                                        // Add the leaderboard entry to the "AdventureLeaderboard" collection
                                        firestore.Collection("AdventureLeaderboard")
                                            .Document(playerGrade.ToString()) // Assuming "playerGrade" is an integer representing the grade
                                            .Collection(sectionId.ToString())// Assuming "sectionId" is an integer representing the section ID
                                            .AddAsync(leaderboardEntry)
                                            .ContinueWithOnMainThread(addTask =>
                                            {
                                                if (addTask.IsCompleted)
                                                {
                                                    Debug.Log("Score added to leaderboard!");
                                                }
                                                else if (addTask.IsFaulted)
                                                {
                                                    Debug.LogError("Failed to add score to leaderboard with error: " + addTask.Exception);
                                                }
                                            });
                                    }
                                }
                                else if (queryTask.IsFaulted)
                                {
                                    Debug.LogError("Failed to query leaderboard with error: " + queryTask.Exception);
                                }
                            });
                    }
                    else
                    {
                        Debug.LogError("Current user document does not exist in the 'user' collection.");
                    }
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("Failed to retrieve current user document with error: " + task.Exception);
                }
            });
    }

    public void CheckAnswer()
    {
        string userAnswer = answerFields[currentQuestion].text;
        string correctAnswer = QnA[currentQuestion].CorrectAnswer.ToString();
        Debug.Log(userAnswer + "==" + correctAnswer);

        if (userAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase))
        {
            correct();
        }
        else
        {
            wrong();
        }
    }

    public void correct()
    {
        score += 1;
        // Set submit button color to green for correct answer
        submitButton.GetComponent<UnityEngine.UI.Image>().color = Color.green;
        PlayCorrectSound(); // Play the correct sound effect
        answerFields[currentQuestion].interactable = false; // Disable the answer field of the current question
        currentQuestion++;
        StartCoroutine(waitForNext());
    }

    public void wrong()
    {
        // Set submit button color to red for wrong answer
        submitButton.GetComponent<UnityEngine.UI.Image>().color = Color.red;
        PlayWrongSound(); // Play the wrong sound effect
        answerFields[currentQuestion].interactable = false; // Disable the answer field of the current question
        currentQuestion++;
        StartCoroutine(waitForNext());
    }


    IEnumerator waitForNext()
    {
        yield return new WaitForSeconds(1);
        countdownTime = 40f;
        generateQuestion();
    }

    void generateQuestion()
    {
        if (currentQuestion < QnA.Count)
        {
            for (int i = 0; i < QnA.Count; i++)
            {
                if (i == currentQuestion)
                {
                    QnA[i].QuestionPanel.SetActive(true);
                    answerFields[i].interactable = true; // Enable the specific answer field
                }
                else
                {
                    answerFields[i].interactable = false; // Disable the specific answer field
                }
            }

            // Reset submit button color to default
            submitButton.GetComponent<UnityEngine.UI.Image>().color = Color.white;

            StartCountdown();
        }
        else
        {
            Debug.Log("Out of Questions");
            GameOver();
        }
    }

    void StartCountdown()
    {
        if (!isCountdownActive)
        {
            isCountdownActive = true;
            StartCoroutine(CountdownCoroutine());
        }
    }

    IEnumerator CountdownCoroutine()
    {
        // Check if CountdownTxt is null or destroyed
        if (CountdownTxt == null)
        {
            Debug.LogError("CountdownTxt is null or has been destroyed.");
            yield break;
        }

        while (countdownTime > 0)
        {
            if (!isTimerPaused)
            {
                timeSpan = TimeSpan.FromSeconds(countdownTime);

                // Check if CountdownTxt is null or destroyed before accessing its text property
                if (CountdownTxt != null)
                {
                    CountdownTxt.text = timeSpan.ToString("mm':'ss");
                }
                else
                {
                    Debug.LogWarning("CountdownTxt is null or has been destroyed.");
                    yield break;
                }

                yield return new WaitForSeconds(1f);
                countdownTime -= 1f;
            }
            else
            {
                yield return null;
            }
        }

        if (isCountdownActive)
        {
            isCountdownActive = false;

            // Check if CountdownTxt is null or destroyed before accessing its text property
            if (CountdownTxt != null)
            {
                CountdownTxt.text = "00:00";
                wrong();
            }
            else
            {
                Debug.LogWarning("CountdownTxt is null or has been destroyed.");
            }
        }
    }

    public void ToggleMenuContainer(bool show)
    {
        MenuContainer.SetActive(show);
        isTimerPaused = show;
    }

    void PlayCorrectSound()
    {
        if (CorrectSound != null)
        {
            AudioSource audioSource = CorrectSound.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }

    void PlayWrongSound()
    {
        if (WrongSound != null)
        {
            AudioSource audioSource = WrongSound.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }

    private void PauseTimer()
    {
        isTimerPaused = true;
    }

    private void ResumeTimer()
    {
        isTimerPaused = false;
    }
}
