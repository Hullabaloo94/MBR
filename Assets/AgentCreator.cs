using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AgentCreator : MonoBehaviour {

	public GameObject agentPrefab;
	private List<AgentInitialiser> population = new List<AgentInitialiser>();
	public int populationAmount = 2;
	public Transform origin; // will be used as the original starting position of the agents before being altered.
	public AnimationCurve sexualityPercentageCurve;
	public AnimationCurve genderMasculinityPercentage;

	// Use this for initialization
	void Start () 
	{
		initializePopulation (agentPrefab, populationAmount);
	}

	private void initializePopulation(GameObject agentPrefab, int amountToSpawn)
	{
		for (var i = 0; i < amountToSpawn; i++) 
		{
			AgentInitialiser agent = new AgentInitialiser ();


			// store the original starting position
			var position = origin.position;
			// alter the position slightly so all agents don't spawn on each other.
			position.x += Random.Range (-5, 5);
			position.y = 1;
			position.z += Random.Range (-5, 5);

			//generate the agent some attributes
			agent.id = AgentInitialiser.getNextId();

			//instantiation stuff
			agent.appearance = Instantiate(agentPrefab, position, Quaternion.identity) as GameObject;
			agent.appearance.transform.name = "Agent - " + agent.id;
			agent.appearance.GetComponent<AgentBehaviour> ().agent = agent;
			//..


			agent.age = Random.Range (20, 50);
			agent.alive = true;
			agent.sex = (genIdx(2) == 0) ? AgentInitialiser.SexChoices.Male : AgentInitialiser.SexChoices.Female;
			agent.oppositeSexPreference = getPercentageFromAnimCurve (sexualityPercentageCurve);
			agent.setColour(agent.sex); 
			agent.genderMasculinityPercentage = getPercentageFromAnimCurve(genderMasculinityPercentage);
			agent.libido = Random.Range (0, 101);

			//add them to the population.
			population.Add(agent);

			// Debugging tool: Show what has been generated in the attributes for the agent
			showAttributes(agent);
		}

	}



	private void showAttributes(AgentInitialiser agent){ // Just outputs to console the particular agents attribute information.
		Debug.Log ("Agent Id: " + agent.id);
		Debug.Log ("Alive: " + agent.alive);
		Debug.Log ("Sex: " + agent.sex);
		Debug.Log ("genderMasculinityPercentage: " + agent.genderMasculinityPercentage);
		Debug.Log ("Age: " + agent.age);
		Debug.Log ("Libido: " + agent.libido);
		Debug.Log ("Opposite Sex Preference: " + agent.oppositeSexPreference);
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
	public float genderMasculinityPercentage;


	public enum SexChoices
	{
		Male, Female
	}
	public SexChoices sex;

	public bool alive;
	public string[] personality;
	public float oppositeSexPreference;
	private AgentInitialiser mother;
	private AgentInitialiser father;
	private AgentInitialiser[] children;

	// Moves the id number along so no agent has the same ID.
	public static int getNextId(){
		return lastId++;
	}

	// Coordinates the colour of the agent to be blue if male and red for female
	public void setColour(SexChoices sexToColour){
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

	}
		
}
