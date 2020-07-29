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
			ftpClient.Connect("yah01", "FTPSERVER01");
			ftpClient.FreshFileList();
			FreshLocalFileList();
		}

		private void LocalFileListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//点击本地文件列表时
			//if (LocalFileListBox.SelectedIndex != -1 && LocalFileListBox.SelectedIndex < CommonData.LocalFolder && LocalFileListBox.IsMouseOver && e.ChangedButton == MouseButton.Left)
			//{
			//	string file = LocalFileListBox.SelectedItem as string;
			//	if (LocalFileListBox.SelectedIndex > 0)//如果是文件夹则改写当前目录地址
			//		CommonData.LocalPath += "\\" + file;
			//	else
			//	{   //列表的0项固定为..表示返回上一级目录
			//		string[] temp = Regex.Split(CommonData.LocalPath, @"\\");
			//		CommonData.LocalPath = "";
			//		for (int i = 0; i < temp.Length - 1; i++) CommonData.LocalPath += temp[i];
			//		//CommonData.LocalPath.Remove(CommonData.LocalPath.Length - 1- temp[temp.Length - 1].Length, temp[temp.Length - 1].Length);
			//		//CommonData.LocalPath += "\\";
			//	}
			//	TextBox_LocalPath.Text = CommonData.LocalPath;
			//	FreshLocalFileList();
			//}
		}

		private void Button_Goto_Click(object sender, RoutedEventArgs e)//转到按钮 改写访问目录的地址为TextBox_LocalPath中的输入
		{
			localPath = TextBox_LocalPath.Text;
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
	}
}
