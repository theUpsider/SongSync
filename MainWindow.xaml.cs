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
using System.Windows.Forms;
using ListBox = System.Windows.Controls.ListBox;

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
        private static BackgroundWorker backgroundWorker3timer = new BackgroundWorker();

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
            backgroundWorker3timer.DoWork += new DoWorkEventHandler(backgroundWorker3_DoWork);

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

            InitSongTimer();

            SongManager.addPlaystateEventhandler(new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(player_PlayStateChange));
        }

        private void player_PlayStateChange(int i)
        {
            if (!SongManager.isStopPressed && i == 1)
                SongManager.Next();
            if (i == 1)
                SongManager.isplaying = false;
        }
            //----------------------------------------------------------------------------------------
            //RECIEVE
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
                        switch (recieve)
                        {
                            case "play":
                                SongManager.setandPlay(true);
                                break;
                            case "stop":
                                SongManager.Stop();
                                break;
                            case "pause":
                                SongManager.Pause();
                                break;
                            case "next":
                                SongManager.Next();
                                break;
                            case "back":
                                SongManager.Back();
                                break;

                            default:
                                if (recieve.Contains("sync"))
                                {
                                    string newrec = recieve.Remove(0, 4);
                                    SongManager.Sync(newrec);
                                }
                                break;
                        }
                    }));

                    recieve = "";

                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
                }
            }
        }
        //---------------------------------------------------------------------------------
        //SELF
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (client == null)
                return;
            if (client.Connected)
            {
                STW.WriteLine(TextToSend);
                this.chatBox.Dispatcher.Invoke(new Action(delegate ()
                {
                    chatBox.AppendText("Me:" + TextToSend + "\n");
                    switch (TextToSend)
                    {
                        case "play":
                            SongManager.setandPlay(true);
                            break;
                        case "stop":
                            SongManager.Stop();
                            break;
                        case "pause":
                            SongManager.Pause();
                            break;
                        case "next":
                            SongManager.Next();
                            break;
                        case "back":
                            SongManager.Back();
                            break;

                        default:
                            SongManager.Sync(SongManager.getCurrentPos());
                            break;
                    }
                    //if (TextToSend.Equals("play"))
                    //    SongManager.setandPlay();
                    //if (TextToSend.Equals("stop"))
                    //    SongManager.Stop();
                    //if (TextToSend.Equals("pause"))
                    //    SongManager.Pause();
                    //if (TextToSend.Equals("next"))
                    //    SongManager.Next();                    
                    //if (TextToSend.Equals("back"))
                    //    SongManager.Back();

                }));
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Sending failed");
            }
            backgroundWorker2.CancelAsync();
        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {


        }

        private void InitSongTimer()
        {
            Timer timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // in miliseconds
            timer1.Start();
        }

        //Update Information about song
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (SongManager.isplaying)
            {
                //SONG TIME
                this.labelplaytime.Dispatcher.Invoke(new Action(delegate ()
                {
                    labelplaytime.Content = SongManager.getCurrentPos();
                }));
                //SONG NAME
                this.labelSongname.Dispatcher.Invoke(new Action(delegate ()
                {
                    labelSongname.Content = SongManager.getSongName();
                }));
                
            }
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
                System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
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
            SongManager.currentsong = textBoxFirstSong.Text.Replace(@"\\", @"\");
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
            if(listBoxFiles.SelectedItem == null)
            {
                System.Windows.Forms.MessageBox.Show("No Song selected");
                return;
            }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextToSend = "sync" + SongManager.getCurrentPos();
            backgroundWorker2.RunWorkerAsync();
        }

        private void ButtonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if(result == System.Windows.Forms.DialogResult.OK)
                {
                    SongManager.songfolder = dialog.SelectedPath;
                    string[] filePaths = Directory.GetFiles(SongManager.songfolder);
                    if(filePaths.Length >1)
                    SongManager.currentsong = filePaths[0] ?? String.Empty;
                    foreach (string filename in filePaths)
                    {
                        if(filename.EndsWith(".mp3"))
                            listBoxFiles.Items.Add(filename);
                    }
                }
            }
        }

        //set new file to play
        private void ListBoxFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;
            string selectedItem = ((ListBox)sender).Items[((ListBox)sender).SelectedIndex].ToString();
            SongManager.currentfile = selectedItem;
        }
    }
}
