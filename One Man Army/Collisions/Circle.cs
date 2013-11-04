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
    public class Circle : CollisionObject
    {
        #region Fields

        internal float radius;

        public override float Radius
        {
            get { return radius; }
        }

        #endregion

        #region Initialization

        public Circle(Vector2 position, float radius)
            : base()
        {
            this.radius = radius;
            this.position = position;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the AABB of the current coordinates in world space.
        /// </summary>
        public override void GetBoundingBox()
        {
            float x1 = position.X - radius;
            float y1 = position.Y - radius;
            float x2 = position.X + radius;
            float y2 = position.Y + radius;

            boundingBox.X = (int)x1;
            boundingBox.Y = (int)y1;
            boundingBox.Width = (int)(x2 - x1);
            boundingBox.Height = (int)(y2 - y1);
        }

        #endregion
    }
}
