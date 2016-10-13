using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AgentBehaviour : MonoBehaviour {

	public AgentInitialiser agent;
	public float rangeOfSight;
	public int frequency = 50;
	public float coneAngle = 45;
	public float loveThreshold = 40;
	public int turningSpeed = 8;
	public int movementSpeed = 2;
	public Vector3 leftVector;

	// wander variables -----
	// number of seconds between each wander direction recalculation
	public float directionChangeInterval = 1;
	// largest alteration in angle from last recalculation
	public float maxHeadingChange = 30;
	// agents heading
	private float heading;
	private Vector3 targetRotation;	


	private Vector3 rightVector;
	private RaycastHit hit;
	private float mostUnattractiveRating = 1.41421356237f;

	// get the sexes 
	private List<int> allSexesMinusOwn = new List<int>();
	private List<int> tempList = new List<int>();

	void Awake ()
	{
		// initialise wander functionality.
		StartCoroutine(NewHeading());
	}

	/// <summary>
	/// Repeatedly calculates a new direction to move towards.
	/// Use this instead of MonoBehaviour.InvokeRepeating so that the interval can be changed at runtime.
	/// </summary>
	IEnumerator NewHeading ()
	{
		while (true) {
			NewHeadingRoutine();
			yield return new WaitForSeconds(directionChangeInterval);
		}
	}

	/// <summary>
	/// Calculates a new direction to move towards.
	/// </summary>
	void NewHeadingRoutine ()
	{
		var floor = Mathf.Clamp(heading - maxHeadingChange, 0, 360);
		var ceil  = Mathf.Clamp(heading + maxHeadingChange, 0, 360);
		heading = Random.Range(floor, ceil);
		targetRotation = new Vector3(0, heading, 0);
	}

	void wander(){

		transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, targetRotation, Time.deltaTime * directionChangeInterval);
		transform.position += transform.forward * Time.deltaTime * movementSpeed;
	}

	// Use this for initialization
	void Start () 
	{
		rightVector = -leftVector;

		// get the sexes
		if(agent.canGiveBirth == true){
			allSexesMinusOwn = Enumerable.Range(0, AgentCreator.numOfSexesAndGenders).ToList();
			// which aren't the agent's sex
			allSexesMinusOwn.RemoveAt (agent.sex);
			// Create a temp copy of the allSexesMinusOwn so that it can be changed.
			tempList = allSexesMinusOwn;

			Debug.Log (agent.appearance.name + "'s sex is : " + agent.sex + " and can give birth, it's tempList length is: " + tempList.Count);
			foreach(var elem in tempList){
				Debug.Log(elem);
			}
		}
		
	}


	void FixedUpdate(){

		// fire rays to see if you can see anyone.
		fireRays (transform.rotation);

		// Seen someone so goes to talk to them.
		if (agent.agentsICanSee.Count != 0) {

			// if never met them before - establish how attracted to them the agent is.
			if (!agent.agentAttractionPercentages.ContainsKey (agent.agentsICanSee [0].id)) {

				// Let's see how attracted the initiator is to the spotted agent! Worst result = 1.41421356237, Best result = 0
				// Add this to the Dictionary of the agent's attraction percentages to ensure that the attraction percentage isn't regenerated everytime they meet.
				agent.agentAttractionPercentages.Add (agent.agentsICanSee [0].id, (100 - (getEuclDistance (agent, agent.agentsICanSee [0]) / mostUnattractiveRating * 100)));
			}

			// use how attractive the agent finds the other agent, and vice versa to establish the link type. 
			string targetType = establishLinkType (agent.agentsICanSee [0]);
			//Debug.Log (agent.appearance.name + " finds " + agent.agentsICanSee [0].appearance.name + " to be " + agent.agentAttractionPercentages [agent.agentsICanSee [0].id] + "% attractive. This makes them in the category of " + targetType);

			initialiseConversation (agent.agentsICanSee [0], targetType);

		} else {
			wander ();
		}
	}

	public void fireRays(Quaternion originalRotation){
		// stop crazy rotation
		var oldRotation = originalRotation;

		// first, go to left hand side of the angle rather than the middle, so you can begin firing rays in a linear fashion.
		transform.Rotate(leftVector * (coneAngle / 2));

		for (int i = 0; i < frequency; i++) {

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

				// seen an agent!
				if (hit.transform.GetComponent<AgentBehaviour> () != null) {

					// get the agent which is seen
					var otherAgent = hit.transform.GetComponent<AgentBehaviour> ().agent;	

					// add into list the agent that is seen (but check if already in there, to make sure it isn't added for each raycasted that hits the seen agent).
					if(!agent.agentsICanSee.Contains(otherAgent)){
						agent.agentsICanSee.Add (otherAgent);
					}
				}
			}
			// move to placement for the next ray
			transform.Rotate(rightVector * (coneAngle/frequency));
		}
		// go back to origin rotation
		transform.rotation = oldRotation;
	}

	public void initialiseConversation(AgentInitialiser agentToChatWith, string targetType){

		// make the other agent turn to face this agent
		Vector3 noticedAgentDirection = agent.appearance.transform.position - agentToChatWith.appearance.transform.position;
		var directionOfAgent = Quaternion.LookRotation (noticedAgentDirection);
		agentToChatWith.appearance.transform.rotation = Quaternion.Slerp (agentToChatWith.appearance.transform.rotation, directionOfAgent, Time.deltaTime * turningSpeed);

		//Get all links with otherAgent (See if already met the agent)
		var linksWithAgent = agent.links.Where (x => x.to == agentToChatWith);

		// See if there is already this particular link with the agents:
		//Find links which are of this type
		var filteredLinks = linksWithAgent.Where (x => x.type.Equals (targetType));

		//So now, if the length of these links (there is more than 0 elements) then a link already exists:
		if (filteredLinks.Count () == 0) {

			//face the other agent
			Vector3 targetDir = agentToChatWith.appearance.transform.position - agent.appearance.transform.position;
			var rotation = Quaternion.LookRotation (targetDir);
			agent.appearance.transform.rotation = Quaternion.Slerp (agent.appearance.transform.rotation, rotation, Time.deltaTime * turningSpeed);

			// walk towards the other agent 
			if (Vector3.Magnitude (targetDir) > 2) {
				float step = movementSpeed * Time.deltaTime;
				transform.position = Vector3.MoveTowards (transform.position, agentToChatWith.appearance.transform.position, step);
					
			} else { // when close enough for seen agent to notice:

				// form the link.
				createLink (agentToChatWith, targetType);
			}
		} else {
			wander ();
		}
	}

	public string establishLinkType(AgentInitialiser agentToLinkWith){
		string targetType;

		// If this agent is attracted 
		if (agent.agentAttractionPercentages[agentToLinkWith.id] >= loveThreshold) {

			// if both can see one another and are attracted to one another :)
			if (agentToLinkWith.agentAttractionPercentages.ContainsKey(agent.id) && agentToLinkWith.agentAttractionPercentages[agent.id] >= loveThreshold) {
				targetType = "Love Partner";
			} else {
				// attracted to other agent but they are not attracted to the agent :(
				targetType = "Friend";
			}
		} else {
			// Agent is not attracted to the seen agent
			targetType = "Friend";
		}

		return targetType;
	}

	public void createLink(AgentInitialiser agentToLinkWith, string targetType){

		// Strength for the link.
		float linkStrength = Random.Range (50, 101);

		//Add the links to both agents
		agent.links.Add (new Link (targetType, agent, agentToLinkWith, linkStrength) );

		if (agent.canGiveBirth == true) {
			Debug.Log (agent.links.Last ().from.appearance.name + "'s relationship status with " + agent.links.Last ().to.appearance.name + " is: " + agent.links.Last ().type);
		}

		// if one of the agents who is forming a love partnership can carry a child and is not already pregnant.
		if(agent.links.Last().type == "Love Partner" && agent.canGiveBirth == true && agent.pregnant == false){

			// Find all of the current Love partner links for this agent who are not the same sex as the agent.
			var agentsLovePartners = agent.links.Where(x => x.type.Equals("Love Partner") && x.to.sex != agent.sex);  
			// See if there is a love partnership with every OTHER sex available
			if(agentsLovePartners.Count() != 0){

				// check through all of the love links and remove the sex number from the array if one exists
				for(int i = 0; i < allSexesMinusOwn.Count(); i++){

					for(int j = 0; j < agentsLovePartners.Count(); j++){

						// Sometimes this fires sometimes it doesn't???
						if(allSexesMinusOwn[i] == agentsLovePartners.ElementAt(j).to.sex){

							var foundIndex = tempList.IndexOf(allSexesMinusOwn[i]);
							tempList.RemoveAt (foundIndex);
							Debug.Log("templength: " + tempList.Count() + "Elements:");
							foreach(var elem in tempList){
								Debug.Log(elem);
							
							}
							break;
						}
					}
				}

				if(tempList.Count == 0){
				// if the array becomes empty then become pregnant with a child.
					agent.pregnant = true;
					Debug.Log ("Pregnancy status of " + agent.appearance.name + ": " + agent.pregnant);
				}
			}
		}
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

