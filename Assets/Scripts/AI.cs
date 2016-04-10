using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AI : MonoBehaviour {

    public string type;
    public string behavior;
    public int teamID;
    public float moveSpeed;
    public float baseMoveSpeed = 1;
    public float damage = 5;
    public float attackRange = 4; //testicles
    public float acquisitionRange = 10;
    public float RoF = 1;
    public float maxhp;
    public float hp;
    public bool selectByAggro = false;          //Does the unit determine target according to aggro?
    public bool stealthed = false;              //Is the unit stealthed? (overriden by detection)
    public float detectionRange = 0;            //Range in which stealthed units are revealed
    public float aggro = 0;                     //Aggro - more aggro means more likely to be targeted by "intelligent" hostiles
    public float maxLifeTime = 1000;            //Starting duration of the unit
    public float lifeTime;                      //How much duration (life time) is left
    public GameObject target;                   //Attackers will attempt to engage target
    public GameObject bodyGraphics;             //The child object that holds its visual appearance and rigidbody colliders
    public bool immune = false;                 //Godmode (testing only (or is it?))
    private float aggroMultiplier = 1;          //Use this to designate high-value targets such as casters, buffers etc.
    public GameObject healthBar;                //The child object containing the healthbar
    public float healthBarScale;                //This operated the healthbar - less health means smaller healthBarScale
    public Vector3 destination = Vector3.zero;  //This is an alternative to defaultDirection - a point in the game space the unit should move towards
    public float capSpeed = 1;                  //How fast does the unit cap nodes?
    public GameObject currentNode;
    private NodeControl currentNodeNodeControl;
    public GameObject targetNode;
    public GameObject finalTargetNode;
    public bool isCapping;
    public bool isDefensive;
    private bool someBool
    {
        set;
        get;
    }

    public List<GameObject> globalTargets = new List<GameObject>();         //These are lists used to sort all enemy units according to whether
    public List<GameObject> localTargets = new List<GameObject>();          //they are inside attackRange, acquisitionRange or outside both
    public List<GameObject> targetsInRange = new List<GameObject>();

    public bool attack = false;         //Is the unit currently attacking something?

    // Use this for initialization
    protected void Start () {
        hp = maxhp;
        moveSpeed = baseMoveSpeed;
        lifeTime = maxLifeTime;
        StartCoroutine("attackCycle");
        healthBarScale = healthBar.transform.localScale.y;
        StartCoroutine(establishAggro());
        StartCoroutine(acquireTarget());
        
        //TODO: overhaul damage process 
        /*GameObject obj = Physics2D.OverlapPoint(transform.position).gameObject;
        if (obj != null && obj.GetComponent<NodeControl>() != null)
        {
            currentNode = obj;
            targetNode = obj;
        }
        else
        {
            Debug.LogError("Unit instantiation attempted in non-node space. Terminating.");
            Destroy(gameObject);
        }*/
    }

    // Update is called once per frame
    protected void Update () {
        lifeTime -= Time.deltaTime;                                                 //subtract from unit's lifetime
        GameController.filterList(localTargets);                                    //filterlist() removes all null members of a list
        GameController.filterList(targetsInRange);
        Vector3 newScale = new Vector3(1, healthBarScale * hp / maxhp, 1);          //Update healthbar size according to hp remaining
        healthBar.transform.localScale = newScale;
        if (currentNodeNodeControl != null)
        {
            if (currentNodeNodeControl.ownerTeamID != teamID || currentNodeNodeControl.ownership < 100)
            {
                isCapping = true;
            }
            if (targetNode == currentNode && !isCapping && !isDefensive)
            {
                targetNode = nodePicker(currentNode, finalTargetNode);
            }
        }
        else
        {
            isCapping = false;
        }

        debugger();
    }

    protected void LateUpdate()
    {
        if (hp < 0) onDeath();
        if (lifeTime < 0) onDeath();        //onDeath() should only be called in LateUpdate
    }

    private void debugger() 
    {
        if (this.destination != Vector3.zero)
        {
            Debug.DrawLine(this.transform.position, this.destination);
        }
    }

    public GameObject spawn(
        Vector3 spawnPoint, 
        float moveSpeed,
        float damage, 
        float attackRange, 
        float acquisitionRange, 
        float RoF, 
        float maxhp, 
        float detectionRange, 
        int teamID, 
        float maxLifeTime, 
        Vector3 destination)
    {
        GameObject newUnit = (GameObject)Instantiate(gameObject, spawnPoint, Quaternion.identity);
        newUnit.GetComponent<AI>().moveSpeed = moveSpeed;
        newUnit.GetComponent<AI>().damage = damage;
        newUnit.GetComponent<AI>().attackRange = attackRange;
        newUnit.GetComponent<AI>().acquisitionRange = acquisitionRange;
        newUnit.GetComponent<AI>().RoF = RoF;
        newUnit.GetComponent<AI>().maxhp = maxhp;
        newUnit.GetComponent<AI>().detectionRange = detectionRange;
        newUnit.GetComponent<AI>().teamID = teamID;
        newUnit.GetComponent<AI>().maxLifeTime = maxLifeTime;
        newUnit.GetComponent<AI>().destination = destination;
        //GameController.teamLists[teamID].Add(newUnit);
        return newUnit;
    }

    public virtual void onHasCapped()
    {
        isCapping = false;
    }

    /// <summary>
    /// Move in the direction dir and orient graphics and colliders accordingly
    /// </summary>
    /// <param name="dir"></param>
    protected void move(Vector3 dir)        
    {
        orient(dir);
        this.transform.Translate(dir.normalized * moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Move towards the point destination
    /// </summary>
    /// <param name="destination"></param>
    protected void moveTo(Vector3 destination)  
    {
        if (range(this.destination) > 0.1f)
        {
            move(this.destination - this.transform.position);
        }
    }

    //DO NOT TOUCH, THIS TOOK FUCKING AGES
    /// <summary>
    /// Orient graphics and colliders according to the target field
    /// </summary>
    public void orient()
    {
        float rotZ;
        if (this.target != null)
        {
            rotZ = Vector3.Angle(new Vector3(0, 1, 0), this.target.transform.position - this.bodyGraphics.transform.position);
            if (this.target.transform.position.x > this.bodyGraphics.transform.position.x) rotZ *= -1;
        }
        else rotZ = 0;
        this.bodyGraphics.transform.rotation = Quaternion.Euler(0, 0, rotZ);
    }

    /// <summary>
    /// Orient towards a specific direction
    /// </summary>
    /// <param name="dir"></param>
    public void orient(Vector3 dir) 
    {
        float rotZ = Vector3.Angle(Vector3.up, dir);
        if (dir.x > 0) rotZ *= -1;
        this.bodyGraphics.transform.rotation = Quaternion.Euler(0, 0, rotZ);
    }

    /// <summary>
    /// Target acquisition process, to be repeated every .5 sec
    /// </summary>
    /// <returns></returns>
    protected IEnumerator acquireTarget()
    {
        while(true)
        {
            this.target = null;
            populate();     //populates the localTargets and targetsInRange lists through colliders
            //if no targets are in sight range, continue;
            if (this.localTargets.Count == 0)
            {
                yield return new WaitForSeconds(0.5f);
                continue;                                                   
            }
            else {
                //Target selection based on aggro (focus fire)
                if (this.selectByAggro)  
                {
                    this.target = unitWithHighestAggro();
                }

                //Target selection based on proximity
                else
                {
                    this.target = nearestEnemy();
                }
                yield return new WaitForSeconds(0.5f);  //after 0.5 seconds, the while(true) loop repeats
            }
        }
    }

    /// <summary>
    /// Automatically populates the lists globalTargets, localTargets, targetsInRange
    /// </summary>
    void populate()
    {
        this.localTargets.Clear();
        this.targetsInRange.Clear();
        Collider2D[] temp1 = Physics2D.OverlapCircleAll(this.transform.position, this.acquisitionRange);      //generate a list of all "visible" targets (targets in acquisitionRange)
        foreach(Collider2D coll in temp1)
        {
            GameObject temp = coll.transform.parent.gameObject;
            AI tempAI = temp.GetComponent<AI>();
            if (tempAI != null)
            {
                if (tempAI.teamID != this.teamID && !tempAI.stealthed)
                {
                    this.localTargets.Add(temp);
                }
            }
        }
        Collider2D[] temp2 = Physics2D.OverlapCircleAll(this.transform.position, this.attackRange);           //generate a list of all targets in attack range - possibly redundant?
        foreach (Collider2D coll in temp2)
        {
            GameObject temp = coll.transform.parent.gameObject;
            AI tempAI = temp.GetComponent<AI>();
            if (tempAI != null)
            {
                if (tempAI.teamID != this.teamID && !tempAI.stealthed)
                {
                    this.targetsInRange.Add(temp);
                }
            }
        }
    }

    /// <summary>
    /// Returns the distance to the GameObject occupying the 'target' field
    /// </summary>
    /// <returns></returns>
    protected float range()
    {
        //TODO: Investigate if it is worth it to use sqrMagnitude for better optimization
        if (this.target == null)
        {
            Debug.Log("Attempt to acces null object in AI.range()");
            return 0;
        }
        return range(this.target);
    }

    /// <summary>
    /// Returns the distance from the unit to t
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    protected float range(GameObject t)
    {
        if(t!= null) return (t.transform.position - this.transform.position).magnitude;
        if(target == null) Debug.Log("Attempt to acces null object in AI.range()");
        return 0;
    }

    /// <summary>
    /// Returns the distance to some position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    protected float range(Vector3 position)
    {
        return (position - this.transform.position).magnitude;
    }

    /// <summary>
    /// Returns the distance between two GameObjects
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public static float range(GameObject obj1, GameObject obj2)
    {
        return (obj1.transform.position - obj2.transform.position).magnitude;
    }

    //this is the attack function
    public virtual bool engage()
    {
        orient();
        //if target is out of range, move towards target and return false
        //TODO: determine range via collider, in case of large units
        if (range() > attackRange)
        {
            move(target.transform.position - this.transform.position);
            this.attack = false;
            return false;
        }
        //otherwise, allow attackCycle() to operate and return true
        else {
            this.attack = true;
            return true;
        }
    }

    //this is actually the attack function
    protected IEnumerator attackCycle()
    {
        while (true)
        {
            if (this.attack && target != null)   //make sure there is a target
            {
                this.target.GetComponent<AI>().doDamage(damage);
            }
            yield return new WaitForSeconds(1 / RoF);       //iterate according to unit's RoF field (rate of fire)
        }
    }

    //Aggro calculation
    //remember - Aggro is how enemies know who to attack, this value is only used by hostile units
    /// <summary>
    /// Dynamically calculates own unit's aggro
    /// </summary>
    /// <returns></returns>
    private IEnumerator establishAggro()
    {
        while (true)
        {
            aggro = 0;
            foreach (List<GameObject> list in GameController.teamLists)
            {
                foreach (GameObject unit in list) 
                {
                    AI temp = unit.GetComponent<AI>();
                    if (temp.teamID != teamID)
                    {
                        if (range(unit) < temp.acquisitionRange)        //for each enemy unit, evaluate whether this unit is in its range
                        {                                               //establish aggro as the sum of all DPS (damage*RoF)
                            aggro += temp.damage * temp.RoF;
                        }
                    }
                }
            }
            aggro *= aggroMultiplier;                                   //include multiplier (if any)
            aggro = aggro * maxhp / hp;                                 //increase aggro if hp is low
            yield return new WaitForSeconds(0.5f);                      //iterate every 0.5 sec
        }
    }

    /// <summary>
    /// Returns the node that is on the optimal path from startingNode to finalNode. This is how units should select where to go next.
    /// </summary>
    /// <param name="startingNode"></param>
    /// <param name="finalNode"></param>
    /// <returns></returns>
    protected GameObject nodePicker(GameObject startingNode, GameObject finalNode)
    {
        if (startingNode == finalNode) return startingNode;
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
        return tempNode;
    }

    /// <summary>
    /// Recursion; travels the node net and returns the square length of the shortest path from startingNode to finalNode.
    /// ::: Do not call this directly - use nodeCrawler(GameObject startingNode, GameObject finalNode) instead!
    /// </summary>
    /// <param name="startingNode"></param>
    /// <param name="finalNode"></param>
    /// <param name="passedNodes"></param>
    /// <returns></returns>
    protected float nodeCrawler(GameObject startingNode, GameObject finalNode, List<GameObject> passedNodes)
    {
        passedNodes.Add(startingNode);
        List<GameObject> nodes = startingNode.GetComponent<NodeControl>().linkedNodes;
        float minDistance = 1000000;
        float f = 1000000;

        if (nodes.Contains(finalNode))
        {
            passedNodes.Add(finalNode);
            return (finalNode.transform.position - startingNode.transform.position).sqrMagnitude;
        }

        foreach (GameObject node in nodes)
        {
            if (!passedNodes.Contains(node))
            {
                f = (node.transform.position - startingNode.transform.position).sqrMagnitude + nodeCrawler(node, finalNode, passedNodes);
                if (f < minDistance)
                {
                    minDistance = f;
                }
            }
        }
        return f;
    }

    /// <summary>
    /// Recursion; travels the node net and returns the square length of the shortest path from startingNode to finalNode.
    /// </summary>
    /// <param name="startingNode"></param>
    /// <param name="finalNode"></param>
    /// <returns></returns>
    protected float nodeCrawler(GameObject startingNode, GameObject finalNode)
    {
        List<GameObject> newList = new List<GameObject>();
        newList.Add(startingNode);
        float t = nodeCrawler(startingNode, finalNode, newList);
        return t;
    }

    //Here are the OnTriggerEnter/Exit2D functions - 
    //for now they are only used to determine whether unit is within the capture area of a node
    //possibly use for a lot of other shit
    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.GetComponent<NodeControl>() != null)
        {
            currentNode = coll.gameObject;
            currentNodeNodeControl = currentNode.GetComponent<NodeControl>();
            currentNodeNodeControl.unitsPresent.Add(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.GetComponent<NodeControl>() != null)
        {
            currentNode = null;
            currentNodeNodeControl = null;
            currentNodeNodeControl.unitsPresent.Remove(gameObject);
        }
    }

    /// <summary>
    /// Returns a random point on a circle
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static Vector3 randomPointOnCircle(Vector3 center, float radius)
    {

        float x = (Random.value - 0.5f) * radius * 2;
        float y = (Random.value - 0.5f) * radius * 2;
        Vector3 newVector = new Vector3(center.x + x, center.y + y, 0);
        return newVector;
    }

    /// <summary>
    /// Legacy code - use nodePicker(...) instead! Sets the destination to the linked node that is closest to finalTargetNode
    /// </summary>
    protected void nodeSelect()
    {
        if (finalTargetNode == null) return;
        GameObject tempTargetNode = null;
        if (currentNode != null)
        {
            if(currentNode.GetComponent<NodeControl>().ownerTeamID != teamID)
            {
                targetNode = currentNode;
                return;
            }
            float minDistance = (currentNode.transform.position - finalTargetNode.transform.position).magnitude;
            foreach (GameObject node in currentNode.GetComponent<NodeControl>().linkedNodes)
            {
                if ((node.transform.position - finalTargetNode.transform.position).magnitude < minDistance)
                {
                    tempTargetNode = node;
                    minDistance = (node.transform.position - finalTargetNode.transform.position).magnitude;
                }
            }
        }
        else
        {
            float minDistance = range(GameController.nodes[0]);
            foreach(GameObject node in GameController.nodes)
            {
                if(range(node) <= minDistance)
                {
                    minDistance = range(node);
                    tempTargetNode = node;
                    Debug.Log("Distance from " + node.transform.name + " is " + range(node) + "\n minDistance is " + minDistance);
                }
            }
        }
        if(tempTargetNode == null)
        {
            Debug.LogError("nodeSelect for " + transform.name + " has failed to locate a node. Terminating");
            return;
        }
        Debug.Log("setting targetNode to " + tempTargetNode.transform.name);
        targetNode = tempTargetNode;
    }

    /// <summary>
    /// Returns the point within the radius 'chkRadius' which will cover the highest number of friendly units with some buff
    /// </summary>
    /// <param name="center"></param>
    /// <param name="chkRadius"></param>
    /// <param name="buffRadius"></param>
    /// <param name="_teamID"></param>
    /// <returns></returns>
    public static Vector3 selectDestination(Vector3 center, float chkRadius, float buffRadius, int _teamID)
    {
        //TODO: convert to static
        int count = 0;
        int maxCount = 0;
        Vector3 tempDestination = Vector3.zero;
        //create a field of discrete points in a local area
        for (float i = center.x - chkRadius; i < center.x + chkRadius; i += 1)
        {
            for (float j = center.y - chkRadius; j < center.y + chkRadius; j += 1)
            {
                count = 0;
                Collider2D[] temp = Physics2D.OverlapCircleAll(new Vector2(i, j), buffRadius);      //count how many friendlies in the area around each point

                foreach (Collider2D item in temp)
                {
                    AI tempAI = item.transform.parent.gameObject.GetComponent<AI>();
                    if (tempAI != null)
                    {
                        if (tempAI.teamID == _teamID && tempAI.damage * tempAI.RoF > 0) count++;    //only count attackers for now (possibly re-implement using layermask?)
                    }
                }
                if (count > maxCount)
                {
                    maxCount = count;
                    tempDestination = new Vector3(i, j, 0);                                         //select the point with the highest number of friendlies around it
                }
            }
        }
        if (maxCount > 1) return tempDestination;                                                   //return the position of that point only if there is at least 1 unit found, otherwise return vector3.zero
        else return Vector3.zero;
    }

    /// <summary>
    /// Returns the position of the nearest friendly offensive unit
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="teamID"></param>
    /// <returns></returns>
    public static Vector3 selectDestinationGlobal(Vector3 origin, int teamID)
    {
        Vector3 temp = new Vector3(0, 0, 0);
        foreach (List<GameObject> list in GameController.teamLists)
        {
            if (list[0].GetComponent<AI>().teamID == teamID)
            {
                float minDistance = 1000;
                foreach (GameObject unit in list)
                {
                    if ((unit.transform.position - origin).magnitude < minDistance && unit.GetComponent<AI>().damage * unit.GetComponent<AI>().RoF > 0)
                    {
                        minDistance = (unit.transform.position - origin).magnitude;
                        temp = unit.transform.position;
                        Debug.DrawLine(origin, unit.transform.position);
                    }
                }
            }
        }
        return temp;
    }

    /// <summary>
    /// Returns the nearest enemy within acquisitionRange; null if none are present
    /// </summary>
    /// <returns></returns>
    protected GameObject nearestEnemy()
    {
        float minRange = 1000;
        GameObject tempTarget = null;
        foreach (GameObject item in localTargets)
        {
            if (range(item) < minRange)
            {
                minRange = range(item);
                tempTarget = item;
            }
        }
        return tempTarget;
    }

    /// <summary>
    /// Returns the nearest unit that is a member of unitList
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    protected GameObject nearestUnit(List<GameObject> unitList)
    {
        float minRange = 1000;
        GameObject tempTarget = null;
        foreach (GameObject item in unitList)
        {
            if (range(item) < minRange)
            {
                minRange = range(item);
                tempTarget = item;
            }
        }
        return tempTarget;
    }

    /// <summary>
    /// Returns the nearest unit in unitList of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="unitList"></param>
    /// <returns></returns>
    protected GameObject nearestUnit<T>(List<GameObject> unitList)
    {
        float minRange = 1000;
        GameObject tempTarget = null;
        foreach (GameObject item in unitList)
        {
            if (range(item) < minRange && item.GetComponent<T>() != null)
            {
                minRange = range(item);
                tempTarget = item;
            }
        }
        return tempTarget;
    }

    /// <summary>
    /// Returns the nearest enemy of type T in unitList
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="unitList"></param>
    /// <returns></returns>
    protected GameObject nearestEnemy<T>(List<GameObject> unitList)
    {
        float minRange = 1000;
        GameObject tempTarget = null;
        foreach (GameObject item in unitList)
        {
            if (range(item) < minRange && item.GetComponent<T>() != null && item.GetComponent<AI>().teamID != teamID)
            {
                minRange = range(item);
                tempTarget = item;
            }
        }
        return tempTarget;
    }

    /// <summary>
    /// Returns the enemy with the highest aggro value out of all enemies in acquisitionRange
    /// </summary>
    /// <returns></returns>
    protected GameObject unitWithHighestAggro()
    {
        GameObject tempTarget = null;
        float aggroMax = 0;
        foreach (GameObject item in localTargets)
        {
            if (item.GetComponent<AI>().aggro >= aggroMax && item.GetComponent<AI>() != null)
            {
                aggroMax = item.GetComponent<AI>().aggro;
                tempTarget = item;
            }
        }
        return tempTarget;
    }

    /// <summary>
    /// Inflicts damage to the parent unit
    /// </summary>
    /// <param name="damageDone"></param>
    public void doDamage(float damageDone)
    {
        if(!immune) this.hp -= damageDone;
        Debug.Log(transform.name + "is hit for " + damageDone + " damage");
    }

    /// <summary>
    /// Kill the unit
    /// </summary>
    protected void onDeath()
    {
        Debug.Log(transform.name + " is destroyed");
        Destroy(gameObject);
    }
}