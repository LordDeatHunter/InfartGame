using Infart.Astronaut;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Infart.Drawing
{
    public abstract class AnimatedGameObject : GameObject
    {
        protected Dictionary<string, AnimationManager> animations_ = new Dictionary<string, AnimationManager>();

        protected string current_animation_;

        protected Rectangle collision_rectangle_;

        protected int current_frame_width_;

        protected int current_frame_height_;

        public AnimatedGameObject()
        {
            collision_rectangle_ = Rectangle.Empty;
        }

        public override void Dispose()
        {
            foreach (KeyValuePair<string, AnimationManager> i in animations_)
                animations_[i.Key].Dispose();
        }

        public override Rectangle CollisionRectangle
        {
            get { return collision_rectangle_; }
        }

        public int Width
        {
            get { return current_frame_width_; }
        }

        public int Height
        {
            get { return current_frame_height_; }
        }

        public override Vector2 Position
        {
            get
            {
                return position_;
            }
            set
            {
                collision_rectangle_.X = (int)value.X;
                collision_rectangle_.Y = (int)value.Y;
                position_ = value;
            }
        }

        private void UpdateAnimation(double gameTime)
        {
            if (animations_.ContainsKey(current_animation_))
            {
                if (animations_[current_animation_].FinishedPlaying)
                {
                    PlayAnimation(animations_[current_animation_].NextAnimation);
                }
                else
                {
                    animations_[current_animation_].Update(gameTime);
                }
            }
        }

        public void PlayAnimation(string name)
        {
            if (!(name == null) && animations_.ContainsKey(name))
            {
                current_animation_ = name;
                animations_[name].Play();
                current_frame_height_ = (int)animations_[current_animation_].FrameHeight;
                current_frame_width_ = (int)animations_[current_animation_].FrameWidth;
                collision_rectangle_.Width = current_frame_width_;
                collision_rectangle_.Height = current_frame_height_;
            }
        }

        public override void Update(double gameTime)
        {
            UpdateAnimation(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (animations_.ContainsKey(current_animation_))
            {
                spriteBatch.Draw(
                       animations_[current_animation_].Texture,
                       position_,
                       animations_[current_animation_].FrameRectangle,
                       overlay_color_,
                       rotation_,
                       origin_,
                       scale_,
                       flip_,
                       depth_);
            }
        }
    }
}
