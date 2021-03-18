using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


class LiveLesson4 : Lesson
{
	private Effect effect;
	private Texture2D heightmap, dirt, water, grass, rock, snow, plant;
	private Model cube, sphere;

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Vert : IVertexType
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector3 Binormal;
		public Vector3 Tangent;
		public Vector2 Texture;

		static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration
		(
			new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
			new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
			new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
			new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
			new VertexElement(48, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
		);

		VertexDeclaration IVertexType.VertexDeclaration => _vertexDeclaration;


		public Vert(Vector3 position, Vector3 normal, Vector3 binormal, Vector3 tangent, Vector2 texture)
		{
			Position = position;
			Normal = normal;
			Binormal = binormal;
			Tangent = tangent;
			Texture = texture;
		}
	}

	private Vert[] vertices;
	private int[] indices;

	private int mouseX, mouseY;

	static float heightMod = 400f;
	Vector3 cameraPos = Vector3.Up * heightMod + Vector3.Right * 1000 - Vector3.Forward * 2500;
	Quaternion cameraRotation = Quaternion.Identity;
	float yaw, pitch;
	RenderTarget2D rt;

	public override void Initialize()
	{
		mouseX = Mouse.GetState().X;
		mouseY = Mouse.GetState().Y;
	}

	public override void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
	{
		effect = Content.Load<Effect>("LiveLesson4");
		heightmap = Content.Load<Texture2D>("heightmap");
		
		water = Content.Load<Texture2D>("watertex");
		dirt = Content.Load<Texture2D>("dirt_diff");
		grass = Content.Load<Texture2D>("grasstex");
		rock = Content.Load<Texture2D>("rocktex");
		snow = Content.Load<Texture2D>("snowtex");

		plant = Content.Load<Texture2D>("plant");

		cube = Content.Load<Model>("cube");
		foreach (ModelMesh mesh in cube.Meshes)
		{
			foreach (ModelMeshPart meshPart in mesh.MeshParts)
			{
				meshPart.Effect = effect;
			}
		}

		sphere = Content.Load<Model>("uv_sphere");
		foreach (ModelMesh mesh in sphere.Meshes)
		{
			foreach (ModelMeshPart meshPart in mesh.MeshParts)
			{
				meshPart.Effect = effect;
			}
		}

		GeneratePlane();

		rt = new RenderTarget2D(graphics.GraphicsDevice, 
								graphics.PreferredBackBufferWidth, 
								graphics.PreferredBackBufferHeight,
								false, //no mipmaps
								graphics.PreferredBackBufferFormat, 
								graphics.PreferredDepthStencilFormat
								);
	}

	private void GeneratePlane()
	{
		float gridSize = 8.0f;
		//Get pixels
		Color[] pixels = new Color[heightmap.Width * heightmap.Height];
		heightmap.GetData<Color>(pixels);

		//Generate vertices & indices
		vertices = new Vert[pixels.Length]; //pixels.Length = heightmap.Width * heightmap.Height
		indices = new int[(heightmap.Width - 1) * (heightmap.Height - 1) * 6];

		//forloops
		for (int y = 0; y < heightmap.Height; y++)
		{
			for (int x = 0; x < heightmap.Width; x++)
			{
				int index = y * heightmap.Width + x;

				//smooth if not at edge
				float r = pixels[index].R / 255f;
				if (y < heightmap.Height - 2 && x < heightmap.Width - 2)
				{
					r += pixels[index + 1].R / 255f;
					r += pixels[index + heightmap.Width].R / 255f;
					r += pixels[index + heightmap.Width + 1].R / 255f;
					r *= .25f;
				}

				//add vertex for current pixel
				vertices[index] = new Vert(new Vector3(gridSize * x, r * heightMod, gridSize * y),
				Vector3.Up, Vector3.Up, Vector3.Up,
				new Vector2(x / (float)heightmap.Width, y / (float)heightmap.Height));

				//if not at edge
				if (y < heightmap.Height - 2 && x < heightmap.Width - 2)
				{
					//add indices for TWO triangles (bottom-right)
					int right = index + 1;                                  //y * heightmap.Width + (x + 1); 
					int bottom = (y + 1) * heightmap.Width + x;             // index + width
					int bottomRight = (y + 1) * heightmap.Width + (x + 1);  //index + width + 1

					//triangle 1
					indices[index * 6 + 0] = index;
					indices[index * 6 + 1] = bottomRight;
					indices[index * 6 + 2] = bottom;

					//triangle 2
					indices[index * 6 + 3] = index;
					indices[index * 6 + 4] = right;
					indices[index * 6 + 5] = bottomRight;
				}
			}
		}

		//Calculate normals
		for (int y = 0; y < heightmap.Height - 1; y++)
		{
			for (int x = 0; x < heightmap.Width - 1; x++)
			{
				int index = y * heightmap.Width + x;

				int right = y * heightmap.Width + x + 1;
				int bottom = (y + 1) * heightmap.Width + x;

				Vector3 vr = Vector3.Normalize(vertices[right].Position - vertices[index].Position);
				Vector3 vd = Vector3.Normalize(vertices[bottom].Position - vertices[index].Position);

				vertices[index].Normal = Vector3.Cross(vr, vd);
			}
		}
	}

	public override void Update(GameTime gameTime)
	{
		float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
		float speed = 200;

		KeyboardState keyState = Keyboard.GetState();

		if (keyState.IsKeyDown(Keys.LeftShift))
		{
			speed *= 3;
		}

		if (keyState.IsKeyDown(Keys.W))
		{
			cameraPos += delta * speed * Vector3.Transform(Vector3.Forward, cameraRotation);
		}
		else if (keyState.IsKeyDown(Keys.S))
		{
			cameraPos -= delta * speed * Vector3.Transform(Vector3.Forward, cameraRotation);
		}
		if (keyState.IsKeyDown(Keys.A))
		{
			cameraPos += delta * speed * Vector3.Transform(Vector3.Left, cameraRotation);
		}
		else if (keyState.IsKeyDown(Keys.D))
		{
			cameraPos += delta * speed * Vector3.Transform(Vector3.Right, cameraRotation);
		}
		if (keyState.IsKeyDown(Keys.Space))
		{
			cameraPos += delta * speed * Vector3.Transform(Vector3.Up, Quaternion.Identity);
		}
		else if (keyState.IsKeyDown(Keys.LeftControl))
		{
			cameraPos += delta * speed * Vector3.Transform(Vector3.Down, Quaternion.Identity);
		}

		MouseState mState = Mouse.GetState();
		int deltaX = mState.X - mouseX;
		int deltaY = mState.Y - mouseY;

		float sensitivity = 0.01f;

		yaw -= deltaX * sensitivity;
		pitch -= deltaY * sensitivity;

		pitch = Math.Clamp(pitch, -MathF.PI * .5f, MathF.PI * .5f);

		cameraRotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);

		mouseX = mState.X;
		mouseY = mState.Y;

		if (mState.RightButton == ButtonState.Pressed)
		{
			yaw = 0;
			pitch = 0;
		}
	}

	public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
	{
		GraphicsDevice device = graphics.GraphicsDevice;
		device.SetRenderTarget(rt);

		device.Clear(Color.Black);

		float r = (float)gameTime.TotalGameTime.TotalSeconds;

		// Build & Set Matrices
		Matrix World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
		Matrix View = Matrix.CreateLookAt(cameraPos, cameraPos + Vector3.Transform(Vector3.Forward, cameraRotation), Vector3.Transform(Vector3.Up, cameraRotation));
		Matrix Projection = Matrix.CreatePerspectiveFieldOfView((MathF.PI / 180f) * 65f, device.Viewport.AspectRatio, 0.01f, 2000f);

		effect.Parameters["World"].SetValue(World);
		effect.Parameters["View"].SetValue(View);
		effect.Parameters["Projection"].SetValue(Projection);

		// Lighting Parameters
		effect.Parameters["lightDirection"].SetValue(Vector3.Normalize(Vector3.Down + Vector3.Right * 2));
		effect.Parameters["ambient"].SetValue(new Vector3(.25f, .20f, .15f));
		effect.Parameters["cameraPosition"].SetValue(cameraPos);

		// Textures
		effect.Parameters["WaterTex"].SetValue(water);
		effect.Parameters["DirtTex"].SetValue(dirt);
		effect.Parameters["GrassTex"].SetValue(grass);
		effect.Parameters["RockTex"].SetValue(rock);
		effect.Parameters["SnowTex"].SetValue(snow);

		effect.Parameters["time"].SetValue(r);

		// Render Sky
		device.RasterizerState = RasterizerState.CullNone;
		device.DepthStencilState = DepthStencilState.None;
		effect.CurrentTechnique = effect.Techniques["SkyBox"];

		RenderModel(cube, Matrix.CreateTranslation(cameraPos));

		// Render Terrain
		device.RasterizerState = RasterizerState.CullCounterClockwise;
		device.DepthStencilState = DepthStencilState.Default;
		effect.CurrentTechnique = effect.Techniques["Terrain"];
		effect.Parameters["World"].SetValue(World);

		effect.CurrentTechnique.Passes[0].Apply();
		device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);

		device.SetRenderTarget(null);

		// Render rt to screenbuffer
		spriteBatch.Begin();
		spriteBatch.Draw(rt, Vector2.Zero, Color.White);
		spriteBatch.End();

		// Set rt as shader variable


		// Render Sphere
		//Alpha Blend
		/*device.BlendState = BlendState.AlphaBlend;
		effect.CurrentTechnique = effect.Techniques["UnlitTransparent"];
		effect.Parameters["GrassTex"].SetValue(plant);

		//render inside
		device.RasterizerState = RasterizerState.CullClockwise;
		RenderModel(sphere, World * Matrix.CreateTranslation(Vector3.Right * 1000 - Vector3.Forward * 2500 + Vector3.Up * 100));
		//render outside
		device.RasterizerState = RasterizerState.CullCounterClockwise;
		RenderModel(sphere, World * Matrix.CreateTranslation(Vector3.Right * 1000 - Vector3.Forward * 2500 + Vector3.Up * 100));
		*/

		//Alpha Test
		effect.CurrentTechnique = effect.Techniques["UnlitTransparent"];
		effect.Parameters["GrassTex"].SetValue(rt);

		device.DepthStencilState = DepthStencilState.Default;
		device.RasterizerState = RasterizerState.CullNone;
		RenderModel(sphere, World * Matrix.CreateTranslation(Vector3.Right * 1000 - Vector3.Forward * 2500 + Vector3.Up * 100));

		// Reset Device States
		device.BlendState = BlendState.Opaque;
		device.RasterizerState = RasterizerState.CullCounterClockwise;
	}

	void RenderModel(Model m, Matrix parentMatrix)
	{
		Matrix[] transforms = new Matrix[m.Bones.Count];
		m.CopyAbsoluteBoneTransformsTo(transforms);

		effect.CurrentTechnique.Passes[0].Apply();

		foreach (ModelMesh mesh in m.Meshes)
		{
			effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * parentMatrix);

			mesh.Draw();
		}
	}
}