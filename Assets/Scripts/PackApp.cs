using UnityEngine;
using System.Collections;
using HostTool;

public class PackApp : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void onPackApp()
    {
        Debug.Log("onPackApp");
        string ret;
        Main.HandleCmd(Main.CmdType.PackApp, out ret);
        Debug.Log(ret);
    }
}
