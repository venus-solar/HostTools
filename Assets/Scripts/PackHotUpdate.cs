using UnityEngine;
using System.Collections;
using HostTool;

public class PackHotUpdate : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void onPackHotUpdate()
    {
        Debug.Log("onPackHotUpdate");
        string ret;
        Main.HandleCmd(Main.CmdType.PackHotUpdate, out ret);
        Debug.Log(ret);
    }
}
