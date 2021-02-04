using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

class LiveLesson1 : Lesson
{
	VertexPositionColor[] vertices = {
			//+z (front)
			new VertexPositionColor( new Vector3(.5f,.5f,.5f), Color.Red), //left, top
			new VertexPositionColor( new Vector3(.5f,-.5f,.5f), Color.Green), //left, bottom
			new VertexPositionColor( new Vector3(-.5f,.5f,.5f), Color.Blue), //right, top
			new VertexPositionColor( new Vector3(-.5f,-.5f,.5f), Color.Yellow), //right, bottom

			//-z (back)
			new VertexPositionColor( new Vector3(.5f,.5f,-.5f), Color.Yellow), //left, top
			new VertexPositionColor( new Vector3(.5f,-.5f,-.5f), Color.Blue), //left, bottom
			new VertexPositionColor( new Vector3(-.5f,.5f,-.5f), Color.Green), //right, top
			new VertexPositionColor( new Vector3(-.5f,-.5f,-.5f), Color.Red), //right, bottom
		};

	int[] indices = {
			//front
			0, 1, 2,
			2, 1, 3,

			//back
			4, 6, 5,
			5, 6, 7,

			//right
			2, 3, 6,
			3, 7, 6,

			//left
			1, 0, 4,
			5, 1, 4,

			//top
			0, 2, 4,
			4, 2, 6,

			//bottom
			3, 1, 5,
			3, 5, 7
		};

	BasicEffect effect;

	public override void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
	{
		effect = new BasicEffect(graphics.GraphicsDevice);
	}

	public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
	{
		GraphicsDevice device = graphics.GraphicsDevice;
		device.Clear(Color.Black);

		//turning mechanics
		effect.World = Matrix.Identity * Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds * 10f) * Matrix.CreateRotationX((float)gameTime.TotalGameTime.TotalSeconds / 2f);
		effect.View = Matrix.CreateLookAt(-Vector3.Forward * 5, Vector3.Zero, Vector3.Up);
		effect.Projection = Matrix.CreatePerspectiveFieldOfView((MathF.PI / 180f) * 65f, device.Viewport.AspectRatio, 0.1f, 100f);

		effect.VertexColorEnabled = true;
		foreach (EffectPass pass in effect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
		}
	}
}