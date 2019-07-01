using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
	public Antigoal antigoal;
	public int score = 0;
	public bool antiGoalTriggered = false;
	float lastGoalTime;
	public TextMesh scoreText;
	public void OnAntiGoal()
	{
		if (Time.time-lastGoalTime >= 2f || lastGoalTime == 0)
		{
			Debug.Log("Upwards!");
			antiGoalTriggered = true;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (antiGoalTriggered)
		{
			Debug.Log("Doesn't count!");
			StartCoroutine(wait(2));
			antiGoalTriggered = false;
			return;
		}

		if ("Basketball".Equals(other.tag) && !antiGoalTriggered)
		{
			Debug.Log("Goal!");
			lastGoalTime = Time.time;
			score++;
			scoreText.text = "Score: " + score;
		}
	}

	IEnumerator wait(float seconds)
	{
		yield return new WaitForSeconds(2);
	}
}
