using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.RightsManagement;
using System.ComponentModel;

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
		private NetworkStream cmdStreamWriter;
		private StreamReader cmdStreamReader;
		private NetworkStream dataStreamWriter;
		private StreamReader dataStreamReader;
		private String cmdData;
		private byte[] byteData;
		private const String EndSignal = "\r\n";
		private string lastResponse;

		public const int FtpCmdPort = 21;

		// 获取命令端口结果
		private string getResponse()
		{
			lastResponse = cmdStreamReader.ReadLine();
			CommonData.statusList.Add(lastResponse);
			return lastResponse;
		}

		// 发送命令
		private string sendCommand(FtpCommand cmd,string parameter)
		{
			cmdData = packCommand(cmd, parameter);
			byteData = Encoding.ASCII.GetBytes(cmdData.ToCharArray());
			cmdStreamWriter.Write(byteData, 0, byteData.Length);
			return getResponse();
		}

		private string packCommand(FtpCommand cmd,string parameter)
		{
			return cmd.ToString() + " " + parameter + EndSignal;
		}

		private FtpRespStatus getRespStatus()
		{
			return (FtpRespStatus)Convert.ToInt32(lastResponse.Substring(0, 3));
		}

		public string Connect(string hostname,string username,string password)
		{
			cmdClient = new TcpClient(hostname, FtpCmdPort);
			cmdStreamReader = new StreamReader(cmdClient.GetStream());
			cmdStreamWriter = cmdClient.GetStream();
			getResponse();

			sendCommand(FtpCommand.USER, username);
			sendCommand(FtpCommand.PASS, password);
			
			if(getRespStatus() == FtpRespStatus.NotLogin)
			{
				return "登录失败，请检查账户名与密码";
			}

			

			return null;
		}


		
	}
}
