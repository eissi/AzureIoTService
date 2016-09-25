using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Diagnostics;

namespace IoTDevices
{
    class DevicePerformance
    {
        const string categoryName = "Processor Information";
        const string counterName = "% Processor Time";
        const string instanceName = "_Total";

        protected PerformanceCounter cpuPerformanceCounter = null;

        const string memCategoryName = "Memory";
        const string memCounterName = "% Committed Bytes In Use";
        const string memInstanceName = "";

        protected PerformanceCounter memPerformanceCounter = null;

        public DevicePerformance()
        {
            this.cpuPerformanceCounter = new PerformanceCounter(categoryName, counterName, instanceName);

            this.memPerformanceCounter = new PerformanceCounter(memCategoryName, memCounterName, memInstanceName);

            this.computername = Environment.MachineName;
        }

        //[DataMember]
        public string computername { get; set; }
        //[DataMember]
        public double cpuusage { get; set; }

        public double memusage {get; set;        }

        public void set_cpuvalue()
        {
            this.cpuusage = this.cpuPerformanceCounter.NextValue();
        }

        public void set_memvalue()
        {
            this.memusage = this.memPerformanceCounter.NextValue();
        }
    }
}
