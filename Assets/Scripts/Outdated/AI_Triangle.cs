using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Triangle : AI {

    public GameObject laser;        //holds the graphics for the laser
    float laserLength;
    public Vector3 endpoint;
    private Vector3 defaultDirection;

	// Use this for initialization
	new void Start () {
        base.Start();
        type = "Attacker";
        laserLength = laser.GetComponent<SpriteRenderer>().bounds.size.y;       //set laserLength to the base length of the laser sprite
	}
	
	// Update is called once per frame
	new void Update () {
        base.Update();
        laser.transform.rotation = bodyGraphics.transform.rotation;             //make sure laser is lined up with bodyGraphics
        if (target == null)                                                     //if there is no target, deactivate laser and move in defaultDirection
        {
            laser.SetActive(false);
            if (destination != Vector3.zero) moveTo(destination);
            else move(defaultDirection);
        }
        else
        {
            engage();
        }
    }


    public override bool engage()
    {
        if (base.engage())  
        {
            RaycastHit2D cast = Physics2D.Raycast(transform.position + (target.transform.position - transform.position).normalized * 0.3f, target.transform.position - transform.position);
            //Debug.DrawRay(transform.position, target.transform.position - transform.position);
            endpoint = new Vector3(cast.point.x, cast.point.y, 0);

            laser.SetActive(true);
            Vector3 newScale = new Vector3(1, (endpoint - transform.position).magnitude  / laserLength, 1);
            laser.transform.localScale = newScale;
            return true;
        }
        else
        {
            laser.SetActive(false);
            return false;
        }

    }

    public void spawnTest()
    {
        GameObject newTriangle = (GameObject)Instantiate(gameObject, new Vector3(0, 0, 0), Quaternion.identity);
        newTriangle.GetComponent<AI_Triangle>().moveSpeed = moveSpeed;
        newTriangle.GetComponent<AI_Triangle>().damage = damage;
        newTriangle.GetComponent<AI_Triangle>().attackRange = attackRange;
        newTriangle.GetComponent<AI_Triangle>().acquisitionRange = acquisitionRange;
        newTriangle.GetComponent<AI_Triangle>().RoF = RoF;
        newTriangle.GetComponent<AI_Triangle>().hp = hp;
        newTriangle.GetComponent<AI_Triangle>().detectionRange = detectionRange;
        newTriangle.GetComponent<AI_Triangle>().teamID = teamID;
        newTriangle.GetComponent<AI_Triangle>().lifeTime = lifeTime;
        GameController.teamLists[teamID].Add(newTriangle);
    }

}
