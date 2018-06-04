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

namespace TCPquiz_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int counter = 0;
        private int QuestionNumber = 0;

        private static readonly Socket ClientSocket = new Socket
      (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int PORT = 100;
        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
            LoadQuestion();
            
           // RequestLoop();
            //Exit();
        }

        private void LoadQuestion()
        {
            Question.Text = "";
            for (int i = 0; i < 5; i++)
            {               
                Question.Text += File.ReadLines(@"c:\quest.txt").Skip(counter).Take(1).First() + "\r\n";
                counter++;
            }
            counter++;
            QuestionNumber++;
        }

        private void ConnectToServer()
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Question.Text+= "Connection attempt " + attempts + "\r\n";
                    //Console.WriteLine("Connection attempt " + attempts);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    ClientSocket.Connect(IPAddress.Loopback, PORT);
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }

            //Console.Clear();
            Question.Text = "";
            Question.Text += "Connected" + "\r\n";
            //Console.WriteLine("Connected");
        }

        private void RequestLoop()
        {
            Question.Text += @"<Type ""exit"" to properly disconnect client>" + "\r\n";
            //Console.WriteLine(@"<Type ""exit"" to properly disconnect client>");

            while (true)
            {
                //SendRequest();
                ReceiveResponse();
            }
        }

        /// <summary>
        /// Close socket and exit program.
        /// </summary>
        private void Exit()
        {
            SendString("exit"); // Tell the server we are exiting
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }

        private void SendRequest()
        {
            Question.Text += "Send a request: " + "\r\n";
            //Console.Write("Send a request: ");
            string request = "get date";
            SendString(request);

            if (request.ToLower() == "exit")
            {
                Exit();
            }
        }

        /// <summary>
        /// Sends a string to the server with ASCII encoding.
        /// </summary>
        private void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            CheckerClient.Text = text;
            //Console.WriteLine(text);
        }

        private void AnswA_Click(object sender, RoutedEventArgs e)
        {
            SendString(QuestionNumber.ToString());
            SendString("A");
            ReceiveResponse();
            LoadQuestion();

        }

        private void AnswB_Click(object sender, RoutedEventArgs e)
        {
            SendString(QuestionNumber.ToString());
            SendString("B");
            ReceiveResponse();
            LoadQuestion();
        }

        private void AnswC_Click(object sender, RoutedEventArgs e)
        {
            SendString(QuestionNumber.ToString());
            SendString("C");
            ReceiveResponse();
            LoadQuestion();
        }

        private void AnswD_Click(object sender, RoutedEventArgs e)
        {
            SendString(QuestionNumber.ToString());
            SendString("D");
            ReceiveResponse();
            LoadQuestion();
        }
    }
}
