using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace One_Man_Army
{
    public abstract class CollisionObject
    {
        internal Rectangle boundingBox = Rectangle.Empty;

        public Rectangle BoundingBox
        {
            get
            {
                GetBoundingBox();
                return boundingBox;
            }
        }

        public virtual float Radius
        {
            get { return (BoundingBox.Width + BoundingBox.Height) / 4; }
        }

        public virtual Vector2 Center
        {
            get { return BoundingBox.GetCenter(); }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                lastPosition = Position;
                position = value;
            }
        }
        protected Vector2 position;

        public Vector2 LastPosition
        {
            get { return lastPosition; }
        }
        protected Vector2 lastPosition;

        public float Orientation = 0;

        public bool IsColliding = false;


        public CollisionObject()
        {
        }

        public virtual void GetBoundingBox()
        {
        }
    }
}
