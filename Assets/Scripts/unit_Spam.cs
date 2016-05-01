using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class unit_Spam : behavior_Default {

    public int splitCapacity;

	// Use this for initialization
	new void Start () {
        base.Start();
        type = "Spam";
	}
	
	// Update is called once per frame
	new void Update () {
        base.Update();
        if (lifeTime < maxLifeTime / 2 && splitCapacity > 0 && lifeTime > 1) split();
	}

    private void split()
    {
        GameObject newSpam = spawn(transform.position, this.moveSpeed, this.damage, this.attackRange, this.acquisitionRange, this.RoF, this.hp, this.detectionRange, this.teamID, this.lifeTime, this.destination);
        newSpam.GetComponent<AI>().maxLifeTime = this.maxLifeTime / 2;
        newSpam.GetComponent<unit_Spam>().splitCapacity = this.splitCapacity - 1;

        newSpam = spawn(transform.position, this.moveSpeed, this.damage, this.attackRange, this.acquisitionRange, this.RoF, this.hp, this.detectionRange, this.teamID, this.lifeTime, this.destination);
        newSpam.GetComponent<AI>().maxLifeTime = this.maxLifeTime / 2;
        newSpam.GetComponent<unit_Spam>().splitCapacity = this.splitCapacity - 1;
        Debug.Log(name + " has successfully split!");
        Destroy(this.gameObject);
        Debug.LogError("Failure to destroy after split");
    }

}
