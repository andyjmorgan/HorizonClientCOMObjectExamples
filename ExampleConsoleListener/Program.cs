using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMwareHorizonClientController;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;

using static ExampleHorizonClientCOMObjectConnector.ClientEvents5.Helpers;

namespace ExampleHorizonClientCOMObjectConnector
{
    class Program
    {
        static void writeColor(string message, ConsoleColor messageColor, ConsoleColor currentColor)
        {
            Console.ForegroundColor = messageColor;
            Console.WriteLine(message);
            Console.ForegroundColor = currentColor;
        }
        static void Main(string[] args)
        {
            TraceListener[] listeners = new TraceListener[] { new TextWriterTraceListener(Console.Out) };
            Debug.Listeners.AddRange(listeners);


            IVMwareHorizonClient4 _client = (IVMwareHorizonClient4)new VMwareHorizonClient();

            IVMwareHorizonClientEvents5 ce = (IVMwareHorizonClientEvents5)new ClientEvents5();


            writeColor("This utility demo's how to hook the horizon client and beginning to listen to events.", ConsoleColor.Green, Console.ForegroundColor);

            _client.Advise2(ce, VmwHorizonClientAdviseFlags.VmwHorizonClientAdvise_None);
            Console.Title = "Press any key to disconnect from events.";


            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Debug.Write("Would you like to open a client now? [y/n]: ");
            Console.ForegroundColor = currentColor;
            var answer = Console.ReadLine();
            if (answer.ToLower() == "y")
            {
                System.Diagnostics.Process.Start("vmware-view://");
            }



            Console.ReadLine();
            _client.Unadvise();
            writeColor("No longer listening for events.", ConsoleColor.Green, Console.ForegroundColor);
            ce = null;
            _client = null;
        }

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
        }

        public void OnConnectFailed(uint serverId, string errorMessage)
        {
            Debug.WriteLine("OnConnectFailed(): {0} - {1}", serverId, errorMessage);
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
        }

        public void OnAuthenticationFailed(uint serverId, VmwHorizonClientAuthType authType, string errorMessage, int retryAllowed)
        {
            Debug.WriteLine("OnAuthenticationFailed(): {0} - {1} - {2} - {3}", serverId, Serialise(authType.ToString()), errorMessage, retryAllowed);
        }

        public void OnLoggedIn(uint serverId)
        {
            Debug.WriteLine("OnLoggedIn(): {0}", serverId);

        }

        public void OnDisconnected(uint serverId)
        {
            Debug.WriteLine("OnDisconnected(): {0}", serverId);

        }

        public void OnReceivedLaunchItems(uint serverId, Array launchItems)
        {

            List<Helpers.launchItem> items = new List<Helpers.launchItem>();
            foreach (object item in launchItems)
            {
                IVMwareHorizonClientLaunchItemInfo i = (IVMwareHorizonClientLaunchItemInfo)item;
                items.Add(new launchItem(i));
            }

            Debug.WriteLine("OnReceivedLaunchItems(): {0} - {1}", serverId, Serialise(items));
        }

        public void OnLaunchingItem(uint serverId, VmwHorizonLaunchItemType type, string launchItemId, VmwHorizonClientProtocol protocol)
        {
            Debug.WriteLine("OnLaunchingItem(): {0} - {1} - {2} - {3}", serverId, type.ToString(), launchItemId, protocol.ToString());

        }

        public void OnItemLaunchSucceeded(uint serverId, VmwHorizonLaunchItemType type, string launchItemId)
        {
            Debug.WriteLine("OnItemLaunchSucceeded(): {0} - {1} - {2}", serverId, type.ToString(), launchItemId);

        }

        public void OnItemLaunchFailed(uint serverId, VmwHorizonLaunchItemType type, string launchItemId, string errorMessage)
        {
            Debug.WriteLine("OnItemLaunchFailed(): {0} - {1} - {2} - {3}", serverId, type.ToString(), launchItemId, errorMessage);

        }

        public void OnNewProtocolSessionCreated(uint serverId, string sessionToken, VmwHorizonClientProtocol protocol, VmwHorizonClientSessionType type, string clientId)
        {
            Debug.WriteLine("OnNewProtocolSessionCreated(): {0} - {1} - {2} - {3} - {4}", serverId, sessionToken, protocol.ToString(), type.ToString(), clientId);

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
            Debug.WriteLine("OnReceivedLaunchItems2(): {0} - {1}", serverId, Serialise(items));
        }
    }

}
