using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {

    private GameManager gameManager;
    private ScoreSystem scoreSystem;
    
    [SerializeField] private TextMeshProUGUI scoreText;

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
        scoreSystem.Evt_AddPoints.AddListener(SetScoreText);
    }

    private void SetScoreText(int value) {
        scoreText.text = value.ToString();
    }

    private void DisplayWinPanel(int totalScore) {
        gameOverPanel.SetActive(true);
        gameOverText.text = winText;
        totalScoreText.text = "Total Score: " + totalScore.ToString();
    }
    
    private void DisplayGameOverPanel(int totalScore) {
        gameOverPanel.SetActive(true);
        gameOverText.text = losingText;
        totalScoreText.text = "Total Score: " + totalScore.ToString();
    }
}
