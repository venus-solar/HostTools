using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using HostTool;

public class CheckOs : MonoBehaviour {
    public GameObject TgIOS;
    public GameObject TgANDROID;
    private Main.OsType Ot;

    public void onCheckIOS(bool check)
    {
        Ot = Main.CurOsType;
        if (check)
        {
            switch (Ot)
            {
                case Main.OsType.ANDROID:
                    Ot = Main.OsType.IOS_ANDROID;
                    break;
                case Main.OsType.Invalid:
                    Ot = Main.OsType.IOS;
                    break;
                default:
                    Debug.Log(string.Format("state {0} has wrong, ignore", Ot.ToString()));
                    break;
            }
        }
        else
        {
            switch (Ot)
            {
                case Main.OsType.IOS:
                    Ot = Main.OsType.Invalid;
                    break;
                case Main.OsType.IOS_ANDROID:
                    Ot = Main.OsType.ANDROID;
                    break;
                default:
                    Debug.Log(string.Format("state {0} has wrong, ignore", Ot.ToString()));
                    break;
            }
        }
        Main.CurOsType = Ot;
    }

    public void onCheckANDROID(bool check)
    {
        Ot = Main.CurOsType;
        if (check)
        {
            switch (Ot)
            {
                case Main.OsType.IOS:
                    Ot = Main.OsType.IOS_ANDROID;
                    break;
                case Main.OsType.Invalid:
                    Ot = Main.OsType.ANDROID;
                    break;
                default:
                    Debug.Log(string.Format("state {0} has wrong, ignore", Ot.ToString()));
                    break;
            }
        }
        else
        {
            switch (Ot)
            {
                case Main.OsType.ANDROID:
                    Ot = Main.OsType.Invalid;
                    break;
                case Main.OsType.IOS_ANDROID:
                    Ot = Main.OsType.IOS;
                    break;
                default:
                    Debug.Log(string.Format("state {0} has wrong, ignore", Ot.ToString()));
                    break;
            }
        }
        Main.CurOsType = Ot;
    }

    void update_Ot()
    {
        bool aon = (TgANDROID.GetComponent<Toggle>() as Toggle).isOn;
        bool ion = (TgIOS.GetComponent<Toggle>() as Toggle).isOn;
        if (aon && ion)
        {
            Ot = Main.OsType.IOS_ANDROID;
        }
        else if (aon)
        {
            Ot = Main.OsType.ANDROID;
        }
        else if (ion)
        {
            Ot = Main.OsType.IOS;
        }
        else
        {
            Ot = Main.OsType.Invalid;
        }
        Main.CurOsType = Ot;
    }

	// Use this for initialization
	void Start () {
        update_Ot();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
