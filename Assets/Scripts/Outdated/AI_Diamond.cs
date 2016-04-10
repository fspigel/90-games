using UnityEngine;
using System.Collections;
using System.Collections.Generic

;public class AI_Diamond : AI {

    float radius = 6;       //radius of the damge buff

	// Use this for initialization
	new void Start () {
        type = "Buffer";
        radius = GetComponent<CircleCollider2D>().radius;
        StartCoroutine(selectDestinationIterator());
	}
	
	// Update is called once per frame
	new void Update () {
        base.Update();
        moveTo(destination);    //destination should constantly be up-to-date
        Debug.DrawLine(transform.position, destination);
	}

    //inscrease the damage of all friendly units within radius
    void OnTriggerEnter2D(Collider2D coll)
    {
        GameObject temp = coll.transform.parent.gameObject;
        if (temp.GetComponent<AI>() != null)
        {
            if (temp.GetComponent<AI>().teamID == teamID)
            {
                temp.GetComponent<AI>().damage *= 2;        
            }
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        GameObject temp = coll.transform.parent.gameObject;
        if (temp.gameObject.GetComponent<AI>() != null)
        {
            if (temp.gameObject.GetComponent<AI>().teamID == teamID)
            {
                temp.gameObject.GetComponent<AI>().damage /= 2;
            }
        }
    }

    //this updates destination periodically via the AI.selectDestination() and AI.selectDestinationGlobal() methods
    private IEnumerator selectDestinationIterator()
    {
        while (true)
        {
            if ((destination = AI.selectDestination(transform.position, 15, radius, teamID)) == Vector3.zero) //selectDestination() returns vector3.zero upon failure
            {
                destination = AI.selectDestinationGlobal(transform.position, teamID);
            }
            yield return new WaitForSeconds(1); //repeat every second
        }
    }

}
