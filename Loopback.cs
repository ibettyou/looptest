using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Loopback
{
    public class LoopUtil
    {
        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;
            public uint Attributes;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct INET_FIREWALL_AC_CAPABILITIES
        {
            public uint count;
            public IntPtr capabilities;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct INET_FIREWALL_AC_BINARIES
        {
            public uint count;
            public IntPtr binaries;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct INET_FIREWALL_APP_CONTAINER
        {
            internal IntPtr appContainerSid;
            internal IntPtr userSid;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string appContainerName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string displayName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string description;
            internal INET_FIREWALL_AC_CAPABILITIES capabilities;
            internal INET_FIREWALL_AC_BINARIES binaries;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string workingDirectory;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string packageFullName;
        }

        [DllImport("FirewallAPI.dll")]
        internal static extern void NetworkIsolationFreeAppContainers(IntPtr pACs);

        [DllImport("FirewallAPI.dll")]
        internal static extern uint NetworkIsolationGetAppContainerConfig(out uint pdwCntACs, out IntPtr appContainerSids);

        [DllImport("FirewallAPI.dll")]
        private static extern uint NetworkIsolationSetAppContainerConfig(uint pdwCntACs, SID_AND_ATTRIBUTES[] appContainerSids);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool ConvertStringSidToSid(string strSid, out IntPtr pSid);

        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool ConvertSidToStringSid(IntPtr pSid, out string strSid);

        [DllImport("FirewallAPI.dll")]
        internal static extern uint NetworkIsolationEnumAppContainers(uint Flags, out uint pdwCntPublicACs, out IntPtr ppACs);

        enum NETISO_FLAG
        {
            NETISO_FLAG_FORCE_COMPUTE_BINARIES = 0x1,
            NETISO_FLAG_MAX = 0x2
        }

        public class AppContainer
        {
            public string appContainerName { get; set; }
            public string displayName { get; set; }
            public string workingDirectory { get; set; }
            public string StringSid { get; set; }
            public bool LoopUtil { get; set; }

            public AppContainer(string appContainerName, string displayName, string workingDirectory, IntPtr sid)
            {
                this.appContainerName = appContainerName;
                this.displayName = ProcessDisplayName(displayName);
                this.workingDirectory = workingDirectory;
                ConvertSidToStringSid(sid, out this.StringSid);
            }

            private static string ProcessDisplayName(string displayName)
            {
                if (string.IsNullOrEmpty(displayName))
                    return displayName;

                int index = displayName.IndexOf('?');
                return index > 0 ? displayName.Substring(0, index) : displayName;
            }
        }

        private List<INET_FIREWALL_APP_CONTAINER> _appList;
        private List<SID_AND_ATTRIBUTES> _appListConfig;
        public List<AppContainer> Apps { get; } = new List<AppContainer>();
        private IntPtr _pACs;

        public LoopUtil()
        {
            LoadApps();
        }

        public void LoadApps()
        {
            Apps.Clear();
            _pACs = IntPtr.Zero;

            _appList = EnumerateAppContainers();
            _appListConfig = GetAppContainerConfig();

            foreach (var piApp in _appList)
            {
                var app = new AppContainer(piApp.appContainerName, piApp.displayName, piApp.workingDirectory, piApp.appContainerSid);
                app.LoopUtil = IsLoopbackEnabled(piApp.appContainerSid);
                Apps.Add(app);
            }
        }

        private bool IsLoopbackEnabled(IntPtr sid)
        {
            foreach (var item in _appListConfig)
            {
                ConvertSidToStringSid(item.Sid, out string configSid);
                ConvertSidToStringSid(sid, out string appSid);
                if (configSid == appSid)
                    return true;
            }
            return false;
        }

        private static List<SID_AND_ATTRIBUTES> GetCapabilities(INET_FIREWALL_AC_CAPABILITIES cap)
        {
            var list = new List<SID_AND_ATTRIBUTES>();
            var arrayValue = cap.capabilities;
            int structSize = Marshal.SizeOf(typeof(SID_AND_ATTRIBUTES));

            for (int i = 0; i < cap.count; i++)
            {
                list.Add((SID_AND_ATTRIBUTES)Marshal.PtrToStructure(arrayValue, typeof(SID_AND_ATTRIBUTES)));
                arrayValue = new IntPtr((long)arrayValue + structSize);
            }
            return list;
        }

        private List<SID_AND_ATTRIBUTES> GetAppContainerConfig()
        {
            var list = new List<SID_AND_ATTRIBUTES>();
            IntPtr arrayValue = IntPtr.Zero;
            uint size = 0;

            uint retval = NetworkIsolationGetAppContainerConfig(out size, out arrayValue);
            if (retval == 0 && size > 0)
            {
                int structSize = Marshal.SizeOf(typeof(SID_AND_ATTRIBUTES));
                for (int i = 0; i < size; i++)
                {
                    list.Add((SID_AND_ATTRIBUTES)Marshal.PtrToStructure(arrayValue, typeof(SID_AND_ATTRIBUTES)));
                    arrayValue = new IntPtr((long)arrayValue + structSize);
                }
            }
            return list;
        }

        private List<INET_FIREWALL_APP_CONTAINER> EnumerateAppContainers()
        {
            var list = new List<INET_FIREWALL_APP_CONTAINER>();
            IntPtr arrayValue = IntPtr.Zero;
            uint size = 0;

            uint retval = NetworkIsolationEnumAppContainers((int)NETISO_FLAG.NETISO_FLAG_MAX, out size, out arrayValue);
            if (retval == 0 && size > 0)
            {
                _pACs = arrayValue;
                int structSize = Marshal.SizeOf(typeof(INET_FIREWALL_APP_CONTAINER));
                for (int i = 0; i < size; i++)
                {
                    list.Add((INET_FIREWALL_APP_CONTAINER)Marshal.PtrToStructure(arrayValue, typeof(INET_FIREWALL_APP_CONTAINER)));
                    arrayValue = new IntPtr((long)arrayValue + structSize);
                }
            }
            return list;
        }

        public bool SaveLoopbackState()
        {
            int enabledCount = Apps.Count(a => a.LoopUtil);
            if (enabledCount == 0)
                return true;

            var sids = new SID_AND_ATTRIBUTES[enabledCount];
            int index = 0;

            foreach (var app in Apps.Where(a => a.LoopUtil))
            {
                sids[index].Attributes = 0;
                ConvertStringSidToSid(app.StringSid, out sids[index].Sid);
                index++;
            }

            return NetworkIsolationSetAppContainerConfig((uint)enabledCount, sids) == 0;
        }

        public void FreeResources()
        {
            if (_pACs != IntPtr.Zero)
            {
                NetworkIsolationFreeAppContainers(_pACs);
                _pACs = IntPtr.Zero;
            }
        }
    }
}
