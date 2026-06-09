using UnityEngine;

namespace Handler.Extensions
{
    public static class AudioExtensions
    {
        public static void Mute(this AudioSource self, bool mute) => self.mute = mute;
        
        public static void Unmute(this AudioSource self) => self.mute = false;
        
        public static void Stop(this AudioSource self) => self.Stop();
        
        public static void Pause(this AudioSource self) => self.Pause();
        
        public static void Unpause(this AudioSource self) => self.UnPause();
    }
}