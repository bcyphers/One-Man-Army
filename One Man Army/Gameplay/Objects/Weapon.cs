using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using DataTypeLibrary;

namespace One_Man_Army
{
    public enum WeaponState
    {
        Inactive,
        ActiveOnMap,
        HeldByPlayer
    }

    public class Weapon
    {
        private WeaponData data;

        public string Name;
        public float FireRate;
        public float Accuracy;
        public float Velocity;
        public float Damage;
        public int NumBullets;
        public int AmmoPerClip;
        public BulletMovementType MovementType;
        public Texture2D WeaponTexture;
        public Texture2D ArmsTexture;
        public Texture2D BulletTexture;
        public Texture2D FireAnimation;
        public Texture2D FireTextParticle;
        public float BulletRadius;
        public float DamageRadius;
        public float AnimationLength;
        public bool IsBulletTrail;
        public int SoundFrequency;

        public Level Level;
        public WeaponState State = WeaponState.Inactive;
        public int Ammo = 0;
        int soundCounter = 0;

        public static int NumWeapons = 13;
        public static int NumTiers = 3;

        public Weapon(WeaponData dat)
        {
            data = dat;

            Accuracy = data.Accuracy;
            AmmoPerClip = data.AmmoPerClip;
            AnimationLength = data.AnimationLength;
            BulletRadius = data.BulletRadius;
            Damage = data.Damage;
            DamageRadius = data.DamageRadius;
            FireRate = data.FireRate;
            Name = data.Name;
            NumBullets = data.NumBullets;
            Velocity = data.Velocity;
            MovementType = (BulletMovementType)data.MovementType;
            IsBulletTrail = data.IsBulletTrail == 1 ? true : false;
            SoundFrequency = data.SoundFrequency;

        }

        public void Activate()
        {
            Ammo = AmmoPerClip;
            State = WeaponState.HeldByPlayer;
        }

        /// <summary>
        /// Deactivates a weapon when it runs out of ammo.
        /// </summary>
        public void Deactivate()
        {
            Ammo = 0;
            State = WeaponState.Inactive;
        }

        /// <summary>
        /// Returns a copy of the weapon, one with the same data, but which will act
        /// as a separate entity in the level.
        /// </summary>
        public Weapon Copy()
        {
            Weapon weapon = new Weapon(data);

            weapon.ArmsTexture = ArmsTexture;
            weapon.BulletTexture = BulletTexture;
            weapon.FireAnimation = FireAnimation;
            weapon.WeaponTexture = WeaponTexture;

            return weapon;
        }

        /// <summary>
        /// Creates and adds bullets, based on the weapon's characteristics.
        /// </summary>
        /// <param name="pos">where to add the bullets</param>
        /// <param name="dir">what direction the bullets will be travelling</param>
        /// <param name="level">the level to add the bullets to</param>
        /// <returns>an array of bullets to be added to the level</returns>
        public Bullet[] Fire(Vector2 pos, Vector2 dir, Level level, bool friendly)
        {
            if (level.GetCollision(pos) == TileCollision.Impassable)
                return null;
            
            if (AmmoPerClip != 0)
                Ammo--;

            Bullet[] Bullets = new Bullet[NumBullets];

            float maxDeviation = (float)((1 - Accuracy) * MathHelper.PiOver4);
            int halfOfSpread = (int)(NumBullets / 2);

            soundCounter--;
            if (soundCounter <= 0)
            {
                float scale = ((float)Math.Pow(BulletRadius / 8, 0.75) + (float)Math.Pow(FireRate * 1.5, 0.67)
                    + (float)Math.Pow(Damage * NumBullets * SoundFrequency * 5, 0.5)) / 3;
                level.WeaponText.AddParticles(pos, scale, FireTextParticle);
                soundCounter = SoundFrequency;
            }

            for (int i = 0; i < NumBullets; i++)
            {
                Vector2 direction = Vector2.Transform(dir, Matrix.CreateRotationZ(
                    (i - halfOfSpread) * maxDeviation * 2));

                direction = Vector2.Transform(direction, Matrix.CreateRotationZ(
                    (float)(level.Random.NextDouble() * maxDeviation * 2) - maxDeviation));

                Bullet bullet = level.BulletQueue.Dequeue();
                bullet.initBullet(pos, direction, Velocity, 
                    BulletRadius, Damage, DamageRadius, 
                    MovementType, BulletTexture, friendly,
                    IsBulletTrail);

                if (IsBulletTrail)
                    bullet.SmokeTrail = new SmokeEmitterInstance(level.SmokeTrails, pos, BulletRadius);

                Bullets[i] = bullet;
            }

            level.Screen.SFXManager.PlayCue(this.Name + " Shot");

            if (Ammo <= 0 && AmmoPerClip != 0)
                Deactivate();

            return Bullets;
        }
    }
}
