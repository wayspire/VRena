using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Antigoal : MonoBehaviour
{
	public Goal goal;

	private void OnTriggerEnter(Collider other)
	{
		if("Basketball".Equals(other.tag))
		{
			Debug.Log("Antigoal triggered");
			goal.OnAntiGoal();
		}
	}
}
