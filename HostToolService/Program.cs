using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace HostToolService
{
    
    class Program
    {
        struct ServerConfig
        {
#pragma warning disable 0649
            public String host;
            public String port;
#pragma warning restore
        };

        public enum CmdType
        {
            Invalid = -1,
            PackRes = 0,
            PackApp = 1,
            PackHotUpdate = 2,
            GetBranches = 20,
            NoticeIsDev = 21,
            NoticeBranch = 22,
        };

        public enum OsType
        {
            Invalid = -1,
            IOS = 0,
            ANDROID = 1,
            IOS_ANDROID = 2,
        };

        delegate Boolean CmdFunc(String br, OsType ot, NetworkStream cs);

        static String ServerConfigFileName = "SERVER_CONFIG.json";
        static ServerConfig SrvCfg;
        static String SrvHost;
        static Int32 SrvPort;
        static TcpClient TcpCliHandle;
        static Dictionary<CmdType, CmdFunc> CmdOps = new Dictionary<CmdType, CmdFunc>
        {
            {CmdType.PackRes, (CmdFunc) PackRes},
            {CmdType.PackApp, (CmdFunc) PackApp},
            {CmdType.PackHotUpdate, (CmdFunc) PackHotUpdate},
            {CmdType.GetBranches, (CmdFunc) GetBranches},
            {CmdType.NoticeIsDev, (CmdFunc) NoticeIsDev},
            {CmdType.NoticeBranch, (CmdFunc) NoticeBranch},
        };
        static NetworkStream CliStream;
        static Boolean IsDev = false;
        public static String CurBranch = String.Empty;

        static void LoadCfg()
        {
            if (!File.Exists(ServerConfigFileName))
                throw new Exception(string.Format("cant find cfg file:{0}", ServerConfigFileName));
            Console.WriteLine(string.Format("load cfg file: {0}", ServerConfigFileName));
            var cfgData = File.ReadAllText(ServerConfigFileName);
            SrvCfg = JsonConvert.DeserializeObject<ServerConfig>(cfgData);
            SrvHost = SrvCfg.host;
            bool ret = Int32.TryParse(SrvCfg.port, out SrvPort);
            if (!ret)
            {
                Console.WriteLine(string.Format("Invalid server port: {0}", SrvCfg.port));
                throw new Exception("Invalid server port");
            }
        }
        public static void Main()
        {
            LoadCfg();
            TcpListener server = null;
            try
            {
                Int32 port = SrvPort;
                IPAddress localAddr = IPAddress.Parse(SrvHost);

                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    NetworkStream cs = client.GetStream();

                    var cliThread = new Thread(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                Byte[] bytes = new Byte[256];
                                String data = null;
                                int num;
                                if ((num = cs.Read(bytes, 0, bytes.Length)) != 0)
                                {
                                    data = Encoding.ASCII.GetString(bytes, 0, num);
                                    Console.WriteLine("Received: {0}", data);
                                    HandleMsg(data, cs);
                                }
                            }
                            catch (SocketException e)
                            {
                                Console.WriteLine("SocketException: {0}", e);
                                client.Close();
                            }
                        }
                    });
                    cliThread.Start();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }

        static public void HandleMsg(String msg, NetworkStream cs)
        {
            char[] delimiterChars = {'?'};
            System.Console.WriteLine("Original message: '{0}'", msg);

            string[] words = msg.Split(delimiterChars);
            string br = String.Empty;
            OsType ot = OsType.Invalid;
            if (words.Length == 3)
            {
                System.Console.WriteLine("HandleMsg words[1] words[2] {0}, {1}", words[1], words[2]);
                br = words[1];
                ot = (OsType)Enum.Parse(typeof(OsType), words[2], true);
            }
            CmdType ct = (CmdType) Enum.Parse(typeof(CmdType), words[0], true);
            CmdFunc mt;
            Boolean ret = CmdOps.TryGetValue(ct, out mt);
            if (ret)
            {
                System.Console.WriteLine("HandleMsg Call mt");
                mt(br, ot, cs);
            }
        }

        static Boolean PackRes(String br, OsType ot, NetworkStream cs)
        {
            System.Console.WriteLine("PackRes");
            String data = "打资源成功!";
            EchoCmd(data, cs);
            Boolean ret = true;
            return ret;
        }

        static Boolean PackApp(String br, OsType ot, NetworkStream cs)
        {
            System.Console.WriteLine("PackApp");
            String data = "打包成功!";
            EchoCmd(data, cs);
            Boolean ret = true;
            return ret;
        }
        static Boolean PackHotUpdate(String br, OsType ot, NetworkStream cs)
        {
            System.Console.WriteLine("PackHotUpdate");
            String data = "打热更新成功!";
            EchoCmd(data, cs);
            Boolean ret = true;
            return ret;
        }

        static Boolean GetBranches(String br, OsType ot, NetworkStream cs)
        {
            String data = String.Empty;
            String binPath = String.Empty ;
            if (Environment.OSVersion.ToString().Contains("Windows"))
            {
                binPath = @"c:\Program Files (x86)\Git\bin\git.exe";
            }
            else
            {
                binPath = @"/bin/usr/git";
            }
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = binPath,
                    Arguments = "branch",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                data = data + "?" + line;
            }
            EchoCmd(data, cs);
            Boolean ret = true;
            return ret;
        }

        static Boolean NoticeIsDev(String br, OsType ot, NetworkStream cs)
        {
            System.Console.WriteLine("NoticeIsDev");
            IsDev = true;
            CurBranch = br;
            String data = "通知是否Dev成功";
            EchoCmd(data, cs);
            Boolean ret = true;
            return ret;
        }
        static Boolean NoticeBranch(String br, OsType ot, NetworkStream cs)
        {
            System.Console.WriteLine("NoticeBranch");
            CurBranch = br;
            String data = "通知分支成功";
            EchoCmd(data, cs);
            Boolean ret = true;
            return ret;
        }

        static void EchoCmd(String data, NetworkStream cs)
        {
            byte[] msg = Encoding.ASCII.GetBytes(data);
            // Send back a response.
            cs.Write(msg, 0, msg.Length);
        }
    }
}
