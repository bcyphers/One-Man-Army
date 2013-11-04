using System.IO;
using System.Threading;
using EasyStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

namespace Sample
{
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont font;

		IAsyncSaveDevice saveDevice;

		GamePadState gps, gpsPrev;
		KeyboardState ks, ksPrev;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			// we can set our supported languages explicitly or we can allow the
			// game to support all the languages. the first language given will
			// be the default if the current language is not one of the supported
			// languages. this only affects the text found in message boxes shown
			// by EasyStorage and does not have any affect on the rest of the game.
			EasyStorageSettings.SetSupportedLanguages(Language.French, Language.Spanish);

			// on Windows Phone we use a save device that uses IsolatedStorage
			// on Windows and Xbox 360, we use a save device that gets a shared StorageDevice to handle our file IO.
#if WINDOWS_PHONE
			saveDevice = new IsolatedStorageSaveDevice();
#else
			// create and add our SaveDevice
			SharedSaveDevice sharedSaveDevice = new SharedSaveDevice();
			Components.Add(sharedSaveDevice);

			// make sure we hold on to the device
			saveDevice = sharedSaveDevice;

			// hook two event handlers to force the user to choose a new device if they cancel the
			// device selector or if they disconnect the storage device after selecting it
			sharedSaveDevice.DeviceSelectorCanceled += (s, e) => e.Response = SaveDeviceEventResponse.Force;
			sharedSaveDevice.DeviceDisconnected += (s, e) => e.Response = SaveDeviceEventResponse.Force;

			// prompt for a device on the first Update we can
			sharedSaveDevice.PromptForDevice();
#endif

			// we use the tap gesture for input on the phone
			TouchPanel.EnabledGestures = GestureType.Tap;

#if XBOX
			// add the GamerServicesComponent
			Components.Add(new Microsoft.Xna.Framework.GamerServices.GamerServicesComponent(this));
#endif

			// hook an event so we can see that it does fire
			saveDevice.SaveCompleted += new SaveCompletedEventHandler(saveDevice_SaveCompleted);

			base.Initialize();
		}

		void saveDevice_SaveCompleted(object sender, FileActionCompletedEventArgs args)
		{
			// just write some debug output for our verification
			Debug.WriteLine("SaveCompleted!");
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			font = Content.Load<SpriteFont>("Font");
		}

		protected override void Update(GameTime gameTime)
		{
			gpsPrev = gps;
			ksPrev = ks;
			gps = GamePad.GetState(PlayerIndex.One);
			ks = Keyboard.GetState();

			bool tapped = false;
			while (TouchPanel.IsGestureAvailable)
			{
				GestureSample gesture = TouchPanel.ReadGesture();
				if (gesture.GestureType == GestureType.Tap)
					tapped = true;
			}

			if ((gps.IsButtonDown(Buttons.A) && gpsPrev.IsButtonUp(Buttons.A)) ||
				(ks.IsKeyDown(Keys.Space) && ksPrev.IsKeyUp(Keys.Space)) ||
				tapped)
			{
				// make sure the device is ready
				if (saveDevice.IsReady)
				{
					// save a file asynchronously. this will trigger IsBusy to return true
					// for the duration of the save process.
					saveDevice.SaveAsync(
						"TestContainer",
						"MyFile.txt",
						stream =>
						{
							// simulate a really, really long save operation so we can visually see that
							// IsBusy stays true while we're saving
							Thread.Sleep(3000);

							using (StreamWriter writer = new StreamWriter(stream))
								writer.WriteLine("Hello, World!");
						});
				}
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			Vector2 textPos = new Vector2(
				GraphicsDevice.Viewport.TitleSafeArea.X + 50,
				GraphicsDevice.Viewport.TitleSafeArea.Y + 10);

			spriteBatch.Begin();

			spriteBatch.DrawString(
				font, 
				string.Format("Save device {0} ready.", saveDevice.IsReady ? "is" : "is not"), 
				textPos, 
				Color.White);
			textPos.Y += font.LineSpacing;

			spriteBatch.DrawString(
				font,
				string.Format("Save device {0} busy.", saveDevice.IsBusy ? "is" : "is not"),
				textPos,
				Color.White);
			textPos.Y += font.LineSpacing;

			if (saveDevice.IsReady)
			{
#if WINDOWS_PHONE
				string instructions = "Tap the screen to save a file.";
#else
				string instructions = "Press the A button or space key to save a file.";
#endif
				spriteBatch.DrawString(font, instructions, textPos, Color.White);
			}

			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
