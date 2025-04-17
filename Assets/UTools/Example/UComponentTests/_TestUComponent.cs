using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTools;

public class _TestUComponent : MonoBehaviour
{
    private Highlighter highlight;
    // Start is called before the first frame update
    void Start()
    {
        highlight = GetComponent<Highlighter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            highlight.StartHighlight();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            highlight.StopHighlight();
        }
    }
}
