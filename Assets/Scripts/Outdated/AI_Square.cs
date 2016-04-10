using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class AI_Square : AI {
    //SEE AI_Diamond DOCUMENTATION
    //this had a lot of overlap with diamond, consider making an abstract parent class

    public float buffRadius = 4;

    // Use this for initialization
    new void  Start () {
        base.Start();
        type = "Buffer";
        StartCoroutine(selectDestinationIterator());
    }

    // Update is called once per frame
    new void Update () {
        lifeTime -= Time.deltaTime;
        GameController.filterList(localTargets);
        GameController.filterList(targetsInRange);
        Vector3 newScale = new Vector3(1, healthBarScale * hp / maxhp, 1);
        healthBar.transform.localScale = newScale;
        moveTo(destination);
        Debug.DrawLine(transform.position, destination);
    }

    private IEnumerator selectDestinationIterator()
    {
        while (true)
        {
            if ((destination = selectDestination(transform.position, 15, buffRadius, teamID)) == Vector3.zero)
            {
                destination = AI.selectDestinationGlobal(transform.position, teamID);
            }

            yield return new WaitForSeconds(1);
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        GameObject temp = coll.transform.parent.gameObject;
        if (temp.gameObject.GetComponent<AI>() != null)
        {
            if (temp.gameObject.GetComponent<AI>().teamID == teamID)
            {
                temp.gameObject.GetComponent<AI>().selectByAggro = true;
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
                temp.gameObject.GetComponent<AI>().selectByAggro = false;
            }
        }
    }
}
