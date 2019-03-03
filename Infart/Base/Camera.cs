﻿#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace fge
{
    public class Camera
    {
        #region Dichiarazioni

        public Vector2 Position = Vector2.Zero;

        // Calcolata nello zoom!
        public int ViewPortWidth;
        public int ViewPortHeight;

        private float zoom_ = 1.0f;
        private Matrix transform_;
        private float rotation_ = 0.0f;

        private float moving_amount = 150.0f;
        private Vector2 velocity_ = Vector2.Zero;
        private Vector2 position_to_go_to_;
        private float intorno_uguaglianza_ = 10.0f;
        private bool moving_ = false;

        #endregion

        #region Costruttore

        public Camera(Vector2 starting_position, Vector2 ViewPortSize, float zoom)
        {
            Position = starting_position;

            zoom_ = zoom;

            ViewPortWidth = 1000;//(int)(ViewPortSize.X + ViewPortSize.X * (1-zoom_));
            ViewPortHeight = 600; // (int)(ViewPortSize.Y + ViewPortSize.Y * (1 - zoom_));
        }

        public void Reset(Vector2 position)
        {
            Position = position;
            moving_ = false;
        }

        #endregion

        #region Proprietà

        public float Zoom
        {
            get { return zoom_; }
            set 
            {
                zoom_ = value;
                if (zoom_ < 0.1f) zoom_ = 0.1f;
            }
        }

        public float Rotation
        {
            get { return rotation_; }
            set { rotation_ = value; }
        }

        #endregion

        #region Metodi pubblici

        public void MoveTo(Vector2 where)
        {
            position_to_go_to_ = where;

            int x_dir;
            if (Position.X < where.X)
                x_dir = +1;
            else if (Position.X > where.X)
                x_dir = -1;
            else x_dir = 0;

            int y_dir;
            if (Position.Y < where.Y)
                y_dir = +1;
            else if (Position.Y > where.Y)
                y_dir = -1;
            else y_dir = 0;

            velocity_ = new Vector2(
                moving_amount * x_dir,
                moving_amount * y_dir);

            moving_ = true;
        }

        public Vector2 ScreenToWorld(Vector2 pos)
        {
            return Vector2.Transform(pos, Matrix.Invert(transform_));
        }

        public Vector2 WorldToScreen(Vector2 pos)
        {
            return Vector2.Transform(pos, transform_);
        }

        public Matrix GetTransformation()
        {
            transform_ =
              Matrix.CreateTranslation(
                    new Vector3(-Position.X, -Position.Y, 0)) *
                    Matrix.CreateRotationZ(rotation_) *
                    Matrix.CreateScale(new Vector3(Zoom, Zoom, 1));
            return transform_;
        }

        #endregion

        #region Update

        public void Update(double gametime)
        {
            if (moving_)
            {
                // Probabilmente non saranno mai uguali, devo stabilire un intorno
                if (position_to_go_to_ != Position)
                {
                    float elapsed = (float)gametime / 1000.0f;
                    Position += velocity_ * elapsed;

                    if (Math.Abs(Position.X - position_to_go_to_.X) < intorno_uguaglianza_)
                    {
                        Position.X = position_to_go_to_.X;
                        velocity_.X = 0.0f;
                    }

                    if (Math.Abs(Position.Y - position_to_go_to_.Y) < intorno_uguaglianza_)
                    {
                        Position.Y = position_to_go_to_.Y;
                        velocity_.Y = 0.0f;
                    }
                }
                else
                    moving_ = false;
            }
        }

        #endregion
    }
}
