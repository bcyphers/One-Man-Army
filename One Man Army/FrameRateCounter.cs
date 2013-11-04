using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace One_Man_Army
{
    /// <summary>
    /// This component is used throughout the game to monitor the frame rate.
    /// </summary>
    class FrameRateCounter : DrawableGameComponent
    {
        private ContentManager _content;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        private int _frameRate = 0;
        private int _counter = 0;

        private float _jump = 0f;
        private float _elapsedTime = 0f;

        public FrameRateCounter(Game game)
            : base(game)
        {
            _content = new ContentManager(game.Services);
        }

        protected override void LoadContent()
        {
            IGraphicsDeviceService graphicsService =
                (IGraphicsDeviceService)this.Game.Services.GetService(
                typeof(IGraphicsDeviceService));

            _spriteBatch = new SpriteBatch(graphicsService.GraphicsDevice);
            _font = _content.Load<SpriteFont>("Content/Fonts/gamefont");
        }

        protected override void UnloadContent()
        {
            _content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (gameTime.ElapsedGameTime.TotalMilliseconds > 20)
                _jump = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedTime >= 1000.0f)
            {
                _elapsedTime -= 1000.0f;
                _frameRate = _counter;
                _counter = 0;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _counter++;

            string _string = "fps: " + _frameRate.ToString();
            string _string2 = "Lag: " + _jump.ToString();
            Rectangle _titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 _location = new Vector2(_titleSafeArea.Width - _font.MeasureString(_string).X, _titleSafeArea.Y);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, _string, _location, Color.Yellow);

            _location = new Vector2(_titleSafeArea.Width - _font.MeasureString(_string2).X,
                _titleSafeArea.Y + _font.MeasureString(_string2).Y);

            _spriteBatch.DrawString(_font, _string2, _location, Color.Yellow);
            _spriteBatch.End();
        }
    }
}
