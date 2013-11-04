using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using DataTypeLibrary;
using EasyStorage;

namespace One_Man_Army
{
    /// <summary>
    /// A struct that holds all the information in a game save at runtime.
    /// </summary>
    public struct SaveGameData
    {
        public const int NUM_DATA = 7;
        public int MaxWave;
        public TimeSpan TimePlayed;
        public int Deaths;
        public int TanksKilled;
        public int HelisKilled;
        public float DamageDealt;
        public float DamageTaken;
        //public int InstanceNumber;
        //public DateTime TimeStamp;

        public int CurrentWave
        {
            get { return (MaxWave / 3) * 3; }
        }

        public bool IsCleared
        {
            get { return TimePlayed == TimeSpan.FromSeconds(0); }
        }

        public SaveGameData(int wave, float time, int deaths, int tanks, int helis, 
            float damageDealt, float damageTaken)
        {
            MaxWave = wave;
            TimePlayed = TimeSpan.FromSeconds(time);
            Deaths = deaths;
            TanksKilled = tanks;
            HelisKilled = helis;
            DamageDealt = damageDealt;
            DamageTaken = damageTaken;
        }

        public void Clear()
        {
            MaxWave = 0;
            TimePlayed = TimeSpan.FromSeconds(0);
            Deaths = 0;
            TanksKilled = 0;
            HelisKilled = 0;
            DamageDealt = 0;
            DamageTaken = 0;
        }
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class One_Man_Army_Game : Microsoft.Xna.Framework.Game
    {
        #region Fields

        // Static fields for handling storage.
        // A generic EasyStorage save device.
        public static IAsyncSaveDevice SaveDevice;

        public static SaveGameData CampaignGameData = new SaveGameData(0, 0, 0, 0, 0, 0, 0);
        public static SaveGameData SurvivalGameData = new SaveGameData(0, 0, 0, 0, 0, 0, 0);

        public static bool IsCampaignFinished
        {
            get { return CampaignGameData.MaxWave >= 24; }
        }

        // The name of the file where options will be stored.
        public static string FileName_Options = "One_Man_Army_Options";
        public static string FileName_Game_Campaign = "One_Man_Army_Game_Campaign";
        public static string FileName_Game_Survival = "One_Man_Army_Game_Survival";
        public static string FileName_Awards = "One_Man_Army_Awards";

        // The name of the save file you'll find if you go into your memory
        // options on the Xbox.
        public static string ContainerName = "One_Man_Army_Save";
        
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private FrameRateCounter frameRate;
        private ScreenManager screenManager;
        GamerServicesComponent gamerServices;
        
        private SignedInGamer gamer;
        public SignedInGamer Gamer
        {
            get { return gamer; }
        }

        private AudioEngine audioEngine;
        public AudioEngine AudioEngine
        {
            get { return audioEngine; }
        }

        private WaveBank musicWaveBank;
        private WaveBank gameplaySfxWaveBank;
        private WaveBank menuSfxWaveBank;
        private WaveBank weaponSfxWaveBank;

        private SoundBank musicSoundBank;
        public SoundBank MusicBank
        {
            get { return musicSoundBank; }
        }

        private SoundBank sfxSoundBank;
        public SoundBank SFXBank
        {
            get { return sfxSoundBank; }
        }

        public GraphicsDeviceManager Graphics
        {
            get { return graphics; }
            set { graphics = value; }
        }

        private static Random random = new Random();
        public static Random Random
        {
            get { return random; }
        }
        
        private string[] weaponKeys;
        public string[] WeaponKeys
        {
            get { return weaponKeys; }   
        }

        private IDictionary<string, Weapon> weapons;
        public IDictionary<string, Weapon> AllWeapons
        {
            get { return weapons; }
        }

        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 1280;
        private const int BackBufferHeight = 720;

        private RenderTarget2D RenderTarget;

        #endregion

        #region Initialize

        public One_Man_Army_Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;
            SetFullScreen(false);
            IsMouseVisible = true;

            Content.RootDirectory = "Content";

            audioEngine = new AudioEngine("Content/Audio/One_Man_Army.xgs");
            musicSoundBank = new SoundBank(audioEngine, "Content/Audio/Music.xsb");
            sfxSoundBank = new SoundBank(audioEngine, "Content/Audio/SFX.xsb");
            musicWaveBank = new WaveBank(audioEngine, "Content/Audio/Music.xwb");
            gameplaySfxWaveBank = new WaveBank(audioEngine, "Content/Audio/Gameplay SFX.xwb");
            menuSfxWaveBank = new WaveBank(audioEngine, "Content/Audio/Menu SFX.xwb");
            weaponSfxWaveBank = new WaveBank(audioEngine, "Content/Audio/Weapon SFX.xwb");

            AudioCategory category = audioEngine.GetCategory("Music");

            category.SetVolume(0.5f);

            // Create the gamer services component.
            gamerServices = new GamerServicesComponent(this);
            Components.Add(gamerServices);

            // Create the screen manager component.
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            // Create the frame rate counter component.
            frameRate = new FrameRateCounter(this);
            // Components.Add(frameRate);
            
            // Framerate differs between platforms.
            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);           
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            RenderTarget = new RenderTarget2D(GraphicsDevice,
                (int)(GraphicsDevice.Viewport.Width),
                (int)(GraphicsDevice.Viewport.Height));

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new StartScreen(), null);
            
            weaponKeys = new string[Weapon.NumWeapons];
            weapons = new Dictionary<string, Weapon>();

            for (int i = 0; i < Weapon.NumWeapons; i++)
            {
                WeaponData data = new WeaponData();
                data = Content.Load<WeaponData>("Weapons/" + (i < 10 ? "0" : "") + i);

                Weapon weapon = new Weapon(data);

                weapon.ArmsTexture = Content.Load<Texture2D>("Sprites/Player/Weapons/" + (i < 10 ? "0" : "") + i + "/arms");
                weapon.BulletTexture = Content.Load<Texture2D>("Sprites/Player/Weapons/" + (i < 10 ? "0" : "") + i + "/bullet");
                weapon.WeaponTexture = Content.Load<Texture2D>("Sprites/Player/Weapons/" + (i < 10 ? "0" : "") + i + "/base");
                weapon.FireAnimation = Content.Load<Texture2D>("Sprites/Player/Weapons/" + (i < 10 ? "0" : "") + i + "/animation");
                weapon.FireTextParticle = Content.Load<Texture2D>("Sprites/Player/Weapons/" + (i < 10 ? "0" : "") + i + "/sound");

                weapons.Add(weapon.Name, weapon);
                weaponKeys[i] = weapon.Name;
            }
        }

        /// <summary>
        /// Called at the beginning of the game, initializing the game with a single player in control
        /// and sets up the storage devices.
        /// </summary>
        public void InitializeGame(PlayerIndex index)
        {
            // The game is only localized in English.
            EasyStorageSettings.SetSupportedLanguages(Language.English);

            // Create and add our SaveDevice.
            SharedSaveDevice sharedSaveDevice = new SharedSaveDevice();
            Components.Add(sharedSaveDevice);

            // Hook two event handlers to force the user to choose a new device if they cancel the
            // device selector or if they disconnect the storage device after selecting it.
            sharedSaveDevice.DeviceSelectorCanceled +=
                (s, e) => e.Response = SaveDeviceEventResponse.Force;
            sharedSaveDevice.DeviceDisconnected +=
                (s, e) => e.Response = SaveDeviceEventResponse.Force;

            // Prompt for a device on the first Update we can
            sharedSaveDevice.PromptForDevice();
            sharedSaveDevice.DeviceSelected += (s, e) =>
            {
                //Save our save device to the global counterpart, so we can access it
                //anywhere we want to save/load
                SaveDevice = (SaveDevice)s;
                CampaignGameData = LoadGameData(FileName_Game_Campaign);
                SurvivalGameData = LoadGameData(FileName_Game_Survival);
            };
        }

        /// <summary>
        /// Load the game data from its permanent location into a struct at runtime.
        /// </summary>
        public SaveGameData LoadGameData(string fileName)
        {
            SaveGameData data = new SaveGameData(0, 0, 0, 0, 0, 0, 0);

            if (SaveDevice.FileExists(ContainerName, fileName))
            {
                SaveDevice.Load(ContainerName, fileName,
                    stream =>
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            data.MaxWave = int.Parse(reader.ReadLine());
                            data.TimePlayed = TimeSpan.FromSeconds(float.Parse(reader.ReadLine()));
                            data.Deaths = int.Parse(reader.ReadLine());
                            data.TanksKilled = int.Parse(reader.ReadLine());
                            data.HelisKilled = int.Parse(reader.ReadLine());
                            data.DamageDealt = float.Parse(reader.ReadLine());
                            data.DamageTaken = float.Parse(reader.ReadLine());
                            //data.InstanceNumber = int.Parse(reader.ReadLine());
                        }
                    }
                );
            }

            return data;
        }

        /// <summary>
        /// Save the game data, kept in the struct during runtime, to a permanent file.
        /// </summary>
        public void SaveGameData(SaveGameData data, string fileName)
        {
            if (SaveDevice.IsReady)
            {
                SaveDevice.SaveAsync(ContainerName, fileName,
                    stream =>
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.WriteLine(data.MaxWave);
                            writer.WriteLine(data.TimePlayed.TotalSeconds);
                            writer.WriteLine(data.Deaths);
                            writer.WriteLine(data.TanksKilled);
                            writer.WriteLine(data.HelisKilled);
                            writer.WriteLine(data.DamageDealt);
                            writer.WriteLine(data.DamageTaken);
                            //writer.WriteLine(data.InstanceNumber);
                        }
                    }
                );
            }
        }

        ///// <summary>
        ///// Load the game data from its permanent location into a struct at runtime.
        ///// </summary>
        //public SaveGameData LoadGameDataInstance(int instance)
        //{
        //    SaveGameData data = new SaveGameData(0, 0, 0, 0, 0, 0, 0, 0, DateTime.Today);

        //    if (SaveDevice.FileExists(ContainerName, FileName_Game + instance))
        //    {
        //        SaveDevice.Load(ContainerName, FileName_Game + instance,
        //            stream =>
        //            {
        //                using (StreamReader reader = new StreamReader(stream))
        //                {
        //                    data.MaxWave = int.Parse(reader.ReadLine());
        //                    data.TimePlayed = TimeSpan.FromSeconds(float.Parse(reader.ReadLine()));
        //                    data.Deaths = int.Parse(reader.ReadLine());
        //                    data.TanksKilled = int.Parse(reader.ReadLine());
        //                    data.HelisKilled = int.Parse(reader.ReadLine());
        //                    data.DamageDealt = float.Parse(reader.ReadLine());
        //                    data.DamageTaken = float.Parse(reader.ReadLine());
        //                    data.InstanceNumber = int.Parse(reader.ReadLine());
        //                    data.TimeStamp = DateTime.Parse(reader.ReadLine());
        //                }
        //            }
        //        );
        //    }

        //    return data;
        //}

        ///// <summary>
        ///// Save the game data, kept in the struct during runtime, to a permanent file.
        ///// </summary>
        //public void SaveNewGameDataInstance()
        //{
        //    if (SaveDevice.IsReady)
        //    {
        //        SaveDevice.SaveAsync(ContainerName, FileName_Game + CampaignGameData.InstanceNumber,
        //            stream =>
        //            {
        //                using (StreamWriter writer = new StreamWriter(stream))
        //                {
        //                    writer.WriteLine(CampaignGameData.MaxWave);
        //                    writer.WriteLine(CampaignGameData.TimePlayed.TotalSeconds);
        //                    writer.WriteLine(CampaignGameData.Deaths);
        //                    writer.WriteLine(CampaignGameData.TanksKilled);
        //                    writer.WriteLine(CampaignGameData.HelisKilled);
        //                    writer.WriteLine(CampaignGameData.DamageDealt);
        //                    writer.WriteLine(CampaignGameData.DamageTaken);
        //                    writer.WriteLine(CampaignGameData.InstanceNumber);
        //                    writer.WriteLine(DateTime.Now);
        //                }
        //            }
        //        );
        //    }

        //    CampaignGameData.Clear();
        //    CampaignGameData.InstanceNumber++;
        //    SaveGameData();
        //}

        #endregion

        #region Draw

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(RenderTarget);

            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin();

            //float width = (int)(GraphicsDevice.Viewport.TitleSafeArea.Width);
            //float height = (int)(GraphicsDevice.Viewport.TitleSafeArea.Height);
            //int x = (int)(GraphicsDevice.Viewport.TitleSafeArea.X + width * .05);
            //int y = (int)(GraphicsDevice.Viewport.TitleSafeArea.Y + height * .05);
            //int w = (int)(width * 0.9);
            //int h = (int)(height * 0.9);
            int w = (int)(GraphicsDevice.Viewport.Width);
            int h = (int)(GraphicsDevice.Viewport.Height);
            int x = (int)(GraphicsDevice.Viewport.X);
            int y = (int)(GraphicsDevice.Viewport.Y);

            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Draw(RenderTarget, new Rectangle(x, y, w, h), Color.White);

            spriteBatch.End();

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the gamertag of a specified PlayerIndex.
        /// </summary>
        public void GetGamerTag(PlayerIndex? playerIndex)
        {
            PlayerIndex index = (PlayerIndex)playerIndex;

            if (SignedInGamer.SignedInGamers[index] != null)
            {
                this.gamer = SignedInGamer.SignedInGamers[index];
            }
        }

        /// <summary>
        /// Attempt to purchase the full version of the game. If the corrent player does not have sufficient
        /// privelages, display a message box.
        /// </summary>
        /// <param name="index"></param>
        public void AttemptBuyFullVersion(PlayerIndex index)
        {
            GetGamerTag(index);

            SignedInGamer gamer = Gamer;

            if (gamer != null && gamer.IsSignedInToLive && gamer.Privileges.AllowPurchaseContent)
                Guide.ShowMarketplace(index);
            else
                screenManager.AddScreen(new MessageBoxScreen("Your account does not allow Marketplace purchases.\nA button = ok", false),
                    index);
        }

        /// <summary>
        /// A handy little function that gives a random float between two
        /// values. This will be used in the particle system, in particilar in
        /// ParticleSystem.InitializeParticle.
        /// </summary>
        public static float RandomBetween(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        /// <summary>
        /// Sets the screen to window or fullscreen mode
        /// </summary>
        public void SetFullScreen(bool fullscreen)
        {
            graphics.IsFullScreen = fullscreen;
        }

        /// <summary>
        /// rotates slowly in one direction to a target vector
        /// </summary>
        public static Vector2 StepSlowlyToVector(
            float elapsed, float speedOfRotation, Vector2 destinationVector, Vector2 currentVector)
        {
            float maxRotation = elapsed * MathHelper.Pi * 2 * (speedOfRotation / 2000);
            float currentRotation = (float)Math.Atan2(currentVector.Y, currentVector.X);
            float destinationRotation = (float)Math.Atan2(destinationVector.Y, destinationVector.X);

            if (Math.Abs(destinationRotation - currentRotation) <= maxRotation)
                return destinationVector;

            float difFromPi = MathHelper.Pi - destinationRotation;

            destinationRotation += difFromPi;

            currentRotation += difFromPi;

            while (currentRotation > MathHelper.Pi)
                currentRotation -= MathHelper.Pi * 2;

            while (currentRotation < -MathHelper.Pi)
                currentRotation += MathHelper.Pi * 2;

            if (currentRotation > 0)
            {
                currentRotation -= difFromPi;
                currentRotation += maxRotation;
            }
            else
            {
                currentRotation -= difFromPi;
                currentRotation -= maxRotation;
            }

            return new Vector2((float)Math.Cos(currentRotation), (float)Math.Sin(currentRotation));

        }

        #endregion
    }
}
