using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Classes
{
	public class ProcessInfo
	{
		private string name;
		private double cpu;
		private double mem;
		public ProcessInfo(string name, double cpu, double mem)
		{
			this.name = name;
			this.cpu = cpu;
			this.mem = mem;
		}
	}
}
