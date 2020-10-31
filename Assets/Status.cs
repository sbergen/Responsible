using UnityEngine;
using UnityEngine.UI;

public class Status : MonoBehaviour
{
	public const int StartingLives = 3;

	[SerializeField] private Text scoreText = null;
	[SerializeField] private Text livesText = null;

	private int lives;
	private int score;

	public int Lives
	{
		get => this.lives;
		private set
		{
			this.lives = value;
			this.livesText.text = this.lives.ToString();
		}
	}

	public int Score
	{
		get => this.score;
		private set
		{
			this.score = value;
			this.scoreText.text = this.score.ToString();
		}
	}

	public bool IsAlive => this.lives > 0;

	private void Awake()
	{
		this.Restart();
	}

	public void HitOrMiss(bool wasHit)
	{
		if (wasHit)
		{
			++this.Score;
		}
		else
		{
			--this.Lives;
		}
	}

	public void Restart()
	{
		this.Lives = StartingLives;
		this.Score = 0;
	}
}
