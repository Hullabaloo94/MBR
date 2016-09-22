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
	private int numOfSexesAndGenders;

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
				// Ensure that a biased animation curve is used when evaluating the gender Norms / sexualPreference Norms of each sex.
				if (agent.sex == j) {
					agent.gender.Add(getPercentageFromAnimCurve (biasedTowardsPossibilityCurve));
					//Debug.Log ("Agent's gender value at element " + j + ": " + agent.gender.ElementAt(j));
					agent.sexPreferences.Add(getPercentageFromAnimCurve (biasedAgainstPossibilityCurve)); // more likely to be most interested in other genders - not same gender.
				} else {
					// Randomly assign which animation curve for each aspect of the gender of the agent that isnt the genderNorm of the sex.
					int whichAnimCurve = Random.Range (0, 3);
					switch(whichAnimCurve){
					case 0:
						agent.gender.Add(getPercentageFromAnimCurve (biasedTowardsPossibilityCurve));
						//Debug.Log ("Agent's gender value at element " + j + ": " + agent.gender.ElementAt(j));
						agent.sexPreferences.Add(getPercentageFromAnimCurve (biasedTowardsPossibilityCurve));
						break;
					case 1:
						agent.gender.Add(getPercentageFromAnimCurve (linearPossibilityCurve));
						//Debug.Log ("Agent's gender value at element " + j + ": "+ agent.gender.ElementAt(j));
						agent.sexPreferences.Add(getPercentageFromAnimCurve (linearPossibilityCurve));
						break;
					case 2:
						agent.gender.Add(getPercentageFromAnimCurve (biasedAgainstPossibilityCurve));
						//Debug.Log ("Agent's gender value at element " + j + ": " + agent.gender.ElementAt(j));
						agent.sexPreferences.Add(getPercentageFromAnimCurve (biasedAgainstPossibilityCurve));
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
				//Debug.Log (agent.appearance.name + "'s gender percentage for element " + aspectOfGender + ": " + agent.gender.ElementAt(aspectOfGender));
			}
			// All percentages added together should make 1
			//Debug.Log (agent.appearance.name + "'s total gender percentage: " + agent.gender.Sum());

			// We do the same for the sexual preferences!
			//Debug.Log (agent.appearance.name + "'s sex: " + agent.sex);
			float sexualPreferenceAspectsTotal = default(float);
			sexualPreferenceAspectsTotal = agent.sexPreferences.Sum(); 

			for(int aspectOfSexualPreference = 0; aspectOfSexualPreference < agent.gender.Count; aspectOfSexualPreference++){
				agent.sexPreferences[aspectOfSexualPreference] = agent.sexPreferences.ElementAt (aspectOfSexualPreference) / sexualPreferenceAspectsTotal; 
				//Debug.Log (agent.appearance.name + "'s sexual preference percentage for element " + aspectOfSexualPreference + ": " + agent.sexPreferences.ElementAt(aspectOfSexualPreference));
			}
			// All percentages added together should make 1
			//Debug.Log (agent.appearance.name + "'s total sexual preference percentage: " + agent.sexPreferences.Sum());

			// Set the colour of the agent based on their percentages of each gender aspect.
			ColourCreator.getColour(agent);

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
	public List<float> sexPreferences = new List<float>();

	// Moves the id number along so no agent has the same ID.
	public static int getNextId(){
		return lastId++;
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

public static class ColourCreator{

	public static void getColour(AgentInitialiser agent){
		Color[] colours = {
			new Color (1.0F, 0.0F, 0.0F, 1.0F),
			new Color (0.0F, 1.0F, 0.0F, 1.0F),
			new Color (0.0F, 0.0F, 1.0F, 1.0F),
			new Color (1.0F, 1.0F, 0.0F, 1.0F)
		};

		Color agentColour = new Color (0.0F, 0.0F, 0.0F, 1.0F);

		int numOfGenders = agent.gender.Count;

		for(int i = 0; i < numOfGenders; i++){
			
			// apply percentages to each colour up to the number of genders.
			colours [i].r *= agent.gender.ElementAt (i); 
			// normalise based on the number of genders there are.
			colours[i].r /= numOfGenders;

			// Do this for R, G and B of the colours used.
			colours [i].g *= agent.gender.ElementAt (i); 
			colours[i].g /= numOfGenders;

			colours[i].b *= agent.gender.ElementAt (i); 
			colours[i].b /= numOfGenders;
		}

		// Now, sum the colours used up and set the agent's r,g and b values to be the sums.
		for (int i = 0; i < numOfGenders; i++) {
			agentColour.r += colours [i].r;
			agentColour.g += colours [i].g;
			agentColour.b += colours [i].b;
		}

		// Apply the colour to the agent.
		agent.appearance.GetComponent<Renderer> ().material.color = agentColour;	
	}
}
