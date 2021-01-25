using System;
using System.Runtime.InteropServices;
using System.Text;


namespace PInvoke
{
    //private static extern bool LookupAccountSid(Nullable, ProcessListing->pUserSid, accountName, bufferLen, domainName, domainNameBufferLen, peUse);
    //private static extern bool WTSFreeMemoryEx(WTSTypeProcessInfoLevel1, originalPtr, processCount);
    class ProcessEnum
    {
        enum SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        [DllImport("wtsapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool WTSEnumerateProcesses(
            IntPtr serverHandle,        // Handle to a terminal server.
            Int32 reserved,             // Must be 0.
            Int32 version,              // Must be 1.
            ref IntPtr ppProcessInfo,   // Pointer to array of WTS_PROCESS_INFO.
            ref Int32 pCount            // Pointer to number of processes.
        );

        [DllImport("wtsapi32.dll", CharSet = CharSet.Unicode, SetLastError=true)]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        //[DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        //static extern bool ConvertSidToStringSid(IntPtr pSid, out string strSid);

        //private static extern bool ConvertSidToStringSid(processListing->pUserSid, &stringSID);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool ConvertSidToStringSid(
            IntPtr pSid,
            out IntPtr ptrSid
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool LookupAccountSid(
            string lpSystemName,
            //[MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
            IntPtr pSid,
            System.Text.StringBuilder lpName,
            ref uint cchName,
            System.Text.StringBuilder ReferencedDomainName,
            ref uint cchReferencedDomainName,
            out SID_NAME_USE peUse
        );

        public struct WTS_PROCESS_INFO
        {
            public int SessionID;
            public int ProcessID;
            public IntPtr ProcessName;  // Pointer to string.
            public IntPtr UserSid;
        }

        private static IntPtr WTS_CURRENT_SERVER_HANDLE = (IntPtr)null;

        public static WTS_PROCESS_INFO[] WTSEnumerateProcesses()
        {
            IntPtr pProcessInfo = IntPtr.Zero;
            int processCount = 0;

            // Enumerate processes.
            if (!WTSEnumerateProcesses(
                WTS_CURRENT_SERVER_HANDLE,
                0,
                1,
                ref pProcessInfo,
                ref processCount))
            {
                return null;
            }

            //Parse processes.
            IntPtr pMemory = pProcessInfo;
            WTS_PROCESS_INFO[] processInfos = new WTS_PROCESS_INFO[processCount];
            Console.WriteLine("PID\t\tProcess Name\t\tSession ID\tSID\tDOMAIN\\USER\n");
            for(int i = 0; i < processCount; i++)
            {
                processInfos[i] = (WTS_PROCESS_INFO)Marshal.PtrToStructure(pProcessInfo, typeof(WTS_PROCESS_INFO));
                pProcessInfo = (IntPtr)((long)pProcessInfo + Marshal.SizeOf(processInfos[i]));
                Console.Write("{0}\t\t{1}\t\t{2}\t ", processInfos[i].ProcessID, Marshal.PtrToStringAuto(processInfos[i].ProcessName), processInfos[i].SessionID);

                //Convert SID to StringSID.
                IntPtr ptrsSid = Marshal.AllocHGlobal(100);
                string sidString;
                if (!ConvertSidToStringSid(processInfos[i].UserSid, out ptrsSid))
                {
                    Console.Write("-\n");
                }
                else
                {
                    try
                    {
                        sidString = Marshal.PtrToStringAuto(ptrsSid);
                        Console.Write("{0}\t", sidString);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptrsSid);
                    }
                }

                StringBuilder userName = new StringBuilder();
                uint userSize = (uint)userName.Capacity;
                StringBuilder domainName = new StringBuilder();
                uint domainSize = (uint)domainName.Capacity;
                SID_NAME_USE sidUse;
                // Lookup Domain and Account name from StringSID.
                if (!LookupAccountSid(
                    null,
                    processInfos[i].UserSid,
                    userName,
                    ref userSize,
                    domainName,
                    ref domainSize,
                    out sidUse
                    ))
                {
                    Console.Write("-\\-\n")
                }
                else
                {
                    Console.Write("{0}\\{1}\n", domainName, userName);
                }
            }

            //Free memory.
            WTSFreeMemory(pMemory);
            //return processInfos;
            return processInfos;
        }

        static void Main(string[] args)
        {
            WTSEnumerateProcesses();
        }
    }
}
