using UnityEngine;
using System.Collections;

using System.Collections
.Generic
;public class testingScript : MonoBehaviour {

    public GameObject node1;
    public GameObject node2;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void testNodePicker()
    {
        nodePicker(node1, node2);
    }

    protected GameObject nodePicker(GameObject startingNode, GameObject finalNode)
    {
        List<GameObject> possibleNodes = startingNode.GetComponent<NodeControl>().linkedNodes;
        float minDistance = 1000000;
        GameObject tempNode = null;
        foreach (GameObject node in possibleNodes)
        {
            float distance = (node.transform.position - startingNode.transform.position).sqrMagnitude + nodeCrawler(node, finalNode);
            if (distance < minDistance)
            {
                minDistance = distance;
                tempNode = node;
            }
        }
        if (tempNode == null) Debug.LogError("nodePicker failure for finalnode " + finalNode.transform.name);
        Debug.DrawLine(startingNode.transform.position, tempNode.transform.position, Color.green, 1f);
        return tempNode;
    }

    protected float nodeCrawler(GameObject startingNode, GameObject finalNode, List<GameObject> passedNodes)
    {
        passedNodes.Add(startingNode);
        List<GameObject> nodes = startingNode.GetComponent<NodeControl>().linkedNodes;
        float minDistance = 1000000;
        float f = 1000000;
        Vector3 tempPos = new Vector3();

        if (startingNode.GetComponent<NodeControl>().linkedNodes.Contains(finalNode))
        {
            passedNodes.Add(finalNode);
            return (finalNode.transform.position - startingNode.transform.position).sqrMagnitude;
        }

        foreach (GameObject node in startingNode.GetComponent<NodeControl>().linkedNodes)
        {
            if (!passedNodes.Contains(node))
            {
                f = (node.transform.position - startingNode.transform.position).sqrMagnitude + nodeCrawler(node, finalNode, passedNodes);
                if (f < minDistance)
                {
                    tempPos = node.transform.position;
                    minDistance = f;
                }
            }
        }
        Debug.DrawLine(startingNode.transform.position, tempPos, Color.green, 1, false);
        return f;
    }

    protected float nodeCrawler(GameObject startingNode, GameObject finalNode)
    {
        List<GameObject> newList = new List<GameObject>();
        float t = nodeCrawler(startingNode, finalNode, newList);
        return t;
    }

}
