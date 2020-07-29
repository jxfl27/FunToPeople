using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Collections.ObjectModel;

namespace FunToPeople
{
	class CommonData
	{
		public static ObservableCollection<string> statusList = new ObservableCollection<string>();
		public static ObservableCollection<string> localFileList = new ObservableCollection<string>();
		public static ObservableCollection<string> remoteFileList = new ObservableCollection<string>();
		public static void AddStatus(string status)
		{
			statusList.Add(status);
		}
	}
}
