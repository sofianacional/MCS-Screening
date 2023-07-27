using UnityEngine;
using UnityEngine.Events;

public class ScoreSystem : MonoBehaviour {
	
	[SerializeField] private int pointsPerGem;
	[SerializeField] private int floatingInitialValue;
	
	//[Space]
	
	private int totalScore;
	private int totalFloatingGemPoints;
	private int totalGemsPopped;

	public UnityEvent<int> Evt_OnUpdateScore = new();
	public UnityEvent<int> Evt_AddPoints = new();

	private void Awake() {
		SingletonManager.Register(this);
	}
	
	public void RemoveSingleton() {
		SingletonManager.Remove<ScoreSystem>();
	}

	public void ComputeSimilarGemsScore(int gems) {
		int value = gems * pointsPerGem;
		AddToTotalScore(value);
		AddTotalGems(gems);
	}

	public void ComputeFloatingGemsScore(int floatingGems) {
		print("floating gem = " + floatingGems);
		int value = floatingInitialValue;
		for (int i = 1; i < floatingGems; i++) {
			value *= 2;
		}
		AddToTotalScore(value);
	}

	private void AddToTotalScore(int value) {
		totalScore += value;
		Evt_OnUpdateScore.Invoke(totalScore);
		Evt_AddPoints.Invoke(value);
	}

	private void AddTotalGems(int value) {
		totalGemsPopped += value;
	}
}
