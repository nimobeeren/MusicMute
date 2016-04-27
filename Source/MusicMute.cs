/*                                                USEFUL DOCS AND INFO
 * How to make a non-part plugin
 * http://forum.kerbalspaceprogram.com/index.php?/topic/44517-how-to-make-a-non-part-plugin/    
 * Nifty255's KSP Modding Tutorials, Volume 1                                                   
 * https://www.youtube.com/playlist?list=PLTWjydcIcK7rao4jyt8UP-JxapS5e0781                     
 * ConfigNode is now in KSPUtil -.-
 * http://forum.kerbalspaceprogram.com/index.php?/topic/97541-11-fuel-tanks-plus-181-2016-04-24/&page=17#comment-2483032
 * Documentation for GameEvents and how to hook into them
 * https://anatid.github.io/XML-Documentation-for-the-KSP-API/class_game_events.html
*/

using System;
using UnityEngine;

namespace MusicMute
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class MusicMute : MonoBehaviour
    {
        /*                DEFAULT KEYBINDINGS             */
        /*   (This is overridden by the config values)    */
        KeyCode toggleKey = KeyCode.F8;
        KeyCode modifierKey = KeyCode.None;
        /*                 END KEYBINDINGS                */

        // Default values
        bool muted = false;
        bool startMuted = false;
        float oldVolume = 0.40f;

        public bool Muted
        {
            get
            {
                return muted;
            }
            set
            {
                // Mute
                if (value == true)
                {
                    // Save the old music volume
                    oldVolume = GameSettings.MUSIC_VOLUME;

                    // Mute the music
                    MusicLogic.SetVolume(0f);
                    print("[MusicMute] Muted music");
                }
                // Unmute
                else
                {
                    // Set the music volume to what it was before
                    MusicLogic.SetVolume(oldVolume);
                    print("[MusicMute] Set music volume to: " + oldVolume);
                }

                muted = value;
            }
        }

        // Runs on succesful load
        public void Start()
        {
            // Hook into the game scene switch events
            GameEvents.onGameSceneSwitchRequested.Add(this.onGameSceneSwitchRequested);
            GameEvents.onLevelWasLoaded.Add(this.onLevelWasLoaded);

            // Load the config file
            var configFile = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/MusicMute/Config/Settings.cfg");

            // Get the start muted preference from config file
            string cfgStartMuted = configFile.GetNode("MusicMute").GetValue("startMuted");
            if (!bool.TryParse(cfgStartMuted, out startMuted))
            {
                print("[MusicMute] Could not parse startMuted value. Please use true or false as the value in the config file.");
            }

            // Get the user-specified toggle key
            string cfgToggleKey = configFile.GetNode("MusicMute").GetValue("toggleKey");

            // Set the toggle key to the one specified in the config file
            if (cfgToggleKey != null)
            {
                try
                {
                    toggleKey = (KeyCode)Enum.Parse(typeof(KeyCode), cfgToggleKey, true);
                }
                catch
                {
                    print("[MusicMute] Could not parse toggle key from config file. Probably misspelled your key. Using the default F8.");
                }
            }
            else
            {
                print("[MusicMute] Could not find toggle key in config file. Using the default F8.");
            }

            // Get the user-specified modifier key
            cfgToggleKey = configFile.GetNode("MusicMute").GetValue("modifierKey");

            // Set the modifier key to the one specified in the config file
            if (!string.IsNullOrEmpty(cfgToggleKey))
            {
                try
                {
                    modifierKey = (KeyCode)Enum.Parse(typeof(KeyCode), cfgToggleKey, true);
                }
                catch
                {
                    print("[MusicMute] Could not parse modifier key from config file. You probably misspelled your key.");
                }
            }
            else
            {
                print("[MusicMute] Could not find modifier key in config file.");
            }

            // Mute very fastttttttt if the user desires
            if (startMuted)
            {
                Muted = true;
            }

            // Keep the plugin running for ever, and ever, and ever... Until we run out of power
            DontDestroyOnLoad(this);
        }

        // Runs all the time
        public void Update()
        {
            // Checks for keypress of specified toggle key
            if (Input.GetKeyDown(toggleKey))
            {
                // Checks if modifier key is also being pressed (if set in config file)
                if (Input.GetKey(modifierKey) || modifierKey == KeyCode.None)
                {
                    // Toggle the muted state
                    Muted = !Muted;
                }
            }
        }

        // Runs on exit
        public void OnDestroy()
        {
            // Unhook from the scene switch events
            GameEvents.onGameSceneSwitchRequested.Remove(this.onGameSceneSwitchRequested);
            GameEvents.onLevelWasLoaded.Remove(this.onLevelWasLoaded);
        }

        // Sets the music volume to 0
        public void VerifyMuted()
        {
            MusicLogic.SetVolume(0f);
        }

        // Runs when scene switch is requested
        private void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> action)
        {
            // The game likes to play music when we switch scenes, so we have to tell it to shut up once more
            if (Muted)
            {
                VerifyMuted();
            }
        }

        // Runs when scene is done switching
        private void onLevelWasLoaded(GameScenes action)
        {
            if (Muted)
            {
                VerifyMuted();
            }
        }
    }
}
