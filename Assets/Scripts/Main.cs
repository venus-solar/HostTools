using UnityEngine;
using UnityEngine.UI;

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Net.Sockets;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HostTool
{
    public class Main : MonoBehaviour
    {
        struct ServerConfig
        {
#pragma warning disable 0649
            public String host;
            public String port;
#pragma warning restore
        }

        public enum CmdType
        {
            Invalid = -1,
            PackRes = 0,
            PackApp = 1,
            PackHotUpdate = 2,
            GetBranches = 20,
            NoticeIsDev = 21,
            NoticeBranch = 22,
        }

        public enum OsType
        {
            Invalid = -1,
            IOS = 0,
            ANDROID = 1,
            IOS_ANDROID = 2,
        }

        static String ServerConfigFileName = "SERVER_CONFIG.json";
        static ServerConfig SrvCfg;
        static String SrvHost;
        static Int32 SrvPort;
        static TcpClient TcpCliHandle;
        static Boolean IsDev = false;
        static String CurBranch;
        static String[] SrvBranches;
        public static OsType CurOsType = OsType.Invalid;
        public GameObject BnBr;
        public Canvas canvas1;

        static void LoadCfg()
        {
            String url = Application.dataPath + "/Scripts/" + ServerConfigFileName;
            if (!File.Exists(url))
                throw new Exception(string.Format("cant find cfg file:{0}", ServerConfigFileName));
            Console.WriteLine(string.Format("load cfg file: {0}", ServerConfigFileName));
            var cfgData = File.ReadAllText(url);
            SrvCfg = JsonConvert.DeserializeObject<ServerConfig>(cfgData);
            SrvHost = SrvCfg.host;
            bool ret = Int32.TryParse(SrvCfg.port, out SrvPort);
            if (!ret)
            {
                Console.WriteLine(string.Format("Invalid server port: {0}", SrvCfg.port));
                throw new Exception("Invalid server port");
            }
        }

        // Use this for initialization
        void Start()
        {
            Debug.Log("Main start, loading config and connecting server");
            LoadCfg();
            Debug.Log("Start Connecting ...");
            Connect(SrvHost, SrvPort);
            String ret;
            if (IsDev)
            {
                Debug.Log("Notice it is Dev Version");
                HandleCmd(CmdType.NoticeIsDev, out ret);
            }
            Debug.Log("Handle Cmd Get Branches...");
            HandleCmd(CmdType.GetBranches, out ret);
            Debug.Log(String.Format("Get branches: {0} the length is {1}", ret, ret.Length));
            char[] delimiterChars = {' ', '?', '*', '\n', '\r'};
            string[] words = ret.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            Int32 i = 0;
            foreach (String word in words)
            {
                // Add button
                Debug.Log(String.Format("Adding button {0}", word));
                GameObject bn = (GameObject)Instantiate(BnBr, new Vector3(0, 160 - 50 * i, 0), Quaternion.identity);
                bn.name = word;
                bn.transform.SetParent(canvas1.transform, false);
                GameObject txt = bn.transform.Find("Text").gameObject;
                (txt.GetComponent<Text>() as Text).text = word;
                (bn.GetComponent<Button>() as Button).onClick.AddListener(() => { onBrClicked(bn.name);});
                i++;
            }
            // TODO update branches show
        }

        public void onBrClicked(String name)
        {
            Debug.Log(String.Format("{0} is clicked", name));
            CurBranch = name;
            String ret;
            HandleCmd(CmdType.NoticeBranch, out ret);
            Application.LoadLevel(1);
        }

        static void Connect(String server, Int32 port)
        {
            try
            {
                Debug.Log("Get TcpCliHandle");
                TcpCliHandle = new TcpClient(server, port);
                if (TcpCliHandle == null)
                {
                    Debug.Log("TcpCliHandle is null");
                }
                Debug.Log("Done Get TcpCliHandle");
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public static void HandleCmd(CmdType ct, out string ret)
        {
            if (TcpCliHandle != null)
            {
                try
                {
                    string newstr;
                    if (ct == CmdType.GetBranches || ct == CmdType.NoticeIsDev)
                    {
                        newstr = ct.ToString();
                    }
                    else
                    {
                        newstr = ct.ToString() + "?" + CurBranch + "?" + CurOsType.ToString();
                    }
                    Debug.Log(newstr);

                    Byte[] data = Encoding.ASCII.GetBytes(newstr);
                    NetworkStream stream = TcpCliHandle.GetStream();
                    Debug.Log(data.Length);
                    stream.Write(data, 0, data.Length);
                    data = new Byte[256];
                    Int32 bytes = stream.Read(data, 0, data.Length);
                    ret = Encoding.ASCII.GetString(data, 0, bytes);
                    Debug.Log("Done send cmd, return ...");
                    return;
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("ArgumentNullException: {0}", e);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
            }
            ret = String.Empty;
            return;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
