using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.RightsManagement;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace FunToPeople
{
    enum FtpCommand
    {
        USER,
        PASS,
        QUIT,
        PASV,
        ABOR,
        LIST,
        STOR,
        RETR,
        CWD,
    }

    enum FtpRespStatus : int
    {
        [Description("not login")]
        NotLogin = 530,
    }
    class FtpClient
    {
        private TcpClient cmdClient;
        private TcpClient dataClient;
        private NetworkStream cmdByteStream;
        private StreamReader cmdStream;
        private NetworkStream dataByteStream;
        private StreamReader dataStream;
        private String cmdData;
        private byte[] byteData;
        private const String EndSignal = "\r\n";
        private string lastResponse;

        public string Hostname;
        public const int FtpCmdPort = 21;

        // 获取命令端口结果
        private string getResponse()
        {
            lastResponse = cmdStream.ReadLine();
            CommonData.statusList.Add(lastResponse);
            return lastResponse;
        }

        // 发送命令
        private string sendCommand(FtpCommand cmd, string parameter = null)
        {
            cmdData = packCommand(cmd, parameter);
            byteData = Encoding.ASCII.GetBytes(cmdData.ToCharArray());
            cmdByteStream.Write(byteData, 0, byteData.Length);
            return getResponse();
        }

        // 打包命令
        private string packCommand(FtpCommand cmd, string parameter = null)
        {
            if (parameter != null)
                return cmd.ToString() + " " + parameter + EndSignal;
            return cmd.ToString() + EndSignal;
        }

        // 获取最近一条命令的状态码
        private FtpRespStatus getRespStatus()
        {
            return (FtpRespStatus)Convert.ToInt32(lastResponse.Substring(0, 3));
        }

        private void turnToPasvMode()
        {
            sendCommand(FtpCommand.PASV);

            // 计算数据端口号
            string[] resps = lastResponse.Split(',');
            int dataPort = Convert.ToInt32(resps[4]) * 256 +
                Convert.ToInt32(Regex.Match(resps[5], "[0-9]*").ToString());

            dataClient = new TcpClient(Hostname, dataPort);
            dataStream = new StreamReader(dataClient.GetStream());
            dataByteStream = dataClient.GetStream();
        }

        private void closeDataPort()
        {
            dataStream.Close();
            dataByteStream.Close();
            getResponse();

            sendCommand(FtpCommand.ABOR);
        }

        public FtpClient(string hostname)
        {
            this.Hostname = hostname;
        }

        // 刷新文件列表
        public void FreshFileList()
        {
            turnToPasvMode();

            sendCommand(FtpCommand.LIST);
            CommonData.remoteFileList.Clear();

            string filepath;
            bool sawFirstFile = false;
            while ((filepath = dataStream.ReadLine()) != null)
            {
                if (sawFirstFile)
                    CommonData.remoteFileList.Add(filepath.Split(' ').Last());
                else
                    sawFirstFile = true;
            }

            closeDataPort();
        }

        public string Connect(string username, string password, string hostname = null)
        {
            if (this.Hostname == null && hostname == null)
            {
                return "请输入服务器ip";
            }

            if (hostname != null)
                this.Hostname = hostname;
            else
                hostname = this.Hostname;

            try
            {
                cmdClient = new TcpClient(hostname, FtpCmdPort);
                cmdStream = new StreamReader(cmdClient.GetStream());
                cmdByteStream = cmdClient.GetStream();
                getResponse();

                sendCommand(FtpCommand.USER, username);
                sendCommand(FtpCommand.PASS, password);

                if (getRespStatus() == FtpRespStatus.NotLogin)
                {
                    return "登录失败，请检查账户名与密码";
                }
            }
			catch(Exception err)
			{
				return err.ToString();
			}


            return null;
        }

        public void Close()
        {
            sendCommand(FtpCommand.QUIT);
            cmdStream.Close();
            cmdByteStream.Close();
        }

        public void Download(string localPath,string remoteFileName)
        {
			turnToPasvMode();


			sendCommand(FtpCommand.RETR,remoteFileName);

			FileStream fileStream = new FileStream(localPath,FileMode.OpenOrCreate);
			byte[] dataBytes = new byte[1024];
			int byteCnt = 0;

			while((byteCnt = dataByteStream.Read(dataBytes,0,1024))>0)
			{
				fileStream.Write(dataBytes,0,byteCnt);
			}
			
			fileStream.Close();

			closeDataPort();
        }

        public void Upload(string localFileName)
        {
			turnToPasvMode();

			sendCommand(FtpCommand.STOR,localFileName);

			FileStream fileStream = new FileStream(localFileName,FileMode.OpenOrCreate);
			byte[] dataBytes = new byte[1024];
			int byteCnt=0;
			while((byteCnt = fileStream.Read(dataBytes,0,byteCnt)) >0)
			{
				dataByteStream.Write(dataBytes,0,byteCnt);
			}

			fileStream.Close();

			closeDataPort();
        }
    }
}