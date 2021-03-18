using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Game1 : Game
{
	private GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;
	private Lesson currentLesson;

	public Game1()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;

		currentLesson = new LiveLesson4();
	}

	protected override void Initialize()
	{
		// Add your initialization logic here
		currentLesson.Initialize();

		_graphics.PreferredBackBufferWidth = 1080;
		_graphics.PreferredBackBufferHeight = 1080;
		_graphics.ApplyChanges();

		base.Initialize();
	}

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(GraphicsDevice);

		// Use this.Content to load your game content here
		currentLesson.LoadContent(Content, _graphics, _spriteBatch);
	}

	protected override void Update(GameTime gameTime)
	{
		if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			Exit();

		// Add your update logic here
		currentLesson.Update(gameTime);
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);

		// Add your drawing code here
		currentLesson.Draw(gameTime, _graphics, _spriteBatch);
		base.Draw(gameTime);
	}
}