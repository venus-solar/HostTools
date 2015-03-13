using UnityEngine;
using System.Collections;
using HostTool;

public class PackRes : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void onPackRes()
    {
        Debug.Log("onPackRes");
        string ret;
        Main.HandleCmd(Main.CmdType.PackRes, out ret);
        Debug.Log(ret);
    }
}
