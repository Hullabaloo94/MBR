using UnityEngine;
using System.Collections;

public class AgentBehaviour : MonoBehaviour {

	public AgentInitialiser agent;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other) {
		
		var otherAgent = other.transform.GetComponent<AgentBehaviour> ().agent;

		//var generatedValue = Random.Range (0, 1);

		// if value generated is less than or equal to the opposite sex preference percentage then go for opposite gender.
		if (Random.Range (0, 1) <= agent.oppositeSexPreference && agent.sex != otherAgent.sex) {
			Debug.Log ("I am interested in " + otherAgent.appearance.name);
		} else {
			Debug.Log ("I am NOT interested in " + otherAgent.appearance.name);
		}
		//Debug.Log("Generated Value = " + generatedValue);
	}
}
