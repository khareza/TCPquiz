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
using System.Text.RegularExpressions;

namespace TCPquiz
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket serverSocket;
        private List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 100;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        Dictionary<string, string> PlayersAndPoints = new Dictionary<string, string>();

        int QuestionNumber = 0;
        private string CorrectAnswer = "";
        private List<int> RandomNumbers;
        private int AmountOfPlayers = 0;  
        

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void LoadAnswer()
        {
            if (RandomNumbers[QuestionNumber] != 0)
            {
                CorrectAnswer = File.ReadLines(@"quest.txt").Skip(RandomNumbers[QuestionNumber] * 5 + RandomNumbers[QuestionNumber] + 5).Take(1).First();
            }
            else
            {
                CorrectAnswer = File.ReadLines(@"quest.txt").Skip(RandomNumbers[QuestionNumber]).Take(1).First();
            }
            

        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
                StartServer.IsEnabled = false;
                StopServer.IsEnabled = true;
                SetupServer();
                var rnd = new Random();
                RandomNumbers = Enumerable.Range(0, 39).OrderBy(x => rnd.Next()).Take(20).ToList();//randomowe 20 liczb(pytan) z przedzialu 1-40

                //mozna tez tak
                //HashSet<int> numbers = new HashSet<int>();
                //while (numbers.Count < 20)
                //{
                //    numbers.Add(rnd.Next(0, 39));
                //}
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            CloseAllSockets();
            StartServer.IsEnabled = true;
            StopServer.IsEnabled = false;
        }

        private void SetupServer()
        {
            
            // Console.WriteLine("Setting up server...");

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress hostIPAddress1 = new IPAddress(new byte[] {10,10,60,163 }); //(Dns.GetHostEntry("10.10.60.163")).AddressList[0];
            IPAddr.Text = hostIPAddress1.ToString();
            serverSocket.Bind(new IPEndPoint(hostIPAddress1, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            Status.Foreground = Brushes.Green;
            Status.Content = "Server is active";
            //Status.Content = hostIPAddress1.ToString();
            //Console.WriteLine("Server setup complete");
        }

        private void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            serverSocket.Close();            
            Status.Foreground = Brushes.Red;
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
            catch (Exception)
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
            if (int.TryParse(text, out QuestionNumber))
            {
                string tempString = RandomNumbers[QuestionNumber].ToString();
                byte[] data = Encoding.ASCII.GetBytes(tempString);
                current.Send(data);
            }
            else if (text.ToLower().Equals(CorrectAnswer.ToLower()) && text.Length == 1)
            {
                //Console.WriteLine("Text is a get time request");
                byte[] data = Encoding.ASCII.GetBytes("Good");
                current.Send(data);

            }
            else if(!text.ToLower().Equals(CorrectAnswer.ToLower()) && text.Length == 1)
            {
                byte[] data = Encoding.ASCII.GetBytes("Bad");
                current.Send(data);
            }
            else
            {
                string[] tempString = text.Split(' ');
                if (tempString.Length==3)
                {                    
                    PlayersAndPoints.Add(tempString[0], tempString[1]);
                    AmountOfPlayers++;
                }
                if (text.ToLower().Equals("request"))
                {
                    if (AmountOfPlayers == clientSockets.Count)
                    {
                        var ordered = PlayersAndPoints.OrderByDescending(x => x.Value).ToList();
                        byte[] data = Encoding.ASCII.GetBytes(ordered[0].ToString());
                        current.Send(data);
                    }
                    else
                    {
                        byte[] data = Encoding.ASCII.GetBytes("error");
                        current.Send(data);
                    }
                }



            }

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }


    }
}
