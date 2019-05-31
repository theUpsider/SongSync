using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace SongSync
{

    public partial class MainWindow : Window
    {
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private TcpClient client;
        public StreamReader STR;
        public StreamWriter STW;
        public string recieve;
        public String TextToSend;

        private static BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private static BackgroundWorker backgroundWorker2 = new BackgroundWorker();

        public MainWindow()
        {

            InitializeComponent();
            //songmanager
            SongManager.songfolder = textBoxSongFolder.Text;
            SongManager.songfolder.Replace(@"\\", @"\");
            SongManager.currentsong = textBoxFirstSong.Text;
            SongManager.currentsong.Replace(@"\\", @"\");
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker2.DoWork += new DoWorkEventHandler(backgroundWorker2_DoWork);

            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress address in localIP)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    textBoxIPServer.Text = address.ToString();
                    textBoxIPClient.Text = address.ToString();
                }
            }
            //StartClient();
            //LoopConnect();
            //SendLoop();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                try
                {
                    recieve = STR.ReadLine();
                    this.chatBox.Dispatcher.Invoke(new Action(delegate ()
                    {
                        chatBox.AppendText("Other:" + recieve + "\n");
                        if (recieve.Equals("play"))
                        {
                            SongManager.setfolderandsong();
                        }
                        if (recieve.Equals("stop"))
                        {
                            SongManager.Stop();
                        }
                        if (recieve.Equals("pause"))
                        {
                            SongManager.Pause();
                        }
                        if (recieve.Equals("next"))
                        {
                            SongManager.Next();
                        }
                        if (recieve.Equals("back"))
                        {
                            SongManager.Back();
                        }
                    }));
                
                    recieve = "";

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (client.Connected)
            {
                STW.WriteLine(TextToSend);
                this.chatBox.Dispatcher.Invoke(new Action(delegate ()
                {
                    chatBox.AppendText("Me:" + TextToSend + "\n");
                    if (TextToSend.Equals("play"))
                    {
                        SongManager.setfolderandsong();
                    }
                    if (TextToSend.Equals("stop"))
                    {
                        SongManager.Stop();
                    }
                    if (TextToSend.Equals("pause"))
                    {
                        SongManager.Pause();
                    }
                    if (TextToSend.Equals("next"))
                    {
                        SongManager.Next();
                    }
                    if (TextToSend.Equals("back"))
                    {
                        SongManager.Back();
                    }
                }));
            }
            else
            {
                MessageBox.Show("Sending failed");
            }
            backgroundWorker2.CancelAsync();
        }

   


        private void ButtonServerStart_Click(object sender, RoutedEventArgs e)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(textBoxPortServer.Text));
            listener.Start();
            client = listener.AcceptTcpClient();
            STR = new StreamReader(client.GetStream());
            STW = new StreamWriter(client.GetStream());
            STW.AutoFlush = true;

            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.WorkerSupportsCancellation = true;
        }

        private void ButtonClientConnect_Click(object sender, RoutedEventArgs e)
        {
            client = new TcpClient();
            IPEndPoint IpEnd = new IPEndPoint(IPAddress.Parse(textBoxIPClient.Text), int.Parse(textBoxPortClient.Text));

            try
            {
                client.Connect(IpEnd);

                if (client.Connected)
                {
                    chatBox.AppendText("Connected to server" + "\n");
                    STW = new StreamWriter(client.GetStream());
                    STR = new StreamReader(client.GetStream());
                    STW.AutoFlush = true;
                    backgroundWorker1.RunWorkerAsync();
                    backgroundWorker2.WorkerSupportsCancellation = true;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }



        private void TextBoxSongFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            SongManager.songfolder = textBoxSongFolder.Text;
            SongManager.songfolder.Replace(@"\\", @"\");
            //SongManager.setfolderandsong();
        }

        private void TextBoxFirstSong_TextChanged(object sender, TextChangedEventArgs e)
        {
            SongManager.currentsong = textBoxFirstSong.Text.Replace(@"\\",@"\");
            SongManager.currentsong.Replace(@"\\", @"\");
            //SongManager.setfolderandsong();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            TextToSend = "pause";
            backgroundWorker2.RunWorkerAsync();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            TextToSend = "stop";
            backgroundWorker2.RunWorkerAsync();
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            TextToSend = "play";
            backgroundWorker2.RunWorkerAsync();
        }

        private void ButtonForwards_Click(object sender, RoutedEventArgs e)
        {
            TextToSend = "next";
            backgroundWorker2.RunWorkerAsync();
        }

        private void ButtonBackwards_Click(object sender, RoutedEventArgs e)
        {
            TextToSend = "back";
            backgroundWorker2.RunWorkerAsync();
        }
    }
}
