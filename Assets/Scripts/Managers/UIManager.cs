using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {

    private GameManager gameManager;
    private ScoreSystem scoreSystem;
    
    [SerializeField] private Image[] scoreBoxes;
    [SerializeField] private Sprite[] digitsSprites;

    [Space]
    [SerializeField] private string winText;
    [SerializeField] private string losingText;
    
    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject tutorialPanel;
    
    private void Start() {
        gameManager = SingletonManager.Get<GameManager>();
        scoreSystem = SingletonManager.Get<ScoreSystem>();
        
        gameManager.Evt_GameOver.AddListener(DisplayGameOverPanel);
        gameManager.Evt_PlayerWin.AddListener(DisplayWinPanel);
        scoreSystem.Evt_OnUpdateScore.AddListener(SetScoreText);
    }

    #region Score

    private void SetScoreText(int value) {
        List<int> digits = GetScoreDigits(value);
        
        if (digits.Count > 5) { // set UI to 99999
            foreach (var boxes in scoreBoxes) {
                boxes.sprite = digitsSprites[10];
            }
            return;
        }
        int temp = scoreBoxes.Length - digits.Count;
        if (temp > 0) {
            for (int i = 0; i < temp; i++) { // blank boxes
                scoreBoxes[i].sprite = digitsSprites[digitsSprites.Length - 1];
            }
        }

        int count = 0;
        for (int i = temp; i < scoreBoxes.Length; i++) {
            scoreBoxes[i].sprite = digitsSprites[digits[count]];
            count++;
        }
    }

    private List<int> GetScoreDigits(int score) {
        List<int> digits = new List<int>();
        int number = score;
        int digitsCount = Mathf.FloorToInt(Mathf.Log10(score) + 1);
        
        for (int i = 0; i < digitsCount; i++) {
            int currentDigit = number % 10;
            number /= 10;
            digits.Insert(0, currentDigit);
        }

        return digits;
    }

    private void DisplayWinPanel(int totalScore) {
        gameOverPanel.SetActive(true);
        gameOverText.text = winText;
        totalScoreText.text = "Total Score: " + totalScore;
    }
    
    private void DisplayGameOverPanel(int totalScore) {
        gameOverPanel.SetActive(true);
        gameOverText.text = losingText;
        totalScoreText.text = "Total Score: " + totalScore;
    }

    #endregion

    #region How-To-Play

    public void ShowTutorialPanel() {
        tutorialPanel.SetActive(true);
        PauseGame();
    }

    public void HideTutorialPanel() {
        tutorialPanel.SetActive(false);
        ResumeGame();
    }

    #endregion

    #region Pause

    public void ShowPausePanel() {
        pausePanel.SetActive(true);
        PauseGame();
    }

    public void HidePausePanel() {
        pausePanel.SetActive(false);
        ResumeGame();
    }
    
    private void PauseGame() {
        gameManager.PauseGame();
    }
    
    private void ResumeGame() {
        gameManager.ResumeGame();
    }

    public void ExitGame() {
        gameManager.ExitGame();
    }

    #endregion
    
}
