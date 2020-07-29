using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using System.Linq;

namespace FunToPeople
{
    class TcpClient
    {
        private Socket sock;
        private byte[] buffer = new byte[1024];
        private int bufferElemNum = 0;

        private void flushBuffer(byte[] target, int offset, int n)
        {
            for (int i = 0; i < n; i++)
            {
                target[i + offset] = buffer[i];
                if (n + i < buffer.Length)
                    buffer[i] = buffer[n + i];
            }
            bufferElemNum -= n;
        }

        private string flushAllOrUntilEndSignal()
		{
            for (int i = 1; i < bufferElemNum; i++)
            {
                if (buffer[i] == '\n' && buffer[i - 1] == '\r')
                {
                    string message = ASCIIEncoding.ASCII.GetString(buffer.Take(i + 1).ToArray());
                    for (int j = 0; j < i; j++)
                    {
                        if (j + i + 1 < buffer.Length)
                        {
                            buffer[j] = buffer[i + j + 1];
                        }
                    }

                    bufferElemNum -= i + 1;

                    return message;
                }
            }

            string msg = ASCIIEncoding.ASCII.GetString(buffer.Take(bufferElemNum).ToArray());
            bufferElemNum = 0;
            return msg;
        }

		public TcpClient(string domainName, int port)
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IPAddress[] ip= Dns.GetHostAddresses(domainName);
                IPEndPoint point = new IPEndPoint(ip[1], port);
                sock.Connect(point);
            }
            catch (Exception e)
            {
                CommonData.statusList.Add(e.Message);
                return;
            }
        }

        ~TcpClient()
		{
            sock.Close();
		}

        public void Close()
		{
            sock.Close();
		}


        public int WriteLine(String message)
        {
            try
            {
                return sock.Send(Encoding.ASCII.GetBytes(message));
            }
            catch (Exception e)
            {
                return -1;
            }
        }
        public int Write(byte[] message)
        {
            try
            {
                return sock.Send(message);
            }
            catch (Exception e)
            {
                return -1;
            }
        }
        public int Read(byte[] message)
        {
            int num = Math.Min(message.Length, bufferElemNum);
            flushBuffer(message,0, num);

            if (num == message.Length)
                return num;

            try
            {
                while(num < message.Length)
				{
                    bufferElemNum = sock.Receive(buffer);
                    int flushNum = Math.Min(bufferElemNum, message.Length - num);

                    bool isOver = bufferElemNum != buffer.Length;
                    flushBuffer(message, num,flushNum );
                    num += flushNum;

                    if (isOver)
                        break;
                }

                return num;
            }
            catch (Exception e)
            {
                return -1;
            }
        }


        private string wrapString(string str)
		{
            str = str.TrimEnd();
            if (str.Length == 0)
                return null;
            return str;
		}
		public string ReadLine() 
        {
            string msg = flushAllOrUntilEndSignal();
            if (msg.EndsWith("\r\n"))
                return wrapString(msg);

			try
			{
                while(true)
				{
                    bufferElemNum = sock.Receive(buffer);
                    bool isOver = bufferElemNum != buffer.Length;
                    msg += flushAllOrUntilEndSignal();
                    if (msg.EndsWith("\r\n"))
                        return wrapString(msg);

                    if (isOver)
                        return wrapString(msg);
                }
			}
            catch(Exception e)
			{
                return null;
			}
        }
	}
}
