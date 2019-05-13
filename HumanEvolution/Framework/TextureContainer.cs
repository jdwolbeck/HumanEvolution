using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TextureContainer
{
    public Texture2D WhitePixel { get; set; }
    public Texture2D ParticleCollisionWestTopSpriteSheet { get; set; }
    public Texture2D ParticleCollisionWestBottomSpriteSheet { get; set; }
    public Texture2D ParticleCollisionEastTopSpriteSheet { get; set; }
    public Texture2D ParticleCollisionEastBottomSpriteSheet { get; set; }
    public Texture2D ParticleCollisionNorthLeftSpriteSheet { get; set; }
    public Texture2D ParticleCollisionNorthRightSpriteSheet { get; set; }
    public Texture2D ParticleCollisionSouthLeftSpriteSheet { get; set; }
    public Texture2D ParticleCollisionSouthRightSpriteSheet { get; set; }
    public Texture2D ClickExplosionSpriteSheet { get; set; }

    public TextureContainer()
    {
    }
}