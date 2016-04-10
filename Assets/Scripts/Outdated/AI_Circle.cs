using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Circle : AI {

    //SEE AI_Diamond DOCUMENTATION
    //this had a lot of overlap with diamond, consider making an abstract parent class

    float radius = 6;

    // Use this for initialization
    new void Start()
    {
        type = "Buffer";
        radius = GetComponent<CircleCollider2D>().radius;
        StartCoroutine(selectDestinationIterator());
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        moveTo(destination);
        Debug.DrawLine(transform.position, destination);
    }

    private IEnumerator selectDestinationIterator()
    {
        if ((destination = AI.selectDestination(transform.position, 15, radius, teamID)) == Vector3.zero)
        {
            destination = AI.selectDestinationGlobal(transform.position, teamID);
        }

        yield return new WaitForSeconds(1);
        StartCoroutine(selectDestinationIterator());
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        GameObject temp = coll.transform.parent.gameObject;
        if (temp.GetComponent<AI>() != null)
        {
            if (temp.GetComponent<AI>().teamID == teamID)
            {
                temp.GetComponent<AI>().moveSpeed = moveSpeed;
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
                temp.gameObject.GetComponent<AI>().moveSpeed = temp.gameObject.GetComponent<AI>().baseMoveSpeed;
            }
        }
    }

}
