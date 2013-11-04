using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DataTypeLibrary
{
    public class HeliData
    {
        public float Acceleration;
        public float MaxHealth;
        public float MaxMoveSpeed;
        public float MaxVerticalSpeed;
        public List<Vector2> Polygon;
        public float MaxOverheatTime;
        public float OverheatFactor;
    }
}
