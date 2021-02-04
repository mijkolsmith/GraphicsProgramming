using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

abstract class Lesson
{
	public virtual void Initialize() { }
	public virtual void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch) { }
	public virtual void Update(GameTime gameTime) { }
	public virtual void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch) { }
}