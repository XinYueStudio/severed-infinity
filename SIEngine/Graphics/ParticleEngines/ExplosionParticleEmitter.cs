﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIEngine.BaseGeometry;
using SIEngine.GUI;
using SIEngine.Other;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Timer = System.Windows.Forms.Timer;
using Color = System.Drawing.Color;
using Object = SIEngine.GUI.Object;

namespace SIEngine.Graphics.ParticleEngines
{
    public class ExplosionParticleEmitter : ParticleEmitter
    {
        #region Fields and Properties
        //properties
        public float Scale { get; set; }
        public Vector Gravity { get; set; }
        public float ExplosionDuration { get; set; }
        public float FadeOutDuration { get; set; }
        public Color StartingColor { get; set; }
        public Color EndColor { get; set; }
        public Color SmokeColor { get; set; }
        protected List<RectangleParticle> Particles { get; set; }

        //fields
        protected float speed = 0.55f;
        protected Vector particleSize = new Vector(0.5f, 0.5f);
        protected Vector particleSizeIncrease = new Vector(1.0f, 1.0f);
        protected Vector sizeIncreseShift = new Vector(-0.5f, -0.5f, 0.0f);
        protected Texture defaultTexture = new Texture("data/img/exp1.png");

        #endregion

        public override void SetInitialValues()
        {
            elapsedTime = 0;
            foreach (var particle in Particles)
            {
                //Here we calculate the direction of our particle
                float vx = speed * GeneralMath.RandomFloat(-1.0f, 1.0f);
                float vy = speed * GeneralMath.RandomFloat(-1.0f, 1.0f);

                //next we should set the required values for the particle to work
                particle.Gravity = Gravity;
                particle.CurrentColor = StartingColor;
                particle.TargetColor = EndColor;
                particle.Velocity.X = vx;
                particle.Velocity.Y = vy;
                particle.Size.X = particleSize.X;
                particle.Size.Y = particleSize.Y;
                particle.Location.X = GeneralMath.RandomFloat(-1.0f, 1.0f);
                particle.Location.Y = GeneralMath.RandomFloat(-1.0f, 1.0f);
                particle.colorCoef = 0.0f;
                particle.ColorCoefIncrease = GeneralMath.RandomFloat(0.01f, 0.05f);
            }
        }

        public void AnimationStep(object sender, EventArgs evArgs)
        {
            if (Paused)
                return;

            elapsedTime++;
            if (ExplosionDuration + FadeOutDuration <= elapsedTime * MainTimer.Interval)
                Pause();

            foreach (var part in Particles)
            {
                if (elapsedTime * MainTimer.Interval == ExplosionDuration)
                {
                    part.TargetColor = SmokeColor;
                    part.ColorCoefIncrease = 0.03f;
                    part.Velocity.X *= 0.3f;
                    part.Velocity.Y = 0.1f;
                }
                part.Size += particleSizeIncrease;
                part.Location += sizeIncreseShift;
                part.AnimationStep(MainTimer.Interval * elapsedTime);
            }
        }

        public ExplosionParticleEmitter(int numParticles)
        {
            MaxParticleCount = numParticles;
            MainTimer = new Timer();
            MainTimer.Tick += AnimationStep;
            MainTimer.Interval = 10;
            Scale = 0.5f;
            Gravity = new Vector(0.0f, 0.0f, 0.0f);
            ExplosionDuration = 70;
            FadeOutDuration = 500;
            StartingColor = Color.FromArgb(255, Color.Orange);
            EndColor = Color.FromArgb(128, Color.Red);
            SmokeColor = Color.FromArgb(0, Color.Black);

            Particles = new List<RectangleParticle>();
            float z = 0.0f;
            for (int i = 0; i < MaxParticleCount; ++i)
            {
                //here we set the constant values of our particle
                var temp = new RectangleParticle(this);
                temp.AnimationTime = 10;
                temp.Location.Z = z;
                temp.Texture = defaultTexture;
                Particles.Add(temp);

                z += 0.1f;
            }
        }

        public override void Pause()
        {
            Paused = true;
            MainTimer.Stop();
        }
        public override void Start()
        {
            SetInitialValues();
            Paused = false;
            MainTimer.Start();
        }
        public override void Stop()
        {
        }

        public override void Draw()
        {
            if (Paused)
                return;

            GeneralGraphics.UseDefaultShaderProgram();
            GeneralGraphics.EnableAlphaBlending();
            GeneralGraphics.EnableTexturing();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            {
                GL.Translate(Location.X, Location.Y, Location.Z);
                GL.Scale(Scale, Scale, Scale);
                foreach (var part in Particles)
                    part.Draw();
            }
            GL.PopMatrix();

            GeneralGraphics.DisableTexturing();
            GeneralGraphics.DisableBlending();
        }
    }
}
