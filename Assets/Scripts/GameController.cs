using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class GameController : MonoBehaviour {

    //teamLists keep track of all units on the map sorted into various teams
    //these are static so you can use them via GameController.teamLists[teamID][unitID]

    public static int teams = 2;
    public static List<List<GameObject>> teamLists = new List<List<GameObject>>();
    public static List<int> ticketList = new List<int>();
    public static List<GameObject> bases = new List<GameObject>();
    public static List<GameObject> nodes = new List<GameObject>();
    public List<GameObject> nodesPublic;
    public float FPS;
    public static List<deathLog> killBoard;

    //Caching
    private AstarPath aPath;

    // Use this for initialization
    void Start () {
        int j = 0;
        for(int i=0; i< teams; i++)
        {
            string teamName = "Team" + i;
            Debug.Log(teamName);
            GameObject[] temp = GameObject.FindGameObjectsWithTag(teamName);
            List<GameObject> temp2 = new List<GameObject>();
            foreach(GameObject item in temp)
            {
                temp2.Add(item);
            }
            teamLists.Add(temp2);
            ticketList.Add(j);
        }
        GameObject[] temp1 = GameObject.FindGameObjectsWithTag("Node");
        if (temp1.Length == 0) Debug.LogError("No nodes found. ABORT! ABORT!");
        foreach (GameObject item in temp1)
        {
            nodes.Add(item);
        }
        killBoard = new List<deathLog>();
    }

    // Update is called once per frame
    void Update () {
        FPS = 1 / Time.deltaTime;
        foreach (List<GameObject> item in teamLists) filterList(item);
        nodesPublic = nodes;
    }

    public static void filterList<T>(List<T> list)
    {
        if (list.Count == 0) return;
        for(int i=0; i<list.Count; i++)
        {
            if (list[i] == null) list.RemoveAt(i);
        }
    }

    public void tempEnableAggro()
    {
        foreach (GameObject unit in teamLists[0]) unit.GetComponent<AI>().selectByAggro = true;
    }

    public void tempDisableAggro()
    {
        foreach (GameObject unit in teamLists[0]) unit.GetComponent<AI>().selectByAggro = false;
    }

    public void writeDeathLog()
    {
        foreach(deathLog log in killBoard)
        {
            Debug.Log("name: " + log.name);
            Debug.Log("killed by: " + log.killedBy);
            Debug.Log("time of death: " + log.timeOfDeath);
            Debug.Log("assists: " + log.assists.ToString());
        }
    }
}
