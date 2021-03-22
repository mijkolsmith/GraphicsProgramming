using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

class LiveLesson3 : Lesson
{
	private Effect myEffect;
	Vector3 lightPosition = Vector3.Zero;

	Model sphere, cube;
	Texture2D day, night, clouds, moon, sun;
	TextureCube sky;

	Matrix earthPos, sunPos, moonPos;

	Vector3 cameraPos = Vector3.Backward * 50;
	Quaternion cameraRotation = Quaternion.Identity;
	private int mouseX, mouseY;
	float yaw, pitch;

	public override void Initialize()
	{
		mouseX = Mouse.GetState().X;
		mouseY = Mouse.GetState().Y;
	}

	public override void Update(GameTime gameTime)
	{
		float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
		float speed = 20;

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

		if (mState.LeftButton == ButtonState.Pressed)
		{
			yaw -= deltaX * sensitivity;
			pitch -= deltaY * sensitivity;
		}

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

	public override void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
	{
		myEffect = Content.Load<Effect>(ToString());

		day = Content.Load<Texture2D>("day");
		night = Content.Load<Texture2D>("night");
		clouds = Content.Load<Texture2D>("clouds");
		moon = Content.Load<Texture2D>("2k_moon");
		sun = Content.Load<Texture2D>("sun");

		sky = Content.Load<TextureCube>("sky_cube");

		sphere = Content.Load<Model>("uv_sphere");
		foreach (ModelMesh mesh in sphere.Meshes)
		{
			foreach (ModelMeshPart meshPart in mesh.MeshParts)
			{
				meshPart.Effect = myEffect;
			}
		}

		cube = Content.Load<Model>("cube");
		foreach (ModelMesh mesh in cube.Meshes)
		{
			foreach (ModelMeshPart meshPart in mesh.MeshParts)
			{
				meshPart.Effect = myEffect;
			}
		}
	}

	public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
	{
		GraphicsDevice device = graphics.GraphicsDevice;

		float time = (float)gameTime.TotalGameTime.TotalSeconds;

		//World matrix
		Matrix World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
		//View matrix
		Matrix View = Matrix.CreateLookAt(cameraPos, cameraPos + Vector3.Transform(Vector3.Forward, cameraRotation), Vector3.Transform(Vector3.Up, cameraRotation));
		//Projection matrix
		Matrix Projection = Matrix.CreatePerspectiveFieldOfView((MathF.PI / 180f) * 40f, device.Viewport.AspectRatio, 0.1f, 1000f);

		//Planet locations
		sunPos = Matrix.CreateScale(0.1f) * Matrix.CreateRotationZ(-time / 100 /* 25*4 */) * World;
		earthPos = Matrix.CreateRotationZ(time / 4) * Matrix.CreateTranslation(Vector3.Down * 40) * Matrix.CreateScale(0.01f) * Matrix.CreateRotationZ(time / 1460 /* 365*4 */) * Matrix.CreateRotationY(MathF.PI / 180 * .23f) * World;
		moonPos = Matrix.CreateScale(0.33f) * Matrix.CreateTranslation(Vector3.Down * 10) * Matrix.CreateRotationZ(time / 4 - time * .03333333f) * Matrix.CreateTranslation(Vector3.Down * 40) * Matrix.CreateScale(0.01f) * Matrix.CreateRotationZ(time / 1460) * World;
		
		myEffect.Parameters["World"].SetValue(World);
		myEffect.Parameters["View"].SetValue(View);
		myEffect.Parameters["Projection"].SetValue(Projection);

		myEffect.Parameters["DayTex"].SetValue(day);
		myEffect.Parameters["NightTex"].SetValue(night);
		myEffect.Parameters["CloudsTex"].SetValue(clouds);
		myEffect.Parameters["MoonTex"].SetValue(moon);
		myEffect.Parameters["SunTex"].SetValue(sun);

		myEffect.Parameters["Time"].SetValue(time);

		myEffect.Parameters["SkyTex"].SetValue(sky);

		myEffect.Parameters["cameraPosition"].SetValue(cameraPos);
		myEffect.Parameters["lightPosition"].SetValue(lightPosition);

		myEffect.CurrentTechnique.Passes[0].Apply();

		device.Clear(Color.Black);

		//Skybox
		myEffect.CurrentTechnique = myEffect.Techniques["Sky"];
		device.DepthStencilState = DepthStencilState.None;
		device.RasterizerState = RasterizerState.CullNone;
		RenderModel(cube, Matrix.CreateTranslation(cameraPos));
		device.RasterizerState = RasterizerState.CullCounterClockwise;
		device.DepthStencilState = DepthStencilState.Default;

		//Sun
		myEffect.CurrentTechnique = myEffect.Techniques["Sun"];
		RenderModel(sphere, sunPos);

		//Earth
		myEffect.CurrentTechnique = myEffect.Techniques["Earth"];
		RenderModel(sphere, earthPos);

		//Moon
		myEffect.CurrentTechnique = myEffect.Techniques["Moon"];
		RenderModel(sphere, moonPos );
	}

	void RenderModel(Model m, Matrix parentMatrix )
	{
		Matrix[] transforms = new Matrix[m.Bones.Count];
		m.CopyAbsoluteBoneTransformsTo(transforms);

		myEffect.CurrentTechnique.Passes[0].Apply();

		foreach(ModelMesh mesh in m.Meshes)
		{
			myEffect.Parameters["World"].SetValue(parentMatrix * transforms[mesh.ParentBone.Index]);

			mesh.Draw();
		}
	}
}