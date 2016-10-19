using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AgentCreator : MonoBehaviour {

	public GameObject agentPrefab;
	public GameObject agent2Prefab;
	public GameObject agent3Prefab;
	public GameObject agent4Prefab;
	private List<AgentInitialiser> population = new List<AgentInitialiser>();
	public int populationAmount;
	public Transform origin; // will be used as the original starting position of the agents before being altered.


	// !!reproduction stuff!!

	// 3 animation curves to use for gender configuration.
	public AnimationCurve linearPossibilityCurve;
	public AnimationCurve biasedTowardsPossibilityCurve;
	public AnimationCurve biasedAgainstPossibilityCurve;

	// used to get a biased towards 2 but can be between 1 and 4 parents/genders/sexes value.
	public AnimationCurve sexAndGenderValueFinderCurve; 

	// The amount of sexes and genders in the soceity.
	public static int numOfSexesAndGenders;

	// Sexes who can give birth.
	public static int sexWhoCanGiveBirth;

	public int numGenerationsSimulatedFromInitGeneration;

	//!!END of reproduction stuff!!


	// Use this for initialization
	void Start ()
	{
		// Generate the number of sexes and genders that will form the society
		numOfSexesAndGenders = (int)Mathf.Round (getPercentageFromAnimCurve (sexAndGenderValueFinderCurve));
		Debug.Log ("The amount of sexes in the society is: " + numOfSexesAndGenders);

		// Establish the child carrier sex of the society
		if (numOfSexesAndGenders > 1) {
			// Not asexual so establish the carrier sex/s...

			// Make a list of all of the indexes of the sexes
			var indexList = Enumerable.Range (0, numOfSexesAndGenders).ToList ();
			//for (int i = 0; i < indexList.Count; i++){
			//	Debug.Log ("Original list element number: " + i + ": " + indexList[i]);
			//}

			// Shuffle them so the sexes taken will be random
			indexList.Shuffle ();
			//for (int i = 0; i < indexList.Count; i++) {
			//	Debug.Log ("Shuffled List element number: " + i + ": " + indexList[i]);
			//}

			// store the index of the sex which can give birth.
			sexWhoCanGiveBirth = indexList.ElementAt (0);
			Debug.Log ("Index of the sex that can give birth : " + sexWhoCanGiveBirth);

		}

		initializePopulation (populationAmount);

		createGenerations ();
	}


	private void createGenerations(){
		
		// Now that an initial population has spawned - simulate N generations from the initial population

		// first for: N number of generations
		for (int i = 0; i < numGenerationsSimulatedFromInitGeneration; i++) {
			
			// Second for: run through all of the agents current in the population (This will be larger each incrementation of the first for).	
			for (int agentInPop = 0; agentInPop < population.Count (); agentInPop++) {

				// Variables which will be reset for each agent in the population

				// store the sexes which aren't this agents sex.
				List<int> allSexesMinusOwn = new List<int> (); 
				allSexesMinusOwn = Enumerable.Range (0, numOfSexesAndGenders).ToList ();
				allSexesMinusOwn.RemoveAt (population [agentInPop].sex);
				// Set tempList to the sexes List above so that the tempList can be altered whilst not changing the length used in the for loop for iteration.
				List<int> tempList = allSexesMinusOwn;

				// get agents behaviour script
				AgentBehaviour behaviour = population [agentInPop].appearance.GetComponent<AgentBehaviour> ();
				// get loveThreshold for this agent.
				float loveThreshold = behaviour.loveThreshold;

				//for 3.1: check sexPref vs Genders on all other initial agents to get attraction levels.
				for (int otherAgent = 0; otherAgent < population.Count (); otherAgent++) {
					// Be sure not to compare the agent itself against itself.
					if(otherAgent != agentInPop){

						// if never met them before - establish how attracted to them the agent is.
						if (!population [agentInPop].agentAttractionPercentages.ContainsKey (population [otherAgent].id)) {
							// Add this to the Dictionary of the agent's attraction percentages to ensure that the attraction percentage isn't regenerated everytime they meet.
							population [agentInPop].agentAttractionPercentages.Add (population [otherAgent].id, (100 - (behaviour.getEuclDistance (population [agentInPop], population [otherAgent]) / AgentBehaviour.mostUnattractiveRating * 100)));
						}

						string targetType = behaviour.establishLinkType (population [agentInPop], population [otherAgent]);

						//Get all links with otherAgent (See if already met the agent)
						var linksWithAgent = population [agentInPop].links.Where (x => x.to == population [otherAgent]);
						// See if there is already this particular link with the agents:
						var filteredLinks = linksWithAgent.Where (x => x.type.Equals (targetType));
						//So now if length is 0 then make a link because a link is yet to be made.
						if (filteredLinks.Count () == 0) {
							population [agentInPop].links.Add (new Link (targetType, population [agentInPop], population [otherAgent], Random.Range (50, 101)));
						}
					}
				}

				// --- All links made with other agents so second loop for the population is no longer needed to be open. ---
				// Check all breeders if they are capable of producing a child with their lovepartners         
				if (population [agentInPop].canGiveBirth && population [agentInPop].age > 15) { 
					// Find all of the current Love partner links for this agent who are not the same sex as the agent and are over 15.
					var agentsLovePartners = population [agentInPop].links.Where (x => x.type.Equals ("Love Partner") && x.to.sex != population [agentInPop].sex && x.to.age > 15);  

					// See if there is a love partnership with every OTHER sex available which is old enough:
					if (agentsLovePartners.Count () != 0) {
						// stores all potential parents in here.
						List<AgentInitialiser> parents = new List<AgentInitialiser>();

						// for 3.2: for all of the sexes which aren't the agent's sex.
						for (int sexes = 0; sexes < allSexesMinusOwn.Count (); sexes++) {
							// for 3.2.1: for all of the links which are love partners of a different sex
							for (int partner = 0; partner < agentsLovePartners.Count (); partner++) {
								// if the love partner is a sex that is in the list of sexes
								if (allSexesMinusOwn [sexes] == agentsLovePartners.ElementAt (partner).to.sex) {
									// Add the potential parent to the list to be used for links later for both the parent and the child
									parents.Add (agentsLovePartners.ElementAt (partner).to); 
									// remove the love partner's sex from the list
									var foundIndex = tempList.IndexOf (allSexesMinusOwn [sexes]);
									tempList.RemoveAt (foundIndex);
									// if at any point all of the sexes are catered for.
									if (tempList.Count == 0) {
										// Add the mother to the list of parents
										parents.Add (population[agentInPop]);

										// -----produce a child------
										// CHOICE 1: if you don't want them all in the same position, just use this position variable in haveChild's first param:
										var position = origin.position;
										// alter the position slightly so all agents don't spawn on each other.
										position.x += Random.Range (-20, 20);
										position.y = 1;
										position.z += Random.Range (-20, 20); 

										haveChild (position, true, Random.Range (50, 101), parents);
										// CHOICE 2: if you want the children and childrens children etc to all spawn on the original mothers location:
										/*haveChild (population [agentInPop].appearance.transform.position, true, Random.Range (50, 101), parents);*/
										//----produce a child----------

										// reset lists
										parents.Clear ();
										tempList = allSexesMinusOwn;
									}
									// this break will take you back to for 3.2 (move on to the next sex to find if there is one.)
									break;
								}
							}
						}
					}
				}

			}

			// At end of all generations except the last generated: add 16 years on to the age as if that time has passed, 16 specifically so that that generations children can have children.
			if(i < numGenerationsSimulatedFromInitGeneration-1){
				for (int agentInPop = 0; agentInPop < population.Count (); agentInPop++) {population[agentInPop].age += 16;}
			}

		}

	} 


	private void initializePopulation(int amountToSpawn)
	{
		for (var i = 0; i < amountToSpawn; i++) 
		{
			// store the original starting position
			var position = origin.position;
			// alter the position slightly so all agents don't spawn on each other.
			position.x += Random.Range (-10, 10);
			position.y = 1;
			position.z += Random.Range (-10, 10);

			// Create an instance of the agent with some predefined characteristics.
			AgentInitialiser agent = new AgentInitialiser (position, genIdx (numOfSexesAndGenders), Random.value*100, Random.Range (0, 101), this);

			// Generate the age
			agent.age = Random.Range (20.0f, 50.0f);

			//add them to the population.
			population.Add(agent);

			// Debugging tool: Show what has been generated in the attributes for the agent
			//showAttributes(agent);
		}

	}

	public void haveChild(Vector3 position, bool DOA, float linkStrength, List<AgentInitialiser> parents){

		// Choosing which of the parents sexes the child will inherit the sex from.
		int babySex = parents[genIdx(parents.Count)].sex;

		// Create the baby
		AgentInitialiser baby = new AgentInitialiser(position, babySex, Random.value*100, Random.Range (0, 101), this);

		// Link all of the agents which contributed to the birth of the child to the child.
		// new link -> type, arrow coming from, arrow going to, link strength.
		for(int i = 0; i < parents.Count(); i++){
			baby.links.Add(new Link ("Child", baby, parents[i], linkStrength) );
			parents[i].links.Add(new Link ("Parent", parents[i], baby, linkStrength) );

			//Debug.Log (baby.links.Last ().from.appearance.name + "is a " + baby.links.Last ().type + " of " + baby.links.Last ().to.appearance.name);
			//Debug.Log (parents[i].links.Last ().from.appearance.name + "is a " + parents[i].links.Last ().type + " of " + parents[i].links.Last ().to.appearance.name);
		}

		// Add to the overall population of the environment
		population.Add(baby);
	}

	private void showAttributes(AgentInitialiser agent){ // Just outputs to console the particular agents attribute information.
		Debug.Log ("Agent Id: " + agent.id);
		Debug.Log ("Sex: " + agent.sex);
		Debug.Log ("Age: " + agent.age);
		Debug.Log ("Libido: " + agent.libido);
		Debug.Log("Loyalty: " + agent.loyalty);
		Debug.Log("Can agent give birth: " + agent.canGiveBirth);
	}

	public int genIdx(int numOfElements){
		return Random.Range(0, numOfElements);
		// On returining an index you can use that index to access the choice which is generated.
		
	}

	// Generates a percentage based on an animation curve
	public float getPercentageFromAnimCurve(AnimationCurve anAnimationCurve){
		// get the x value as a random value from 0 to 1.
		var x = Random.value;
		// get the y axis value (The percentage) based on x.
		return anAnimationCurve.Evaluate (x);
	}

	public GameObject instantiatePrefab(Vector3 position, int sex)
	{
		//Return value is going to be a gameobject
		var returnValue = default(GameObject);

		switch(sex)
		{
			//Depending on the sex, instantiate a difference prefab
			case 0:
				returnValue = UnityEngine.Object.Instantiate (agentPrefab, position, Quaternion.identity) as GameObject;
				break;

			case 1:
				returnValue = UnityEngine.Object.Instantiate (agent2Prefab, position, Quaternion.identity) as GameObject;
				break;

			case 2:
				returnValue = UnityEngine.Object.Instantiate (agent3Prefab, position, Quaternion.identity) as GameObject;
				break;

			case 3:
				returnValue = UnityEngine.Object.Instantiate (agent4Prefab, position, Quaternion.identity) as GameObject;
				break;
		}

		//Return the appearance
		return returnValue;
	}


}

public static class ShuffleExtension{
	private static System.Random rng = new System.Random();  

	public static void Shuffle<T>(this IList<T> list)  
	{  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}
}

