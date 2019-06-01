using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongSync
{
    static class SongManager
    {
        private static System.Media.SoundPlayer player = new System.Media.SoundPlayer();
        private static WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();

        public static string songfolder = "C:\temp";
        public static string currentsong = "chordsample.mp3";
        public static string currentfile = "";
        public static bool isplaying = false;
        private static double songtime = 0;
        public static bool isStopPressed = false;

        public static void setandPlay(bool isbuttonpress)
        {
            if (currentsong.EndsWith("mp3"))
            {
                //if after stop or pause. init the song
                if (!isplaying)
                {
                    if (currentfile.Equals("")) //if no song dont play
                        return;
                    if (songtime == 0) //if not continue then init first time
                    {
                        isStopPressed = false;
                        string output = (songfolder + "\\" + currentsong).Replace(@"\\", @"\");
                        wplayer.URL = currentfile;
                        isplaying = true;
                    }
                    else
                    {
                        isStopPressed = false;
                        wplayer.controls.currentPosition = songtime;
                        wplayer.controls.play();
                        isplaying = true;
                    }
                }
            }
            else if (currentsong.EndsWith("wav"))
            {
                if (!isplaying)
                {
                    player.SoundLocation = songfolder + "\\" + currentsong;

                }
            }
        }

        public static void Pause()
        {
            if (isplaying)
            {
                wplayer.controls.pause();
                player.Stop();
                songtime = wplayer.controls.currentPosition;
                isplaying = false;
            }
        }


        public static void Stop()
        {
            if (isplaying)
            {
                
                isStopPressed = true; //needs to be called first cause when stopping, an stop event gets triggered

                wplayer.controls.currentPosition = 0;
                wplayer.controls.stop();
                player.Stop();
                isplaying = false;
            }
        }


        //public static void Play()
        //{
        //    if (!isplaying)
        //    {
        //        wplayer.controls.play();
        //        player.PlaySync();
        //        isplaying = true;
        //    }
        //}

        public static void Next()
        {
            
                if (wplayer.controls.get_isAvailable("next"))
                {
                    wplayer.controls.next();
                }

            
        }


        public static void Back()
        {
            if (isplaying)
            {
                if (wplayer.controls.get_isAvailable("previous"))
                {
                    wplayer.controls.previous();
                }
            }
        }

        public static string getCurrentPos()
        {
            return (wplayer.controls.currentPosition).ToString();
        }

        public static double getSongPauseTime()
        {
            return songtime;
        }

        public static void Sync(string synctime)
        {
            wplayer.controls.currentPosition = double.Parse(synctime);
        }

        public static string getSongName()
        {

            return wplayer.controls.currentItem.name;
        }

        public static string getMaxPos()
        {
            if (wplayer.controls.currentItem != null)
                return (wplayer.controls.currentItem.duration).ToString();


            return (string)"99999";
        }

        public static void addPlaystateEventhandler(WMPLib._WMPOCXEvents_PlayStateChangeEventHandler ev)
        {
            wplayer.PlayStateChange += ev;
        }
    }
}
