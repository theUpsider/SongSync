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

        public static string songfolder = "C:\\temp";
        public static string currentsong = "chorsample.mp3";
        public static bool isplaying = false;

        public static void setfolderandsong()
        {
            if (currentsong.EndsWith("mp3"))
            {
                if (!isplaying) { 
                    wplayer.URL = songfolder + "\\"+ currentsong;

                }
            }else if(currentsong.EndsWith("wav"))
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
                isplaying = false;
            }
        }


        public static void Stop()
        {
            if (isplaying)
            {
                wplayer.controls.stop();
                player.Stop();
                isplaying = false;
            }
        }


        public static void Play()
        {
            if (!isplaying)
            {
                wplayer.controls.play();
                player.PlaySync();
                isplaying = true;
            }
        }

        public static void Next()
        {
            if (isplaying)
            {
                if (wplayer.controls.get_isAvailable("next"))
                {
                    wplayer.controls.next();
                }
                
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

    }
}
