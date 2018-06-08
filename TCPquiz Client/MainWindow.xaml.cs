using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        private int ClientQuestionNumber = 0;
        private int QuestionNumber = 0;
        private int PointsCounter = 0;
        string[] tempStrings;
        public string Name = "";
        BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private static readonly Socket ClientSocket = new Socket
        (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int PORT = 100;
        public MainWindow()
        {
            InitializeComponent();
            if (ConnectToServer())
            {
                SendString(ClientQuestionNumber.ToString());
                ReceiveResponse();
                LoadQuestion();
                backgroundWorker1.DoWork += backgroundWorker1_DoWork;
                backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
                backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
                backgroundWorker1.WorkerReportsProgress = true;
            }

           
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string fsdfsdf = null;
            tempStrings = new string[1];
            while (tempStrings.Length != 2)
            {
                SendString("request");
                fsdfsdf = ReceiveResponse();
                tempStrings = fsdfsdf.Split(',');
                Thread.Sleep(500);
                
            }
            

        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Question.Text = "The winner is: " + tempStrings[0] + " with points: " + tempStrings[1] + "\r\n";
        }

        private void LoadQuestion()
        {
            if (QuestionNumber!=0)
            {
                Question.Text = File.ReadLines(@"quest.txt", Encoding.GetEncoding("Windows-1250")).Skip(QuestionNumber * 5 + QuestionNumber).Take(1).First() + "\r\n";
                AnswA.Content = File.ReadLines(@"quest.txt", Encoding.GetEncoding("Windows-1250")).Skip(QuestionNumber * 5 + QuestionNumber + 1).Take(1).First() + "\r\n";
                AnswB.Content = File.ReadLines(@"quest.txt", Encoding.GetEncoding("Windows-1250")).Skip(QuestionNumber * 5 + QuestionNumber + 2).Take(1).First() + "\r\n";
                AnswC.Content = File.ReadLines(@"quest.txt", Encoding.GetEncoding("Windows-1250")).Skip(QuestionNumber * 5 + QuestionNumber + 3).Take(1).First() + "\r\n";
                AnswD.Content = File.ReadLines(@"quest.txt", Encoding.GetEncoding("Windows-1250")).Skip(QuestionNumber * 5 + QuestionNumber + 4).Take(1).First() + "\r\n";
            }                                    
            else                                 
            {                                    
                Question.Text = File.ReadLines(@"quest.txt", Encoding.GetEncoding("Windows-1250")).Skip(QuestionNumber).Take(1).First() + "\r\n";
                AnswA.Content = File.ReadLines(@"quest.txt", Encoding.GetEncoding("Windows-1250")).Skip(QuestionNumber + 1).Take(1).First() + "\r\n";
                AnswB.Content = File.ReadLines(@"quest.txt", Encoding.GetEncoding("Windows-1250")).Skip(QuestionNumber + 2).Take(1).First() + "\r\n";
                AnswC.Content = File.ReadLines(@"quest.txt", Encoding.GetEncoding("Windows-1250")).Skip(QuestionNumber + 3).Take(1).First() + "\r\n";
                AnswD.Content = File.ReadLines(@"quest.txt", Encoding.GetEncoding("Windows-1250")).Skip(QuestionNumber + 4).Take(1).First() + "\r\n";
            }
            ClientQuestionNumber++;
        }

        private bool ConnectToServer()
        {
            int attempts = 0;
            bool IsConnected = true;

                try
                {
                    attempts++;
                    Question.Text+= "Connection attempt " + attempts + "\r\n";
                //Console.WriteLine("Connection attempt " + attempts);
                // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                IPAddress hostIPAddress1 = new IPAddress(new byte[] { 10, 10, 60, 163 });// (Dns.GetHostEntry("10.10.60.163")).AddressList[0];
                ClientSocket.Connect(hostIPAddress1, PORT);
                }
                catch (SocketException)
                {
                    MessageBox.Show("Server is closed");
                    IsConnected = false;
                    LoginWindow LoginWindow = new LoginWindow();
                    this.Close();
                    LoginWindow.Show();
                }


            return IsConnected;

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

        private string ReceiveResponse()
        {
            string result = "";
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0)  return result;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            int.TryParse(text, out QuestionNumber);
            CheckScore(text);
            if (ClientQuestionNumber == 20 && !string.IsNullOrEmpty(text) )
            {

                string tempString = text.Trim(new Char[] { '[', ']' });

                result = tempString;
            }
            return result;
            //Console.WriteLine(text);
        }

        private void CheckStop()
        {
            if (ClientQuestionNumber == 20)
            {
                AnswA.IsEnabled = false;
                AnswB.IsEnabled = false;
                AnswC.IsEnabled = false;
                AnswD.IsEnabled = false;
                AnswA.Content = "Wait";
                AnswB.Content = "Wait";
                AnswC.Content = "Wait";
                AnswD.Content = "Wait";

                Question.Text = "Waiting for other players...";
                SendString(Name + " " + PointsCounter + " 20");

                backgroundWorker1.RunWorkerAsync();

            }
        }

        private void CheckScore(string Response)
        {
            if (Response.Equals("Good"))
            {
                PointsCounter++;
                PointCounter.Content = PointsCounter + "/20";
                CheckerClient.Foreground = Brushes.Green;
                CheckerClient.Text = "Good";
            }
            else if(Response.Equals("Bad"))
            {
                CheckerClient.Foreground = Brushes.Red;
                CheckerClient.Text = "Bad";
            }
            
        }

        //pozniej przeniose te przyciski do jednej metody
        private void AnswA_Click(object sender, RoutedEventArgs e)
        {

            SendString("A");
            ReceiveResponse();
            SendString(ClientQuestionNumber.ToString());
            ReceiveResponse();
            LoadQuestion();
            NumberOfQuestion.Content = ClientQuestionNumber + "/20";
            CheckStop();
        }

        private void AnswB_Click(object sender, RoutedEventArgs e)
        {

            SendString("B");
            ReceiveResponse();
            SendString(ClientQuestionNumber.ToString());
            ReceiveResponse();
            LoadQuestion();
            NumberOfQuestion.Content = ClientQuestionNumber + "/20";
            CheckStop();
        }

        private void AnswC_Click(object sender, RoutedEventArgs e)
        {

            SendString("C");
            ReceiveResponse();
            SendString(ClientQuestionNumber.ToString());
            ReceiveResponse();
            LoadQuestion();
            NumberOfQuestion.Content = ClientQuestionNumber + "/20";
            CheckStop();
        }

        private void AnswD_Click(object sender, RoutedEventArgs e)
        {
            SendString("D");
            ReceiveResponse();
            SendString(ClientQuestionNumber.ToString());
            ReceiveResponse();
            LoadQuestion();
            NumberOfQuestion.Content = ClientQuestionNumber + "/20";
            CheckStop();
        }


    }
}
