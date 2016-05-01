using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class behavior_Attacker : AI
{

    // Use this for initialization
    new protected void Start()
    {
        behavior = "Attacker";
        base.Start();
        destination = randomPointOnCircle(targetNode.transform.position, 5);
    }

    // Update is called once per frame
    
    new protected void Update()
    {
        base.Update();
        acquireTarget();
        if(target != null)
        {
            base.engage();
        }
        if (currentNode != targetNode && (targetNode.transform.position - destination).sqrMagnitude > Mathf.Pow(targetNode.GetComponent<NodeControl>().capRadius, 2))
        {
            do
            {
                destination = randomPointOnCircle(targetNode.transform.position, targetNode.GetComponent<NodeControl>().capRadius);
            } while ((destination - targetNode.transform.position).sqrMagnitude < Mathf.Pow(targetNode.GetComponent<SpriteRenderer>().bounds.extents.x, 2));
        }
        else if (!isCapping && !isDefensive)
        {
            if (target == null)
            {

                if (destination != Vector3.zero) moveTo(destination);
                else destination = randomPointOnCircle(targetNode.transform.position, targetNode.GetComponent<NodeControl>().capRadius);
            }
            else
            {
                base.engage();
            }
        }
        else if (!isDefensive)
        {
            if (destination != Vector3.zero) moveTo(destination);
            if (currentNode == finalTargetNode)
            {
                finalTargetNode = null;
                isDefensive = true;
                acquisitionRange *= 2;
            }
        }

        if (currentNode == finalTargetNode && finalTargetNode.GetComponent<NodeControl>().ownerTeamID!=teamID && !isDefensive)
        {
            finalTargetNode = null;
            isDefensive = true;
        }
    }

    public override void onHasCapped()
    {
        Debug.Log("Hello World");
        base.onHasCapped();
        //nodeSelect();
        if (finalTargetNode == null) return;
        destination = randomPointOnCircle(targetNode.transform.position, targetNode.transform.GetComponent<NodeControl>().capRadius);
    }

    void OnTrigger2DEnter(Collider2D coll)
    {
        if (coll.transform.gameObject.GetComponent<NodeControl>() != null)
        {
            currentNode = coll.transform.gameObject;
        }
    }

    void OnTrigger2DStay(Collider2D coll)
    {
        if(coll.transform.gameObject == currentNode && !isCapping)
        {
            nodeSelect();
            destination = randomPointOnCircle(targetNode.transform.position, targetNode.transform.GetComponent<NodeControl>().capRadius);
        }
    }

    void OnTrigger2DExit(Collider2D coll)
    {
        if(coll.transform.gameObject.GetComponent<NodeControl>() != null)
        {
            currentNode = null;
        }
    }
}