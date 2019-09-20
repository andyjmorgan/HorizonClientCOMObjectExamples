using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMwareHorizonClientController;
using static ExampleLogonSimulator.Program.ClientEvents5.Helpers;

namespace ExampleLogonSimulator
{
    class Program
    {

        public static uint Serverid { get; set; }
        public static List<LaunchItem2> Launchitems { get; set; }
        public static bool hasAuthenticated { get; set; }
        public static bool hasLaunchItems { get; set; }
        public static bool hasFailed { get; set; }
        public static bool hasLaunched { get; set; }
        public static string LaunchID { get; set; }

        public static string launchtoken { get; set; }

        static string GetPassword()
        {
            string pass = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);
            Console.WriteLine();
            return pass;
        }
        static void WriteColor(string message, ConsoleColor messageColor, bool FullLine)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = messageColor;
            if (FullLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
            Console.ForegroundColor = currentColor;
        }

        static void FindAndKillSessions()
        {
            WriteColor("Active sessions detected, would you like to end them now? [y/n]", ConsoleColor.Yellow, false);
            var response = Console.ReadLine();
            if (response.ToLower() == "y")
            {
                var opensessions = System.Diagnostics.Process.GetProcessesByName("vmware-view");
                if (opensessions.Count() > 0)
                {
                    foreach (var p in System.Diagnostics.Process.GetProcessesByName("vmware-view"))
                    {
                        p.Kill();
                    }
                }
            }
        }

        static string GetValidInput(string Message, bool password = false)
        {
            string returnvalue = "";
            while (returnvalue.Length <= 0)
            {
                WriteColor(Message, ConsoleColor.Cyan, false);
                if (password)
                {
                    returnvalue = GetPassword();
                }
                else
                {
                    returnvalue = Console.ReadLine();
                }
            }
            return returnvalue;

        }
        static void Main(string[] args)
        {
            //TraceListener[] listeners = new TraceListener[] { new TextWriterTraceListener(Console.Out) };
            //Debug.Listeners.AddRange(listeners);


            IVMwareHorizonClient4 _client = (IVMwareHorizonClient4)new VMwareHorizonClient();

            IVMwareHorizonClientEvents5 ce = new ClientEvents5();


            WriteColor("This utility demo's how to logon and launch a horizon resource programatically.", ConsoleColor.Green, true);


            _client.Advise2(ce, VmwHorizonClientAdviseFlags.VmwHorizonClientAdvise_None);

            var server = GetValidInput("Enter connection server name: ");
            var username = GetValidInput("Enter Username: ");
            var password = GetValidInput("Enter Password: ", true);
            var domain = GetValidInput("Enter Domain: ");
            LoginInfo li = new LoginInfo
            {
                domainName = domain,
                password = password,
                username = username,
                loginAsCurrentUser = 0,
                samlArtifact = "",
                smartCardPIN = "224466",
                unauthenticatedAccessAccount = "",
                unauthenticatedAccessEnabled = 0
            };

            bool validChoice = false;
            _client.ConnectToServer(server, li, VmwHorizonClientLaunchFlags.VmwHorizonClientLaunch_NonInteractive);
            while (!hasFailed)
            {
                if (hasLaunched)
                {
                    hasFailed = false;
                    WriteColor("Session created successfully: " + LaunchID, ConsoleColor.Green, true);
                    break;
                }

                if (hasAuthenticated)
                {
                    if (hasLaunchItems)
                    {
                        var i= 0;
                        foreach(var item in Launchitems)
                        {
                            Console.WriteLine("{0} - " + item.name, i);
                            i += 1;
                        }
                       
                        
                        while (!validChoice)
                        {
                            WriteColor("enter a number of a resource to launch: ", ConsoleColor.Cyan, false);
                            var choice = GetValidInput("");
                            try
                            {
                                int parseresult = 0;
                                if (int.TryParse(choice, out parseresult))
                                {
                                    if (parseresult >= 0 && parseresult <= i)
                                    {
                                        validChoice = true;
                                        var launchitem = Launchitems[parseresult];
                                        _client.ConnectToLaunchItem2(Serverid, launchitem.id, VmwHorizonClientProtocol.VmwHorizonClientProtocol_Default, VmwHorizonClientDisplayType.VmwHorizonClientDisplayType_WindowSmall, "");                                      
                                        
                                    }
                                }
                                else
                                {
                                    validChoice = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                validChoice = false;
                            }
                            
                        }
                    }
                }
                System.Threading.Thread.Sleep(5000);
            }

            if (!hasFailed)
            {
                WriteColor("Press any key to log off the session", ConsoleColor.Cyan, false);
                Console.ReadLine();
                _client.LogoffProtocolSession(Serverid, launchtoken);
            }

            _client.ShutdownClient(Serverid, 1);
            _client.Unadvise();
            ce = null;
            _client = null;
        }

        public class LoginInfo : IVMwareHorizonClientAuthInfo2
        {
            public uint loginAsCurrentUser { get; set; }

            public string username { get; set; }

            public string domainName { get; set; }

            public string password { get; set; }

            public string smartCardPIN { get; set; }

            public string samlArtifact { get; set; }

            public uint unauthenticatedAccessEnabled { get; set; }

            public string unauthenticatedAccessAccount { get; set; }
        }
        public class ClientEvents5 : IVMwareHorizonClientEvents5
        {
           

            public class Helpers
            {
                [Flags]
                public enum SupportedProtocols
                {
                    VmwHorizonClientProtocol_Default = 0,
                    VmwHorizonClientProtocol_RDP = 1,
                    VmwHorizonClientProtocol_PCoIP = 2,
                    VmwHorizonClientProtocol_Blast = 4
                }
                [Flags]
                public enum LaunchItemType
                {
                    VmwHorizonLaunchItem_HorizonDesktop = 0,
                    VmwHorizonLaunchItem_HorizonApp = 1,
                    VmwHorizonLaunchItem_XenApp = 2,
                    VmwHorizonLaunchItem_SaaSApp = 3,
                    VmwHorizonLaunchItem_HorizonAppSession = 4,
                    VmwHorizonLaunchItem_DesktopShadowSession = 5,
                    VmwHorizonLaunchItem_AppShadowSession = 6
                }

                public class launchItem
                {
                    public string name { get; set; }

                    public string id { get; set; }

                    [JsonConverter(typeof(StringEnumConverter))]
                    public LaunchItemType type { get; set; }

                    [JsonConverter(typeof(StringEnumConverter))]
                    public SupportedProtocols supportedProtocols { get; set; }
                    [JsonConverter(typeof(StringEnumConverter))]
                    public VmwHorizonClientProtocol defaultProtocol { get; set; }
                    public launchItem(IVMwareHorizonClientLaunchItemInfo item)
                    {
                        name = item.name;
                        id = item.id;
                        type = (LaunchItemType)item.type;
                        supportedProtocols = (SupportedProtocols)item.supportedProtocols;
                        defaultProtocol = item.defaultProtocol;
                    }
                }
                public class LaunchItem2
                {
                    public string name { get; set; }

                    public string id { get; set; }

                    [JsonConverter(typeof(StringEnumConverter))]
                    public LaunchItemType type { get; set; }

                    [JsonConverter(typeof(StringEnumConverter))]
                    public SupportedProtocols supportedProtocols { get; set; }

                    [JsonConverter(typeof(StringEnumConverter))]
                    public VmwHorizonClientProtocol defaultProtocol { get; set; }

                    public uint hasRemotableAssets { get; set; }
                    public LaunchItem2(IVMwareHorizonClientLaunchItemInfo2 i)
                    {
                        name = i.name;
                        id = i.id;
                        type = (LaunchItemType)i.type;
                        supportedProtocols = (SupportedProtocols)i.supportedProtocols;
                        defaultProtocol = i.defaultProtocol;
                        hasRemotableAssets = i.hasRemotableAssets;
                    }
                }
            }
            public static string Serialise(object obj)
            {
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            public static string Serialise(object obj, object objType)
            {
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            public void OnStarted()
            {
                Debug.WriteLine("OnStarted():");
            }
            public void OnExit()
            {
                Debug.WriteLine("OnExit():");
            }

            public void OnConnecting(object serverInfo)
            {
                IVMwareHorizonClientServerInfo si = (IVMwareHorizonClientServerInfo)serverInfo;
                Debug.WriteLine("OnConnecting(): {0}", Serialise(si));
                Debug.WriteLine(si.serverType);
                Debug.WriteLine(si.serverId);
                Debug.WriteLine(si.serverAddress);
            }

            public void OnConnectFailed(uint serverId, string errorMessage)
            {
                Debug.WriteLine("OnConnectFailed(): {0} - {1}", serverId, errorMessage);
                hasFailed = true;
            }

            public void OnAuthenticationRequested(uint serverId, VmwHorizonClientAuthType authType)
            {
                Debug.WriteLine("OnAuthenticationRequested(): {0} - {1}", serverId, Serialise(authType.ToString()));
            }

            public void OnAuthenticating(uint serverId, VmwHorizonClientAuthType authType, string user)
            {
                Debug.WriteLine("OnAuthenticating(): {0} - {1} - {2}", serverId, Serialise(authType.ToString()), user);
            }

            public void OnAuthenticationDeclined(uint serverId, VmwHorizonClientAuthType authType)
            {
                Debug.WriteLine("OnAuthenticationDeclined(): {0} - {1}", serverId, Serialise(authType.ToString()));
                hasFailed = true;
            }

            public void OnAuthenticationFailed(uint serverId, VmwHorizonClientAuthType authType, string errorMessage, int retryAllowed)
            {
                Debug.WriteLine("OnAuthenticationFailed(): {0} - {1} - {2} - {3}", serverId, Serialise(authType.ToString()), errorMessage, retryAllowed);
                hasFailed = true;
            }

            public void OnLoggedIn(uint serverId)
            {
                Debug.WriteLine("OnLoggedIn(): {0}", serverId);
                hasAuthenticated = true;
                Serverid = serverId;

            }

            public void OnDisconnected(uint serverId)
            {
                Debug.WriteLine("OnDisconnected(): {0}", serverId);
                hasFailed = true;
            }

            public void OnReceivedLaunchItems(uint serverId, Array launchItems)
            {

                List<Helpers.launchItem> items = new List<Helpers.launchItem>();
                foreach (object item in launchItems)
                {
                    IVMwareHorizonClientLaunchItemInfo i = (IVMwareHorizonClientLaunchItemInfo)item;
                    items.Add(new launchItem(i));
                }
                //hasLaunchItems = true;
                Debug.WriteLine("OnReceivedLaunchItems(): {0} - {1}", serverId, Serialise(items));
            }

            public void OnLaunchingItem(uint serverId, VmwHorizonLaunchItemType type, string launchItemId, VmwHorizonClientProtocol protocol)
            {
                Debug.WriteLine("OnLaunchingItem(): {0} - {1} - {2} - {3}", serverId, type.ToString(), launchItemId, protocol.ToString());

            }

            public void OnItemLaunchSucceeded(uint serverId, VmwHorizonLaunchItemType type, string launchItemId)
            {
                Debug.WriteLine("OnItemLaunchSucceeded(): {0} - {1} - {2}", serverId, type.ToString(), launchItemId);
                hasLaunched = true;
                LaunchID = launchItemId;
            }

            public void OnItemLaunchFailed(uint serverId, VmwHorizonLaunchItemType type, string launchItemId, string errorMessage)
            {
                Debug.WriteLine("OnItemLaunchFailed(): {0} - {1} - {2} - {3}", serverId, type.ToString(), launchItemId, errorMessage);
                hasFailed = true;
            }

            public void OnNewProtocolSessionCreated(uint serverId, string sessionToken, VmwHorizonClientProtocol protocol, VmwHorizonClientSessionType type, string clientId)
            {
                Debug.WriteLine("OnNewProtocolSessionCreated(): {0} - {1} - {2} - {3} - {4}", serverId, sessionToken, protocol.ToString(), type.ToString(), clientId);
                launchtoken = sessionToken;
                hasLaunched = true;
            }

            public void OnProtocolSessionDisconnected(uint serverId, string sessionToken, uint connectionFailed, string errorMessage)
            {
                Debug.WriteLine("OnProtocolSessionDisconnected(): {0} - {1} - {2} - {3}", serverId, sessionToken, connectionFailed, errorMessage);
            }

            public void OnSeamlessWindowsModeChanged(uint serverId, string sessionToken, uint enabled)
            {
                Debug.WriteLine("OnSeamlessWindowsModeChanged(): {0} - {1} - {2}", serverId, sessionToken, enabled);

            }

            public void OnSeamlessWindowAdded(uint serverId, string sessionToken, string windowPath, string entitlementId, int windowId, long windowHandle, VmwHorizonClientSeamlessWindowType type)
            {
                Debug.WriteLine("OnSeamlessWindowAdded(): {0} - {1} - {2} - {3} - {4} - {5} - {6}", serverId, sessionToken, windowPath, entitlementId, windowId, windowHandle, type.ToString());

            }

            public void OnSeamlessWindowRemoved(uint serverId, string sessionToken, int windowId)
            {
                Debug.WriteLine("OnSeamlessWindowRemoved(): {0} - {1} - {2}", serverId, sessionToken, windowId);

            }

            public void OnUSBInitializeComplete(uint serverId, string sessionToken)
            {
                Debug.WriteLine("OnUSBInitializeComplete(): {0} - {1}", serverId, sessionToken);

            }

            public void OnConnectUSBDeviceComplete(uint serverId, string sessionToken, uint isConnected)
            {
                Debug.WriteLine("OnConnectUSBDeviceComplete(): {0} - {1} - {2}", serverId, sessionToken, isConnected);

            }

            public void OnUSBDeviceError(uint serverId, string sessionToken, string errorMessage)
            {
                Debug.WriteLine("OnUSBDeviceError(): {0} - {1} - {2}", serverId, sessionToken, errorMessage);
            }

            public void OnAddSharedFolderComplete(uint serverId, string fullPath, uint succeeded, string errorMessage)
            {
                Debug.WriteLine("OnAddSharedFolderComplete(): {0} - {1} - {2} - {3}", serverId, fullPath, succeeded, errorMessage);

            }

            public void OnRemoveSharedFolderComplete(uint serverId, string fullPath, uint succeeded, string errorMessage)
            {
                Debug.WriteLine("OnRemoveSharedFolderComplete(): {0} - {1} - {2} - {3}", serverId, fullPath, succeeded, errorMessage);

            }

            public void OnFolderCanBeShared(uint serverId, string sessionToken, uint canShare)
            {
                Debug.WriteLine("OnFolderCanBeShared(): {0} - {1} - {2}", serverId, sessionToken, canShare);
            }

            public void OnCDRForcedByAgent(uint serverId, string sessionToken, uint forcedByAgent)
            {
                Debug.WriteLine("OnCDRForcedByAgent(): {0} - {1} - {2}", serverId, sessionToken, forcedByAgent);
            }

            public void OnItemLaunchSucceeded2(uint serverId, VmwHorizonLaunchItemType type, string launchItemId, string sessionToken)
            {
                Debug.WriteLine("OnItemLaunchSuccedded2(): {0} - {1} - {2} - {3}", serverId, type.ToString(), launchItemId, sessionToken);
                hasLaunched = true;
            }


            public void OnReceivedLaunchItems2(uint serverId, Array launchItems)
            {
                List<LaunchItem2> items = new List<LaunchItem2>();
                //IVMwareHorizonClientLaunchItemInfo2 items = (IVMwareHorizonClientLaunchItemInfo2)launchItems;
                foreach (var item in launchItems)
                {
                    IVMwareHorizonClientLaunchItemInfo2 i = (IVMwareHorizonClientLaunchItemInfo2)item;
                    items.Add(new LaunchItem2(i));

                }
                Launchitems = items;
                hasLaunchItems = true;
                Debug.WriteLine("OnReceivedLaunchItems2(): {0} - {1}", serverId, Serialise(items));
            }
        }
    }
}
