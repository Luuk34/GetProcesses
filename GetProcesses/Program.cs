using System.Management;
using System.Linq;

namespace GetProcesses
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Geef een processnaam op");
                Environment.Exit(-1);
            }
            var tmp = GetProcesses(args[0]);
            foreach (var item in tmp)
            {
                Console.WriteLine($"" +
                    $"Parent        : {item.ParentProcessId}\n" +
                    $"ProcessId     : {item.ProcessId} \n" +
                    $"Name          : {item.Name} \n" +
                    $"Commandline   : {item.CommandLine} \n" +
                    $"ExecutablePath: {item.ExecutablePath} \n" );
            }
        }

        static uint GetParentProcessId(uint myId)
        {
            uint returnId = 1;
            var query = string.Format("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {0}", myId);
            var search = new ManagementObjectSearcher("root\\CIMV2", query);
            var results = search.Get().GetEnumerator();
            results.MoveNext();
            if (results is not null)
            {
                var queryObj = results.Current;
                returnId = (uint)queryObj["ParentProcessId"];
            }
            return returnId;
        }

        static List<MyProcess> GetProcesses(string processname)
        {
            var query = "SELECT * FROM Win32_Process WHERE Name = '" + processname.Trim() + "'";
            var search = new ManagementObjectSearcher("root\\CIMV2", query).Get();

            var tmpList = (from mo in search.OfType<ManagementObject>()
                            select new
                            {
                                ProcessID = Convert.ToInt32(mo.Properties["ProcessId"].Value),
                                ParentProcessID = Convert.ToInt32(mo.Properties["ParentProcessId"].Value),
                                Name = Convert.ToString(mo.Properties["Name"].Value),
                                CommandLine = Convert.ToString(mo.Properties["CommandLine"].Value),
                                ExecutablePath = Convert.ToString(mo.Properties["ExecutablePath"].Value)
                            }
                   ).ToList();
            var myProcesses = (from t in tmpList
                               //where t.Name.Contains(processname, StringComparison.CurrentCultureIgnoreCase)
                               select new MyProcess
                               {
                                   ProcessId = t.ProcessID,
                                   ParentProcessId = t.ParentProcessID,
                                   Name = t.Name ?? "",
                                   CommandLine = t.CommandLine,
                                   ExecutablePath = t.ExecutablePath
                               }).ToList<MyProcess>();

            return myProcesses;
        }

        public class MyProcess
        {
            public int ProcessId;
            public int ParentProcessId;
            public required string Name; 
            public required string CommandLine;
            public required string ExecutablePath;
        }

        //  https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-process
        //
        //[Dynamic, Provider("CIMWin32"), SupportsCreate, CreateBy("Create"), SupportsDelete, DeleteBy("DeleteInstance"), UUID("{8502C4DC-5FBB-11D2-AAC1-006008C78BC7}"), DisplayName("Processes"), AMENDMENT]
        //class Win32_Process : CIM_Process
        //{
        //    string CreationClassName;
        //    string Caption;
        //    string CommandLine;
        //    datetime CreationDate;
        //    string CSCreationClassName;
        //    string CSName;
        //    string Description;
        //    string ExecutablePath;
        //    uint16 ExecutionState;
        //    string Handle;
        //    uint32 HandleCount;
        //    datetime InstallDate;
        //    uint64 KernelModeTime;
        //    uint32 MaximumWorkingSetSize;
        //    uint32 MinimumWorkingSetSize;
        //    string Name;
        //    string OSCreationClassName;
        //    string OSName;
        //    uint64 OtherOperationCount;
        //    uint64 OtherTransferCount;
        //    uint32 PageFaults;
        //    uint32 PageFileUsage;
        //    uint32 ParentProcessId;
        //    uint32 PeakPageFileUsage;
        //    uint64 PeakVirtualSize;
        //    uint32 PeakWorkingSetSize;
        //    uint32 Priority;
        //    uint64 PrivatePageCount;
        //    uint32 ProcessId;
        //    uint32 QuotaNonPagedPoolUsage;
        //    uint32 QuotaPagedPoolUsage;
        //    uint32 QuotaPeakNonPagedPoolUsage;
        //    uint32 QuotaPeakPagedPoolUsage;
        //    uint64 ReadOperationCount;
        //    uint64 ReadTransferCount;
        //    uint32 SessionId;
        //    string Status;
        //    datetime TerminationDate;
        //    uint32 ThreadCount;
        //    uint64 UserModeTime;
        //    uint64 VirtualSize;
        //    string WindowsVersion;
        //    uint64 WorkingSetSize;
        //    uint64 WriteOperationCount;
        //    uint64 WriteTransferCount;
        //};

    }
}
