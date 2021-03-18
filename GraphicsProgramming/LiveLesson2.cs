using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

class LiveLesson2 : Lesson
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionColorNormal : IVertexType
	{
		public Vector3 Position;
		public Color Color;
		public Vector3 Normal;
		public Vector2 Texture;

		static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration
		(
			new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
			new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
			new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
			new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
		);

		VertexDeclaration IVertexType.VertexDeclaration => _vertexDeclaration;

		public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal, Vector2 texture)
		{
			Position = position;
			Color = color;
			Normal = normal;
			Texture = texture;
		}
	}

	private VertexPositionColorNormal[] vertices = {
			//FRONT
			new VertexPositionColorNormal( new Vector3(-1f, 1f, 1f), Color.Red, Vector3.Backward, new Vector2(0,1) ),
			new VertexPositionColorNormal( new Vector3(1f, -1f, 1f), Color.Red, Vector3.Backward, new Vector2(1,0) ),
			new VertexPositionColorNormal( new Vector3(-1f, -1f, 1f), Color.Red, Vector3.Backward, new Vector2(0,0) ),
			new VertexPositionColorNormal( new Vector3(1f, 1f, 1f), Color.Red, Vector3.Backward, new Vector2(1,1) ),

			//BACK
			new VertexPositionColorNormal( new Vector3(-1f, 1f, -1f), Color.Green, Vector3.Forward, new Vector2(0,0) ),
			new VertexPositionColorNormal( new Vector3(1f, -1f, -1f), Color.Green, Vector3.Forward, new Vector2(1,1) ),
			new VertexPositionColorNormal( new Vector3(-1f, -1f, -1f), Color.Green, Vector3.Forward, new Vector2(0,1) ),
			new VertexPositionColorNormal( new Vector3(1f, 1f, -1f), Color.Green, Vector3.Forward, new Vector2(1,0) ),

			//LEFT
			new VertexPositionColorNormal( new Vector3(-1f, 1f, -1f), Color.Blue, Vector3.Left, new Vector2(0,0) ),
			new VertexPositionColorNormal( new Vector3(-1f, -1f, 1f), Color.Blue, Vector3.Left, new Vector2(1,1) ),
			new VertexPositionColorNormal( new Vector3(-1f, -1f, -1f), Color.Blue, Vector3.Left, new Vector2(1,0) ),
			new VertexPositionColorNormal( new Vector3(-1f, 1f, 1f), Color.Blue, Vector3.Left, new Vector2(0,1) ),

			//RIGHT
			new VertexPositionColorNormal( new Vector3(1f, 1f, -1f), Color.Cyan, Vector3.Right, new Vector2(1,0) ),
			new VertexPositionColorNormal( new Vector3(1f, -1f, 1f), Color.Cyan, Vector3.Right, new Vector2(0,1) ),
			new VertexPositionColorNormal( new Vector3(1f, -1f, -1f), Color.Cyan, Vector3.Right, new Vector2(0,0) ),
			new VertexPositionColorNormal( new Vector3(1f, 1f, 1f), Color.Cyan, Vector3.Right, new Vector2(1,1) ),

			//TOP
			new VertexPositionColorNormal( new Vector3(-1f, 1f, 1f), Color.Magenta, Vector3.Up, new Vector2(1,1) ),
			new VertexPositionColorNormal( new Vector3(1f, 1f, -1f), Color.Magenta, Vector3.Up, new Vector2(0,0) ),
			new VertexPositionColorNormal( new Vector3(-1f, 1f, -1f), Color.Magenta, Vector3.Up, new Vector2(1,0) ),
			new VertexPositionColorNormal( new Vector3(1f, 1f, 1f), Color.Magenta, Vector3.Up, new Vector2(0,1) ),

			//BOTTOM
			new VertexPositionColorNormal( new Vector3(-1f, -1f, 1f), Color.Yellow, Vector3.Down, new Vector2(0,1) ),
			new VertexPositionColorNormal( new Vector3(1f, -1f, -1f), Color.Yellow, Vector3.Down, new Vector2(1,0) ),
			new VertexPositionColorNormal( new Vector3(-1f, -1f, -1f), Color.Yellow, Vector3.Down, new Vector2(0,0) ),
			new VertexPositionColorNormal( new Vector3(1f, -1f, 1f), Color.Yellow, Vector3.Down, new Vector2(1,1) ),
		};

	private int[] indices = {
			//FRONT
			//triangle 1
			0, 1, 2,
			//triangle 2
			0, 3, 1,
			
			//BACK
			//triangle 1
			4, 6, 5,
			//triangle 2
			4, 5, 7,
			
			//LEFT
			//triangle 1
			8, 9, 10,
			//triangle 2
			8, 11, 9,

			//RIGHT
			//triangle 1
			12, 14, 13,
			//triangle 2
			12, 13, 15,

			//TOP
			//triangle 1
			16, 18, 17,
			//triangle 2
			16, 17, 19,

			//BOTTOM
			//triangle 1
			20, 21, 22,
			//triangle 2
			20, 23, 21
		};

	private Effect myEffect;
	private Texture2D crateTexture, crateNormal;
	Vector3 lightPosition = Vector3.Right * 2 + Vector3.Up * 2 + Vector3.Backward * 2;

	public override void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
	{
		myEffect = Content.Load<Effect>(ToString());
		crateTexture = Content.Load<Texture2D>("avatars-000012910459-m8gz6m-t500x500");
		crateNormal = Content.Load<Texture2D>("normaltim2");
	}

	public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
	{
		GraphicsDevice device = graphics.GraphicsDevice;

		float time = (float)gameTime.TotalGameTime.TotalSeconds;
		lightPosition = new Vector3(MathF.Cos(time) * 2, 1.5f, MathF.Sin(time) * 2);

		Vector3 cameraPos = -Vector3.Forward * 5 + Vector3.Up * 5 + Vector3.Right * 5;

		Matrix World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
		//Matrix World = Matrix.Identity * Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds * 1f) * Matrix.CreateRotationX((float)gameTime.TotalGameTime.TotalSeconds / 2f);

		Matrix View = Matrix.CreateLookAt(cameraPos, Vector3.Zero, Vector3.Up);

		Matrix Projection = Matrix.CreatePerspectiveFieldOfView((MathF.PI / 180f) * 40f, device.Viewport.AspectRatio, 0.001f, 100f);

		myEffect.Parameters["World"].SetValue(World);
		myEffect.Parameters["View"].SetValue(View);
		myEffect.Parameters["Projection"].SetValue(Projection);

		myEffect.Parameters["MainTex"].SetValue(crateTexture);
		myEffect.Parameters["NormalTex"].SetValue(crateNormal);

		myEffect.Parameters["cameraPosition"].SetValue(cameraPos);
		myEffect.Parameters["lightPosition"].SetValue(lightPosition);

		myEffect.CurrentTechnique.Passes[0].Apply();

		device.Clear(Color.Black);
		device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
	}
}