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
using Microsoft.Win32;

namespace FunToPeople
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            FtpClient ftpClient = new FtpClient("localhost");
			string err = ftpClient.Connect("yah01", "FTPSERVER01");
            if (err != null)
            {
				CommonData.statusList.Add(err);
				return;
            }
            ftpClient.FreshFileList();

           
			
        }
    }
}
