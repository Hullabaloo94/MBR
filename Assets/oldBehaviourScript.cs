using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class oldBehaviourScript : MonoBehaviour {

	public AgentInitialiser agent;
	public float rangeOfSight;
	public int frequency = 50;
	public float coneAngle = 45;
	public float loveThreshold = 40;

	public Vector3 leftVector;
	private Vector3 rightVector;
	private RaycastHit hit;
	private string targetType;
	private float mostUnattractiveRating = 1.41421356237f;

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


			/**
			 * Used for debugging if the agents agentsICanSee list only has who can actually see at the time.
			 * 
			 * if (agent.agentsICanSee.Count != 0) {
				Debug.Log (agent.appearance.name + "'s amount of agents they can see:" + agent.agentsICanSee.Count);
				for(int apples = 0; apples < agent.agentsICanSee.Count; apples++){
					Debug.Log (agent.appearance.name + " can see: " + agent.agentsICanSee [apples].appearance.name);
				}
			}*/

			// clear the list everytime it cannot see any agents.
			if(i == 0){
				agent.agentsICanSee.Clear ();
			}

			// visualise the raycast - Used for Debugging.
			Vector3 fwd = transform.TransformDirection (Vector3.forward);
			Debug.DrawRay (transform.position, fwd * rangeOfSight, Color.green);

			var ray = Physics.Raycast (transform.position, fwd, out hit, rangeOfSight);

			if (ray) {
				Debug.DrawRay(transform.position, fwd * rangeOfSight, Color.red);
				if (hit.transform.GetComponent<AgentBehaviour> () != null) {

					// get the agent which is seen
					var otherAgent = hit.transform.GetComponent<AgentBehaviour> ().agent;	

					// add into list the agent that is seen.
					agent.agentsICanSee.Add (otherAgent);

					// Can the seen agent see this agent?
					bool canOtherAgentSeeYou = otherAgent.agentsICanSee.Contains(agent);

					//Get all links with otherAgent (See if already met the agent)
					var linksWithAgent = agent.links.Where (x => x.from == otherAgent || x.to == otherAgent);

					// if they can see the agent then:
					if(canOtherAgentSeeYou == true){

						// if never met them before
						if (!agent.agentAttractionPercentages.ContainsKey(otherAgent.id)) {

							// Let's see how attracted the initiator is to the spotted agent! Worst result = 1.41421356237, Best result = 0
							// Add this to the Dictionary of the agent's attraction percentages to ensure that the attraction percentage isn't regenerated everytime they meet.
							agent.agentAttractionPercentages.Add(otherAgent.id, (100 - (getEuclDistance (agent, otherAgent) / mostUnattractiveRating * 100)) );
						}

						//---LOOOOOVE MACHINE-------

						// If this agent is attracted 
						if (agent.agentAttractionPercentages[otherAgent.id] >= loveThreshold) {

							// if both can see one another and are attracted to one another :)
							if (otherAgent.agentAttractionPercentages.ContainsKey(agent.id) && otherAgent.agentAttractionPercentages[agent.id] >= loveThreshold) {
								targetType = "Love Partner";
							} else {
								// attracted to other agent but they are not attracted to the agent :(
								targetType = "Friend";
							}
						} else {
							// Agent is not attracted to the seen agent
							targetType = "Friend";
						}

						//-----------------------

						// See if there is already this particular link with the agents:
						//Find links which are of this type
						var filteredLinks = linksWithAgent.Where (x => x.type.Equals (targetType));

						//So now, if the length of these links (there is more than 0 elements) then a link already exists:
						if (filteredLinks.Count () == 0) {
							// Strength for the link.
							float linkStrength = Random.Range (50, 101);

							//Add the links to both agents
							agent.links.Add (new Link (targetType, agent, otherAgent, linkStrength) );
							otherAgent.links.Add (new Link (targetType, otherAgent, agent, linkStrength) );
						}

						//Debug.Log (agent.appearance.name + " finds " + otherAgent.appearance.name + " to be " + agent.agentAttractionPercentages[otherAgent.id] + "% attractive. This makes them in the category of " + targetType);

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

