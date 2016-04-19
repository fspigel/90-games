using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class unit_Jailbreak : behavior_Attacker {

    private string buffName = "allAroundBuff";
    private float radius = 10;

	// Use this for initialization
	new void Start () {
        base.Start();
        type = "Jailbreak";
	}
	
	// Update is called once per frame
	new void Update () {
        base.Update(); 
        buffAlliesInRadius();
	}

    private void buffAlliesInRadius()
    {
        foreach (List< GameObject> list in GameController.teamLists)
        {
            foreach (GameObject unit in list)
            {
                if (unit.GetComponent<AI>().teamID == this.teamID)
                {
                    if ((this.transform.position - unit.transform.position).sqrMagnitude < Mathf.Pow(radius, 2))
                    {
                        applyBuff(unit);
                    }
                    else
                    {
                        removeBuff(unit);
                    }
                }
            }
        }
    }

    private void applyBuff(GameObject unit)
    {
        AI tempAI = unit.GetComponent<AI>();
        if (!tempAI.modifiers.Contains(buffName))
        {
            Debug.Log("Unit " + unit.transform.name + " is getting buffed!");
            tempAI.damage++;
            tempAI.hp++;
            tempAI.moveSpeed++;
            tempAI.capSpeed++;
            tempAI.modifiers.Add(buffName);
        }
    }

     private void removeBuff(GameObject unit)
    {
        AI tempAI = unit.GetComponent<AI>();
        if (tempAI.modifiers.Contains(buffName))
        {
            Debug.Log("Unit " + unit.transform.name + " is getting debuffed!");

            tempAI.damage--;
            tempAI.hp--;
            tempAI.moveSpeed--;
            tempAI.capSpeed--;
            tempAI.modifiers.Remove(buffName);
        }
    }
}
