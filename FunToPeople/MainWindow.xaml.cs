using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Enumeration;

namespace FunToPeople
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		FtpClient ftpClient = new FtpClient("localhost");
		string localPath;
		private void BindData()
		{
			StatusListBox.ItemsSource = CommonData.statusList;
			LocalFileListBox.ItemsSource = CommonData.localFileList;
			RemoteFileListBox.ItemsSource = CommonData.remoteFileList;
		}
		public MainWindow()
		{
			InitializeComponent();
			BindData();
			localPath = AppDomain.CurrentDomain.BaseDirectory;
			LocalPathTextBox.Text = localPath;
			ftpClient.Connect("yah01", "FTPSERVER01");
			ftpClient.FreshFileList();
			FreshLocalFileList();
		}

		private void LocalFileListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			string fileName = System.IO.Path.Join(localPath, LocalFileListBox.SelectedItem.ToString());
			if (Directory.Exists(fileName))
			{
				LocalPathTextBox.Text = localPath = fileName;
				FreshLocalFileList();
			}
			else
			{
				ftpClient.Upload(fileName);
				ftpClient.FreshFileList();
			}
		}

		private void Button_Goto_Click(object sender, RoutedEventArgs e)//转到按钮 改写访问目录的地址为TextBox_LocalPath中的输入
		{
			localPath = LocalPathTextBox.Text;
			FreshLocalFileList();
		}

		public void FreshLocalFileList()//刷新本地文件列表
		{
			CommonData.localFileList.Clear();
			if (localPath == "") return;
			CommonData.localFileList.Add("..");
			string[] Directorys;
			Directorys = Directory.GetDirectories(localPath, "*.*");
			foreach (string Directory in Directorys)//查询当前目录的文件夹
			{
				string[] temp = Regex.Split(Directory, @"\\");
				CommonData.localFileList.Add(temp[temp.Length - 1]);
			}

			string[] Files;
			Files = Directory.GetFiles(localPath, "*.*");
			foreach (string File in Files)//查询当前目录下的文件
			{
				string[] temp = Regex.Split(File, @"\\");
				CommonData.localFileList.Add(temp[temp.Length - 1]);
			}
		}

		private void RemoteFileListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			ftpClient.Download(localPath, RemoteFileListBox.SelectedItem.ToString());
			FreshLocalFileList();
		}
	}
}
