using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Link{
	public AgentInitialiser to;
	public AgentInitialiser from;

	public string type;
	public float strength;

	public Link(string type, AgentInitialiser from, AgentInitialiser to, float strength)
	{
		this.type = type;
		this.to = to;
		this.from = from;
		this.strength = strength;
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


//Used to set up an agent.
public class AgentInitialiser {
	public AgentCreator creator;

	public List<Link> links = new List<Link>();
	private static int lastId = 0;
	public int id;
	public GameObject appearance;
	public float age;
	public List<float> gender = new List<float>(); // Each sex will have numOfGender elements.
	public int sex;
	public List<float> sexPreferences = new List<float>();
	public Dictionary<int, float> agentAttractionPercentages = new Dictionary<int, float> (); 
	public List<AgentInitialiser> agentsICanSee = new List<AgentInitialiser>();
	public bool canGiveBirth = false;
	public bool pregnant = false;

	public float loyalty;
	public int libido;

	// Moves the id number along so no agent has the same ID.
	public static int getNextId(){
		return lastId++;
	}

	// constructor to create an predefined attributed agent
	public AgentInitialiser(Vector3 position, int sex, float loyalty, int libido, AgentCreator creator){


		this.creator = creator;
		this.id = getNextId();
		this.age = 0.0f;
		this.sex = sex;
		this.loyalty = loyalty;
		this.libido = libido;

		// set if they can give birth or not.
		if(this.sex == AgentCreator.sexWhoCanGiveBirth){
			this.canGiveBirth = true;
		}

		this.appearance = creator.instantiatePrefab (position, this.sex);

		// Name the agent in the game.
		this.appearance.transform.name = "Agent - " + this.id;

		// This will be used for collision of the agents themselves.
		this.appearance.GetComponent<AgentBehaviour> ().agent = this;

		// numOfSexes biased animation curves then randomly generate the values to which will then be used as such: w% = w/(w+x+y+z)
		for(int j = 0; j < AgentCreator.numOfSexesAndGenders; j++){
			// Ensure that a biased animation curve is used when evaluating the gender Norms / sexualPreference Norms of each sex.
			if (this.sex == j) {
				this.gender.Add(this.creator.getPercentageFromAnimCurve (this.creator.biasedTowardsPossibilityCurve));

				this.sexPreferences.Add(this.creator.getPercentageFromAnimCurve (this.creator.biasedAgainstPossibilityCurve)); // more likely to be most interested in other genders - not same gender.
			} else {
				// Randomly assign which animation curve for each aspect of the gender of the agent that isnt the genderNorm of the sex.
				int whichAnimCurve = Random.Range (0, 3);
				switch(whichAnimCurve){
				case 0:
					this.gender.Add(this.creator.getPercentageFromAnimCurve (this.creator.biasedTowardsPossibilityCurve));
					this.sexPreferences.Add(this.creator.getPercentageFromAnimCurve (this.creator.biasedTowardsPossibilityCurve));
					break;
				case 1:
					this.gender.Add(this.creator.getPercentageFromAnimCurve (this.creator.linearPossibilityCurve));
					this.sexPreferences.Add(this.creator.getPercentageFromAnimCurve (this.creator.linearPossibilityCurve));
					break;
				case 2:
					this.gender.Add(this.creator.getPercentageFromAnimCurve (this.creator.biasedAgainstPossibilityCurve));
					this.sexPreferences.Add(this.creator.getPercentageFromAnimCurve (this.creator.biasedAgainstPossibilityCurve));
					break;
				}
			}
		}

		// Now we have values for each aspect of the agents gender, we now need to make them percentages.
		float genderAspectsTotal = default(float);
		// To do this, we first sum all of the aspects of the agent's gender
		genderAspectsTotal = this.gender.Sum(); 

		// We then make each of the elements into percentages
		for(int aspectOfGender = 0; aspectOfGender < this.gender.Count; aspectOfGender++){
			this.gender[aspectOfGender] = this.gender.ElementAt (aspectOfGender) / genderAspectsTotal; 
		}

		// We do the same for the sexual preferences!
		float sexualPreferenceAspectsTotal = default(float);
		sexualPreferenceAspectsTotal = this.sexPreferences.Sum(); 

		for(int aspectOfSexualPreference = 0; aspectOfSexualPreference < this.gender.Count; aspectOfSexualPreference++){
			this.sexPreferences[aspectOfSexualPreference] = this.sexPreferences.ElementAt (aspectOfSexualPreference) / sexualPreferenceAspectsTotal; 
		}

		// Set the colour of the agent based on their percentages of each gender aspect.
		ColourCreator.getColour(this);

	}
}