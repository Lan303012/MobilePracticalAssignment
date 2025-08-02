using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CountingManager : MonoBehaviour
{
    public GameObject objectPrefab;
    public RectTransform objectGrid;
    public Button[] optionButtons;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timerText;

    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip tickSound;
    public AudioClip startSound;
    public AudioClip warningSound;
    public AudioSource audioSource;

    private int correctAnswer;

    public GameObject titlePanel;
    public CanvasGroup titleCanvasGroup;
    public TextMeshProUGUI countdownText;
    private int currentLevel = 1;
    private int totalLevels = 10;
    private float remainingTime;
    private int displayTimeSnapshot;
    private float initialTime;
    private int wrongAttempts = 0;
    private bool gameActive = false;
    public GameObject gameOverPanel;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI statsText;
    private bool canAnswer = true;
    private bool warningPlayed = false;
    void Start()
    {
        // reset game
        currentLevel = 1;
        UpdateLevelUI();
        wrongAttempts = 0;
        canAnswer = true;
        gameActive = false;

        audioSource = GetComponent<AudioSource>();
        countdownText.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);

        // Time Setting
        initialTime = ModeManager.SelectedDifficulty == "Easy" ? 60f : 120f;
        remainingTime = initialTime;
        UpdateTimerUI();

        StartCoroutine(ShowTitleThenCountdown());
    }


    void Update()
    {
        if (gameActive)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerUI();

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                EndGame(false);
            }

            if (remainingTime <= 30f && !warningPlayed)
            {
                timerText.color = Color.red;
                audioSource.PlayOneShot(warningSound);
                warningPlayed = true;

            }
        }
    }


    void WinGame()
    {
        EndGame(true);
    }

    void EndGame(bool win)
    {
        gameActive = false;
        canAnswer = false;
        gameOverPanel.SetActive(true);

        int uiRemainingTime = displayTimeSnapshot;
        float timeUsed = initialTime - uiRemainingTime;

        if (win)
        {
            statusText.text = "You Win!";
            statusText.color = Color.green;
        }
        else
        {
            statusText.text = "Time's Up!";
            statusText.color = Color.red;
        }

        statsText.text = "Time Used: " + Mathf.CeilToInt(timeUsed) + "s" +
                        "\n\nWrong Attempts: " + wrongAttempts;
    }

    void UpdateTimerUI()
    {
        int displayTime;

        if (gameActive)
        {
            displayTime = Mathf.CeilToInt(remainingTime);
            displayTimeSnapshot = displayTime;
        }
        else
        {
            displayTime = displayTimeSnapshot;
        }

        timerText.text = displayTime + "s";

        if (displayTime > 30)
        {
            timerText.color = Color.green;
        }
        else
        {
            timerText.color = Color.red;
        }
    }


    void UpdateLevelUI()
    {
        if (currentLevel > totalLevels)
            levelText.text = totalLevels + "/" + totalLevels;
        else
            levelText.text = currentLevel + "/" + totalLevels;
    }

    IEnumerator ShowTitleThenCountdown()
    {
        titlePanel.SetActive(true);
        titleCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(2f);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            titleCanvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }

        titlePanel.SetActive(false);

        countdownText.gameObject.SetActive(true);

        yield return StartCoroutine(CountdownSequence());

        countdownText.gameObject.SetActive(false);
        gameActive = true;
        GenerateQuestion();
    }
    IEnumerator CountdownSequence()
    {
        string[] countdown = { "3", "2", "1", "Start!" };

        foreach (string text in countdown)
        {
            countdownText.text = text;

            if (text == "Start!")
            {
                countdownText.color = Color.yellow;
                audioSource.PlayOneShot(startSound);
                yield return StartCoroutine(ScaleText(countdownText, 1.8f, 0.5f));
                yield return StartCoroutine(FlashText(countdownText));
            }
            else
            {
                countdownText.color = Color.white;
                audioSource.PlayOneShot(tickSound);
                yield return StartCoroutine(ScaleText(countdownText, 1.5f, 0.5f));
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
    void GenerateQuestion()
    {
        ClearGrid();

        // set difficulty：Easy 10，Hard 20
        int maxNumber = ModeManager.SelectedDifficulty == "Easy" ? 10 : 20;
        correctAnswer = Random.Range(1, maxNumber + 1);


        List<Vector2> usedPositions = new List<Vector2>();
        float minDistance = 150f;

        for (int i = 0; i < correctAnswer; i++)
        {
            GameObject obj = Instantiate(objectPrefab, objectGrid);

            Vector2 newPos;
            int attempt = 0;
            do
            {
                float x = Random.Range(-objectGrid.rect.width / 2f, objectGrid.rect.width / 2f);
                float y = Random.Range(-objectGrid.rect.height / 2f, objectGrid.rect.height / 2f);
                newPos = new Vector2(x, y);
                attempt++;
            }
            while (!IsPositionValid(newPos, usedPositions, minDistance) && attempt < 30);

            usedPositions.Add(newPos);

            RectTransform objRect = obj.GetComponent<RectTransform>();
            objRect.anchoredPosition = newPos;


            float angle = Random.Range(-15f, 15f);
            objRect.localRotation = Quaternion.Euler(0, 0, angle);
        }

        GenerateOptions(maxNumber);
        feedbackText.text = "";

        canAnswer = true;
    }

    void GenerateOptions(int maxNumber)
    {
        List<int> options = new List<int> { correctAnswer };
        while (options.Count < 3)
        {
            int wrong = Random.Range(1, maxNumber + 1);
            if (!options.Contains(wrong))
                options.Add(wrong);
        }

        options.Sort((a, b) => Random.Range(-1, 2)); // Shuffle

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int val = options[i];
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = val.ToString();
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => CheckAnswer(val));
        }
    }

    void CheckAnswer(int selected)
    {
        if (!gameActive || !canAnswer) return;

        canAnswer = false;

        if (selected == correctAnswer)
        {
            feedbackText.text = "Correct!";
            feedbackText.color = Color.green;
            audioSource.PlayOneShot(correctSound);

            if (currentLevel == totalLevels)
            {
                UpdateLevelUI();
                Invoke("WinGame", 1.5f);
            }
            else
            {
                currentLevel++;
                UpdateLevelUI();
                Invoke("GenerateQuestion", 1.5f);
            }
        }
        else
        {
            feedbackText.text = "Try Again!";
            feedbackText.color = Color.red;
            audioSource.PlayOneShot(wrongSound);
            wrongAttempts++;
            StartCoroutine(ShakeObjectGrid());
            StartCoroutine(EnableAnswerAfterDelay(0.5f));
        }
    }


    IEnumerator EnableAnswerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canAnswer = true;
    }

    IEnumerator ShakeObjectGrid()
    {
        Vector3 originalPos = objectGrid.localPosition;
        float duration = 0.5f;
        float elapsed = 0f;
        float magnitude = 10f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            objectGrid.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        objectGrid.localPosition = originalPos;
    }

    void ClearGrid()
    {
        foreach (Transform child in objectGrid)
        {
            Destroy(child.gameObject);
        }
    }

    bool IsPositionValid(Vector2 pos, List<Vector2> usedPositions, float minDist)
    {
        foreach (Vector2 used in usedPositions)
        {
            if (Vector2.Distance(pos, used) < minDist)
                return false;
        }
        return true;
    }

    IEnumerator ScaleText(TextMeshProUGUI textObj, float maxScale, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * maxScale;

        while (elapsed < duration / 2f)
        {
            float t = elapsed / (duration / 2f);
            textObj.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textObj.transform.localScale = targetScale;
        elapsed = 0f;

        while (elapsed < duration / 2f)
        {
            float t = elapsed / (duration / 2f);
            textObj.transform.localScale = Vector3.Lerp(targetScale, startScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textObj.transform.localScale = startScale;
    }

    IEnumerator FlashText(TextMeshProUGUI textObj)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Color startColor = textObj.color;
        Color flashColor = Color.white;

        while (elapsed < duration)
        {
            float t = Mathf.PingPong(elapsed * 3f, 1f);
            textObj.color = Color.Lerp(startColor, flashColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textObj.color = startColor;
    }
}
