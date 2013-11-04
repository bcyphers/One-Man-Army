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
    public class WeaponData
    {
        public string Name;
        public float FireRate;
        public float Accuracy;
        public float Velocity;
        public float Damage;
        public int NumBullets;
        public int AmmoPerClip;
        public int MovementType;
        public float BulletRadius;
        public float DamageRadius;
        public float AnimationLength;
        public int IsBulletTrail;
        public int SoundFrequency;

        public WeaponData()
        {
        }
    }
}
