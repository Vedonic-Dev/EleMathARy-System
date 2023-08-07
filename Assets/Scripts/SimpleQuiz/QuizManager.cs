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

public class QuizManager : MonoBehaviour
{
    public static QuizManager instance;

    public List<QuestionAndAnswers> QnA;
    public GameObject[] options;
    public int currentQuestion;

    public GameObject Quizpanel;
    public GameObject GoPanel;
    public GameObject MenuContainer;
    public GameObject CorrectSound; // Reference to the GameObject with correct sound AudioSource
    public GameObject WrongSound; // Reference to the GameObject with wrong sound AudioSource

    public Text QuestionTxt;
    public RawImage QuestionImage;
    public Text ScoreTxt;
    [SerializeField] private Text CountdownTxt;

    int totalQuestions = 10;
    public int score;

    private float countdownTime = 40f;
    private bool isCountdownActive = false;
    private TimeSpan timeSpan;
    private bool isTimerPaused = false;


    public FirebaseApp firebaseApp;
    public FirebaseFirestore firestore;
    public FirebaseAuth auth;

    private FirebaseManager firebaseManager;

    private void Start()
    {
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
        Quizpanel.SetActive(false);
        GoPanel.SetActive(true);
        ScoreTxt.text = score + "/" + 10;
        AddScoreToLeaderboard();
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
                        // Get the current user name from the user document
                        string playerName = documentSnapshot.GetValue<string>("UserName");

                        // Check if the player name already exists in the leaderboard
                        firestore.Collection("SimpleLeaderboard")
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
                                        // Player name does not exist, add new entry to the leaderboard
                                        Dictionary<string, object> leaderboardEntry = new Dictionary<string, object>
                                        {
                                            { "score", score },
                                            { "playerName", playerName }
                                        };

                                        // Add the leaderboard entry to the "leaderboard" collection in Firestore
                                        firestore.Collection("SimpleLeaderboard").AddAsync(leaderboardEntry)
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

    public void correct()
    {
        score += 1;
        QnA.RemoveAt(currentQuestion);
        PlayCorrectSound(); // Play the correct sound effect
        StartCoroutine(waitForNext());
    }

    public void wrong()
    {
        QnA.RemoveAt(currentQuestion);
        PlayWrongSound(); // Play the wrong sound effect
        StartCoroutine(waitForNext());
    }

    IEnumerator waitForNext()
    {
        yield return new WaitForSeconds(1);
        countdownTime = 40f;
        generateQuestion();
    }

    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<Image>().color = options[i].GetComponent<AnswerScript>().startColor;
            options[i].GetComponent<AnswerScript>().isCorrect = false;
            options[i].transform.GetChild(0).GetComponent<Text>().text = QnA[currentQuestion].Answers[i];

            if (QnA[currentQuestion].CorrectAnswer == i + 1)
            {
                options[i].GetComponent<AnswerScript>().isCorrect = true;
            }
        }
    }

    void generateQuestion()
    {
        if (QnA.Count > 0 && totalQuestions > 0)
        {
            currentQuestion = UnityEngine.Random.Range(0, QnA.Count);

            QuestionTxt.text = QnA[currentQuestion].Question;

            if (QnA[currentQuestion].QuestionImage != null)
            {
                QuestionImage.texture = QnA[currentQuestion].QuestionImage;
                QuestionImage.gameObject.SetActive(true);
            }
            else
            {
                QuestionImage.gameObject.SetActive(false);
            }

            SetAnswers();

            totalQuestions--;

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
                StartCoroutine(waitForNext());
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
}