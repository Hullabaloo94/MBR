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

	//!!END of reproduction stuff!!


	// Use this for initialization
	void Start () 
	{
		// Generate the number of sexes and genders that will form the society
		numOfSexesAndGenders = (int) Mathf.Round(getPercentageFromAnimCurve(sexAndGenderValueFinderCurve));
		Debug.Log ("The amount of sexes in the society is: " + numOfSexesAndGenders);

		// Establish the child carrier sex of the society
		if(numOfSexesAndGenders > 1){
			// Not asexual so establish the carrier sex/s...

			// Make a list of all of the indexes of the sexes
			var indexList = Enumerable.Range (0, numOfSexesAndGenders).ToList();
			//for (int i = 0; i < indexList.Count; i++){
			//	Debug.Log ("Original list element number: " + i + ": " + indexList[i]);
			//}

			// Shuffle them so the sexes taken will be random
			indexList.Shuffle ();
			//for (int i = 0; i < indexList.Count; i++) {
			//	Debug.Log ("Shuffled List element number: " + i + ": " + indexList[i]);
			//}

			// store the index of the sex which can give birth.
			sexWhoCanGiveBirth = indexList.ElementAt(0);
			Debug.Log ("Index of the sex that can give birth : " + sexWhoCanGiveBirth);

		}

		initializePopulation (populationAmount);

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

