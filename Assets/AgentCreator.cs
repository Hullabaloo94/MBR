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
	public AnimationCurve sexualityPercentageCurve;
	public AnimationCurve genderMasculinityPercentage;


	// !!reproduction stuff!!

	// 3 animation curves to use for gender configuration.
	public AnimationCurve linearPossibilityCurve;
	public AnimationCurve biasedTowardsPossibilityCurve;
	public AnimationCurve biasedAgainstPossibilityCurve;

	public AnimationCurve sexAndGenderValueFinderCurve; // used to get a biased towards 2 but can be between 1 and 4 parents/genders/sexes value.
	// number of parents is also the number of sexes AND genders that are in the society.
	private int numOfSexesAndGenders; // The amount of sexes and genders in the soceity.
	// Sexes who can give birth.
	private int sexWhoCanGiveBirth;
	// The number required to breed -> will be the same as th enumber of sexs
	private int numNeedToBreed;
	//!!END of reproduction stuff!!


	// Use this for initialization
	void Start () 
	{
		// Generate the number of sexes and genders that will form the society
		numOfSexesAndGenders = (int) Mathf.Round(getPercentageFromAnimCurve(sexAndGenderValueFinderCurve));
		Debug.Log ("The amount of sexes in the society is: " + numOfSexesAndGenders);

		// The amount to breed is set to the number of sexs because why have sexes which aren't doing anything.
		numNeedToBreed = numOfSexesAndGenders;
		//Debug.Log ("numToBreed: " + numNeedToBreed);

		// Establish the child carrier sex/s of the society
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
			//Debug.Log ("Index of the sex that can give birth : " + sexWhoCanGiveBirth);

		}

		initializePopulation (populationAmount);

	}

	private void initializePopulation(int amountToSpawn)
	{
		for (var i = 0; i < amountToSpawn; i++) 
		{
			AgentInitialiser agent = new AgentInitialiser ();

			// store the original starting position
			var position = origin.position;
			// alter the position slightly so all agents don't spawn on each other.
			position.x += Random.Range (-10, 10);
			position.y = 1;
			position.z += Random.Range (-10, 10);

			//generate the agent some attributes
			agent.id = AgentInitialiser.getNextId();
			agent.sex = genIdx (numOfSexesAndGenders);

			//instantiation stuff
			switch(agent.sex){
			case 0:
				agent.appearance = Instantiate (agentPrefab, position, Quaternion.identity) as GameObject;
				break;
			case 1:
				agent.appearance = Instantiate (agent2Prefab, position, Quaternion.identity) as GameObject;
				break;
			case 2:
				agent.appearance = Instantiate (agent3Prefab, position, Quaternion.identity) as GameObject;
				break;
			case 3:
				agent.appearance = Instantiate (agent4Prefab, position, Quaternion.identity) as GameObject;
				break;
			}

			agent.appearance.transform.name = "Agent - " + agent.id;

			// This will be used for collision of the agents themselves.
			agent.appearance.GetComponent<AgentBehaviour> ().agent = agent;
			//..

			agent.age = Random.Range (20, 50);
			agent.alive = true;
			agent.libido = Random.Range (0, 101);

			//..

			// numOfSexes biased animation curves then randomly generate the values to which will then be used as such: w% = w/(w+x+y+z)
			for(int j = 0; j < numOfSexesAndGenders; j++){
				// Ensure that a biased animation curve is used when evaluating the genderNorms of each sex.
				if (agent.sex == j) {
					agent.gender.Add(getPercentageFromAnimCurve (biasedTowardsPossibilityCurve));
					//Debug.Log ("Agent's gender value at element " + j + ": " + agent.gender.ElementAt(j));
				} else {
					// Randomly assign which animation curve for each aspect of the gender of the agent that isnt the genderNorm of the sex.
					int whichAnimCurve = Random.Range (0, 3);
					switch(whichAnimCurve){
					case 0:
						agent.gender.Add(getPercentageFromAnimCurve (biasedTowardsPossibilityCurve));
						//Debug.Log ("Agent's gender value at element " + j + ": " + agent.gender.ElementAt(j));
						break;
					case 1:
						agent.gender.Add(getPercentageFromAnimCurve (linearPossibilityCurve));
						//Debug.Log ("Agent's gender value at element " + j + ": "+ agent.gender.ElementAt(j));
						break;
					case 2:
						agent.gender.Add(getPercentageFromAnimCurve (biasedAgainstPossibilityCurve));
						//Debug.Log ("Agent's gender value at element " + j + ": " + agent.gender.ElementAt(j));
						break;
					}
				}
			}

			// Now we have values for each aspect of the agents gender, we now need to make them percentages.
			float genderAspectsTotal = default(float);
			// To do this, we first sum all of the aspects of the agent's gender
			genderAspectsTotal = agent.gender.Sum(); 
			// We then make each of the elements into percentages
			for(int aspectOfGender = 0; aspectOfGender < agent.gender.Count; aspectOfGender++){
				agent.gender[aspectOfGender] = agent.gender.ElementAt (aspectOfGender) / genderAspectsTotal; 
				Debug.Log ("Agent's gender percentage for element " + aspectOfGender + ": " + agent.gender.ElementAt(aspectOfGender));
			}
			// All percentages added together should make 1
			Debug.Log ("Total percentage: " + agent.gender.Sum());



			// Next: Do Something similar to above, for this too...
			agent.oppositeSexPreference = getPercentageFromAnimCurve (sexualityPercentageCurve);




			//add them to the population.
			population.Add(agent);

			// Debugging tool: Show what has been generated in the attributes for the agent
			//showAttributes(agent);
		}

	}



	private void showAttributes(AgentInitialiser agent){ // Just outputs to console the particular agents attribute information.
		Debug.Log ("Agent Id: " + agent.id);
		//Debug.Log ("Alive: " + agent.alive);
		Debug.Log ("Sex: " + agent.sex);
		//Debug.Log ("genderMasculinityPercentage: " + agent.genderMasculinityPercentage);
		//Debug.Log ("Age: " + agent.age);
		//Debug.Log ("Libido: " + agent.libido);
		//Debug.Log ("Opposite Sex Preference: " + agent.oppositeSexPreference);

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

	// Update is called once per frame
	void Update () {
		
	}


		
}
	
//Used to set up an agent.
public class AgentInitialiser {

	private static int lastId = 0;
	public int id;
	public GameObject appearance;
	public int libido;
	public int age;
	public List<float> gender = new List<float>(); // Each sex will have numOfGender elements.
	public int sex;
	public bool alive;
	//public string[] personality;
	public float oppositeSexPreference;
	private AgentInitialiser mother;
	private AgentInitialiser father;
	private AgentInitialiser[] children;

	// Moves the id number along so no agent has the same ID.
	public static int getNextId(){
		return lastId++;
	}

	// Coordinates the colour of the agent to be blue if male and red for female
	/**public void setColour(SexChoices sexToColour){
		Color targetColour = default(Color);

		switch(sexToColour){
		case SexChoices.Male:
			targetColour = Color.blue;
			break;


		case SexChoices.Female:
			targetColour = Color.red;
			break;
		
		}

		// Apply to appropriate colour.
		appearance.GetComponent<Renderer> ().material.color = targetColour;

	}*/
		
}

public static class ShuffleExtension
{
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
