using UnityEngine;
using System.Collections;

using System.Collections
.Generic
;public class RangeDisplay : MonoBehaviour {

    public GameObject circle;
    public bool displayRange = false;
    public float radius;

    private float spriteSize;

	// Use this for initialization
	void Start () {
        spriteSize = circle.GetComponent<SpriteRenderer>().bounds.extents.x;
    }

    // Update is called once per frame
    void Update () {
        Vector3 newScale = new Vector3(radius / spriteSize, radius / spriteSize, 1);
        circle.transform.localScale = newScale;

        if (displayRange) circle.SetActive(true);
        else circle.SetActive(false);
    }
}
