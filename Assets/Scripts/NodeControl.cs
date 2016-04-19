using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NodeControl : MonoBehaviour {

    public float capRadius = 5;
    public int ownerTeamID = -1;
    public int cappingTeamID = -1;
    public float ownership = 0;
    public int ticketValue = 10;
    public Text ownerDisp;
    public Text ownershipDisp;
    public List<GameObject> linkedNodes;
    public List<GameObject> unitsPresent;
    public Collider2D[] collidersPresent;

	// Use this for initialization
	void Start () {
        StartCoroutine(cap());
        StartCoroutine(tick());
        assignColor();
        foreach(GameObject node in linkedNodes)
        {
            if (!node.GetComponent<NodeControl>().linkedNodes.Contains(gameObject)) node.GetComponent<NodeControl>().linkedNodes.Add(gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
        ownership = Mathf.Clamp(ownership, 0,  100);
        collidersPresent = Physics2D.OverlapCircleAll(transform.position, capRadius);
        if (ownership <= 0)
        {
            ownership = 100;
            ownerTeamID = cappingTeamID;
            foreach(Collider2D unit in collidersPresent)
            {
                if (unit.transform.parent.GetComponent<AI>() != null)
                {
                    AI tempAI = unit.transform.parent.GetComponent<AI>();
                    if (tempAI.teamID == ownerTeamID)
                    {
                        tempAI.onHasCapped();

                    }
                }
            }
            assignColor();
        }
        ownerDisp.text = "Owner: " + ownerTeamID;
        ownershipDisp.text = "" + ownership;
        debugger();
	}

    /// <summary>
    /// Assign color according to which team owns the node - grey if neutral
    /// </summary>
    /// <returns></returns>
    private void assignColor()
    {
        switch (ownerTeamID)
        {
            case 0:
                GetComponent<SpriteRenderer>().color = Color.green;
                Debug.Log("attempting to switch color to green");
                break;
            case -1:
                GetComponent<SpriteRenderer>().color = Color.grey;
                break;
            default:
                GetComponent<SpriteRenderer>().color = Color.red;
                break;
        }
    }

    /// <summary>
    /// Periodically update the "ownership" value of the node - should only be used in Start()
    /// </summary>
    /// <returns></returns>
    private IEnumerator cap()
    {
        while (true)
        {
            Collider2D[] temp = Physics2D.OverlapCircleAll(transform.position, capRadius);
            bool selectedBaseTeam = false;
            int baseTeamID = -1;
            foreach (Collider2D item in temp)
            {
                if (item.transform.parent.gameObject.GetComponent<AI>() != null)
                {
                    AI tempAI = item.transform.parent.gameObject.GetComponent<AI>();
                    if (!selectedBaseTeam)
                    {
                        selectedBaseTeam = true;
                        baseTeamID = tempAI.teamID;
                    }
                    else if (tempAI.teamID != baseTeamID)
                    {
                        yield return new WaitForSeconds(1);
                        continue;
                    }
                }
            }
            foreach (Collider2D item in temp)
            {
                if (item.transform.parent.gameObject.GetComponent<AI>() != null)
                {
                    AI tempAI = item.transform.parent.gameObject.GetComponent<AI>();
                    if (tempAI.teamID == ownerTeamID) ownership += tempAI.capSpeed;
                    else ownership -= tempAI.capSpeed;
                    cappingTeamID = baseTeamID;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
    
    /// <summary>
    /// Periodically add tickets to the owner team
    /// </summary>
    /// <returns></returns>
    private IEnumerator tick()
    {
        while (true)
        {
            if(ownerTeamID != -1) GameController.ticketList[ownerTeamID] += ticketValue;
            yield return new WaitForSeconds(1);
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.transform.parent.gameObject.GetComponent<AI>() != null)
        {
            unitsPresent.Add(coll.transform.parent.gameObject);
        }
    }

    private void debugger()
    {
        foreach(GameObject node in linkedNodes)
        {
            Debug.DrawLine(transform.position, node.transform.position);
        }
    }
}
