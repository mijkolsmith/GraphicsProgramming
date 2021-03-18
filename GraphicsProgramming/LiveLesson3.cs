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
	Vector3 lightPosition = Vector3.Right * 2 + Vector3.Up * 2 + Vector3.Backward * 2;

	Model sphere, cube;
	Texture2D day, night, clouds, moon, sun;
	TextureCube sky;

	float yaw, pitch, zoom = 30f;
	int prevX, prevY, prevZoom;

	int planet = 1;
	bool left, right;

	Matrix earthPos, sunPos, moonPos;

	public override void Update(GameTime gameTime)
	{
		MouseState mState = Mouse.GetState();
		KeyboardState keyState = Keyboard.GetState();

		if (mState.LeftButton == ButtonState.Pressed)
		{
			//update yaw & pitch
			yaw -= (mState.X - prevX) * 0.01f;
			pitch -= (mState.Y - prevY) * 0.01f;

			pitch = MathF.Min(Math.Max(pitch, -MathF.PI / 2.00001f), MathF.PI / 2.00001f);
		}

		zoom -= (mState.ScrollWheelValue - prevZoom) * 0.01f;

		if (keyState.IsKeyDown(Keys.Left) && !left)
		{
			left = true;
			if (planet <= 1)
			{
				planet = 3;
			}
			else 
			{
				planet--;
			}
		}
		else if(keyState.IsKeyUp(Keys.Left))
		{
			left = false;
		}

		if (keyState.IsKeyDown(Keys.Right) && !right)
		{
			right = true;
			if (planet >= 3)
			{
				planet = 1;
			}
			else 
			{ 
				planet++;
			}
		}
		else if (keyState.IsKeyUp(Keys.Right))
		{
			right = false;
		}

		if (keyState.IsKeyDown(Keys.NumPad1))
		{
			planet = 1;
		}
		else if (keyState.IsKeyDown(Keys.NumPad2))
		{
			planet = 2;
		}
		else if (keyState.IsKeyDown(Keys.NumPad3))
		{
			planet = 3;
		}

		if (keyState.GetPressedKeyCount() > 0)
		{
			switch (planet)
			{
				case 1:
					zoom = 50;
					break;
				case 2:
					zoom = 10;
					break;
				case 3:
					zoom = 5;
					break;
			}
		}


		Console.WriteLine(planet);
		prevZoom = mState.ScrollWheelValue;
		prevX = mState.X;
		prevY = mState.Y;
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
		lightPosition = Vector3.Zero; //new Vector3(MathF.Cos(time), 0, MathF.Sin(time)) * 200;
		
		//World matrix
		Matrix World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

		//Planet locations
		sunPos = Matrix.CreateScale(0.1f) * Matrix.CreateRotationZ(-time / 100 /* 25*4 */);
		earthPos = Matrix.CreateRotationZ(time / 4) * Matrix.CreateTranslation(Vector3.Down * 40) * Matrix.CreateScale(0.01f) * Matrix.CreateRotationZ(time / 1460 /* 365*4 */) * Matrix.CreateRotationY(MathF.PI / 180 * .23f);
		moonPos = Matrix.CreateScale(0.33f) * Matrix.CreateTranslation(Vector3.Down * 10) * Matrix.CreateRotationZ(time / 4 - time * .03333333f) * Matrix.CreateTranslation(Vector3.Down * 40) * Matrix.CreateScale(0.01f) * Matrix.CreateRotationZ(time / 1460);

		//camerapos
		Vector3 cameraPos = -Vector3.Forward * zoom;// + Vector3.Up * 5 + Vector3.Right * 5;
		cameraPos = Vector3.Transform(cameraPos, Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0));

		//View matrix
		Matrix View = Matrix.CreateLookAt(cameraPos, Vector3.Zero, Vector3.Up);
		if (planet == 1)
		{
			cameraPos = Vector3.Transform(cameraPos, Matrix.CreateTranslation(Matrix.Invert(sunPos).Translation));
			View = Matrix.CreateLookAt(cameraPos, Matrix.Invert(sunPos).Translation, Vector3.Up);
		}
		else if (planet == 2)
		{
			cameraPos = Vector3.Transform(cameraPos, Matrix.CreateTranslation(Matrix.Invert(earthPos).Translation));
			View = Matrix.CreateLookAt(cameraPos, Matrix.Invert(earthPos).Translation, Vector3.Up);
		}
		else if (planet == 3)
		{
			cameraPos = Vector3.Transform(cameraPos, Matrix.CreateTranslation(Matrix.Invert(moonPos).Translation));
			View = Matrix.CreateLookAt(cameraPos, Matrix.Invert(moonPos).Translation, Vector3.Up);
		}

		//Projection matrix
		Matrix Projection = Matrix.CreatePerspectiveFieldOfView((MathF.PI / 180f) * 40f, device.Viewport.AspectRatio, 0.1f, 1000f);

		myEffect.Parameters["World"].SetValue(World);
		myEffect.Parameters["View"].SetValue(View);
		myEffect.Parameters["Projection"].SetValue(Projection);

		myEffect.Parameters["DayTex"].SetValue(day);
		myEffect.Parameters["NightTex"].SetValue(night);
		myEffect.Parameters["CloudsTex"].SetValue(clouds);
		myEffect.Parameters["MoonTex"].SetValue(moon);
		myEffect.Parameters["SunTex"].SetValue(sun);

		myEffect.Parameters["Time"].SetValue(time);

		//myEffect.Parameters["SkyTex"].SetValue(sky);

		myEffect.Parameters["cameraPosition"].SetValue(cameraPos);
		myEffect.Parameters["lightPosition"].SetValue(lightPosition);

		myEffect.CurrentTechnique.Passes[0].Apply();

		device.Clear(Color.Black);

		//Sun
		myEffect.CurrentTechnique = myEffect.Techniques["Sun"];
		RenderModel(sphere, sunPos * World);

		//Earth
		myEffect.CurrentTechnique = myEffect.Techniques["Earth"];
		RenderModel(sphere, earthPos * World);

		//Moon
		myEffect.CurrentTechnique = myEffect.Techniques["Moon"];
		RenderModel(sphere, moonPos * World);

		//Skybox
		myEffect.CurrentTechnique = myEffect.Techniques["Sky"];
		device.DepthStencilState = DepthStencilState.None;
		device.RasterizerState = RasterizerState.CullNone;
		RenderModel(cube, Matrix.CreateTranslation(cameraPos) * Matrix.CreateScale(20000));
		device.RasterizerState = RasterizerState.CullCounterClockwise;
		device.DepthStencilState = DepthStencilState.Default;
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