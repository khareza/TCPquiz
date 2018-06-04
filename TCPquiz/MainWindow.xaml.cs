using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace TCPquiz
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 100;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        int QuestionNumber = 0;
        private string CorrectAnswer = "";


        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void LoadAnswer()
        {
            
            CorrectAnswer = File.ReadLines(@"c:\quest.txt").Skip(QuestionNumber+4).Take(1).First();

        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            SetupServer();
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            CloseAllSockets();
        }

        private void SetupServer()
        {
            Checker.Text += "Setting up server..." + "\r\n";
           // Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            Status.Content = "Server is active";
            //Console.WriteLine("Server setup complete");
            Checker.Text += "Server setup complete" + "\r\n";
        }
        private void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
            Status.Content = "Server is closed";
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            //Checker.Text += "Client connected, waiting for request..." + "\r\n";
            //Console.WriteLine("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                //Checker.Text += "Client forcefully disconnected" + "\r\n";
                //Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            //Checker.Text += "Received Text: " + text + "\r\n";
            //Console.WriteLine("Received Text: " + text);
            LoadAnswer();
            if(int.TryParse(text, out QuestionNumber))
            {
                //xdxdxdxd
            }
            else
            {
                if (text.ToLower().Equals(CorrectAnswer.ToLower())) // Client requested time
                {
                    //Checker.Text += "Text is a get time request" + "\r\n";
                    //Console.WriteLine("Text is a get time request");
                    byte[] data = Encoding.ASCII.GetBytes("Good");
                    current.Send(data);
                    // Checker.Text += "Time sent to client" + "\r\n";
                    //Console.WriteLine("Time sent to client");
                }
                else if (!text.ToLower().Equals(CorrectAnswer.ToLower()))
                {
                    byte[] data = Encoding.ASCII.GetBytes("Bad");
                    current.Send(data);
                }
                else if (text.ToLower() == "exit") // Client wants to exit gracefully
                {
                    // Always Shutdown before closing
                    current.Shutdown(SocketShutdown.Both);
                    current.Close();
                    clientSockets.Remove(current);
                    //Checker.Text += "Client disconnected" + "\r\n";
                    //Console.WriteLine("Client disconnected");
                    return;
                }
                else
                {
                    //Checker.Text += "Text is an invalid request" + "\r\n";
                    // Console.WriteLine("Text is an invalid request");
                    byte[] data = Encoding.ASCII.GetBytes("Invalid request");
                    current.Send(data);
                    //Checker.Text += "Warning Sent" + "\r\n";
                    //Console.WriteLine("Warning Sent");
                }
            }
           

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }


    }
}
