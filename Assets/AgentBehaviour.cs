using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AgentBehaviour : MonoBehaviour {

	public AgentInitialiser agent;
	public float rangeOfSight;
	public int frequency = 50;
	public float coneAngle = 45;

	public Vector3 leftVector;
	private Vector3 rightVector;
	private RaycastHit hit;

	// Use this for initialization
	void Start () 
	{
		rightVector = -leftVector;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate(){
		// stop crazy rotation
		var oldRotation = transform.rotation;

		// first, go to left hand side of the angle rather than the middle, so you can begin firing rays in a linear fashion.
		transform.Rotate(leftVector * (coneAngle / 2));

		for (int i = 0; i < frequency; i++) {
			
			// visualise the raycast - Used for Debugging.
			Vector3 fwd = transform.TransformDirection(Vector3.forward);
			Debug.DrawRay (transform.position, fwd * rangeOfSight, Color.green);

			var ray = Physics.Raycast (transform.position, fwd, out hit, rangeOfSight);
			if (ray) {
				Debug.DrawRay(transform.position, fwd * rangeOfSight, Color.red);
				if(hit.transform.GetComponent<AgentBehaviour> () != null){
					// get the agent which is seen
					var otherAgent = hit.transform.GetComponent<AgentBehaviour> ().agent;	

					// Let's see how attracted the initiator is to the spotted agent! Worst result = 1.41421356237, Best result = 0
					float unattractedRating = getEuclDistance (agent, otherAgent);

					float mostUnattractiveRating = 1.41421356237f;
					if(unattractedRating <= (mostUnattractiveRating * 0.4) ){ // agents are attracted enough to form links if they are 60% or more attractive to the agent [<= 40% unnattractive ] (can add additional conditions)

						Debug.Log ("Max Unattractiveness: " + (mostUnattractiveRating * 0.4) + ", Looker = " + agent.appearance.name + ", spotted agent:  " + otherAgent.appearance.name + ", unattracted to rating: " +  unattractedRating);

						// Strength for the link.
						float linkStrength = Random.Range (50, 101);

						// Type of link
						var targetType = "friendship";

						// See if there is already this particular link with the agents:

						//Get all links with otherAgent
						var linksWithAgent = agent.links.Where (x => x.from == otherAgent || x.to == otherAgent);

						//Find links which are of this type:
						var filteredLinks = linksWithAgent.Where(x => x.type.Equals(targetType));

						//So now, if the length of these links (there is more than 0 elements) then a link already exists:
						if (filteredLinks.Count() == 0) 
						{
							//Add the link from this agent to other agent
							Link linkFromAgentToOther = new Link (targetType, agent, otherAgent, linkStrength);
							agent.links.Add (linkFromAgentToOther);

							Link linkFromOtherToAgent = new Link ("partner", otherAgent, agent, linkStrength);
							otherAgent.links.Add (linkFromOtherToAgent);

							/**Debug.Log (agent.appearance.name + "'s Link from: " + linkFromAgentToOther.from.appearance.name + " to: " + linkFromAgentToOther.to.appearance.name 
								+ " is in the category of: " + linkFromAgentToOther.type + " with a link strength of: " + linkFromAgentToOther.strength);
				
							Debug.Log (agent.appearance.name + "'s Link from: " + linkFromOtherToAgent.from.appearance.name + " to: " + linkFromOtherToAgent.to.appearance.name 
							+ " is in the category of: " + linkFromOtherToAgent.type + " with a link strength of: " + linkFromOtherToAgent.strength);*/
						}

					}


				}

			}

			// move to placement for the next ray
			transform.Rotate(rightVector * (coneAngle/frequency));
		}

		// go back to origin rotation
		transform.rotation = oldRotation;

	}


	public float getEuclDistance(AgentInitialiser initiator, AgentInitialiser spotted){
		
		// get agents attractivenessPreference (attractiveness is formed for each agent by comparing the agent's sexualpreferences for each gender with the other agents gender percentages).
		// Euclidean distance of the attractiveness and the gender of the other agents
		// Output the eucl distance.

	 	List<float> sexPreferencesOfInitiator = new List<float>();
		sexPreferencesOfInitiator = initiator.sexPreferences;

		List<float> genderOfSpotted = new List<float>();
		genderOfSpotted = spotted.gender;

		List<float> subtractedThenSquared = new List<float>();

		float attractedRating = 0;

		// First subtract the sexual preferences of each gender from the gender of the spotted agent then square the resultant value.
		for(int i = 0; i < sexPreferencesOfInitiator.Count(); i++){
			subtractedThenSquared.Add( Mathf.Pow ( (sexPreferencesOfInitiator [i] - genderOfSpotted [i]), 2) );
		}

		// Then get the sum of all of the values.
		float resultsOfSum = subtractedThenSquared.Sum ();

		// Finally square root the result to get the euclideanDistance
		attractedRating = Mathf.Sqrt(resultsOfSum);

		// Return the euclidean distance
		return attractedRating;
	}


}

