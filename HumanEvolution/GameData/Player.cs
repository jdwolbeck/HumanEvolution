using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Player
{
    public Player()
    { }

    //Returns true if this function is handling all inputs
    public bool HandleInput(InputState inputState, ref GameData gameData)
    {
        if (inputState.CurrentMouseState.LeftButton == ButtonState.Pressed)
        {
            if (gameData.MiniMap.BackgroundTargetRectangle.Contains(inputState.CurrentMouseState.Position))
            {
                Vector2 miniMapToRealMapPos = gameData.MiniMap.GetNewCameraPositionForPoint(inputState.CurrentMouseState.Position);
                Global.Camera.CenterOn(miniMapToRealMapPos);

                return true;
            }
        }

        MouseState mouseState;
        if (inputState.IsNewLeftMouseClick(out mouseState))
        {
            //Vector2 worldPosition = Vector2.Transform(new Vector2(mouseState.Position.X, mouseState.Position.Y), Matrix.Invert(Global.Camera.TranslationMatrix));
            ////gameData.Animations.Add(gameData.OnClickAnimFactory.Build(new Point((int)worldPosition.X, (int)worldPosition.Y)));

            //for(int i = 0; i < gameData.Sprites.Count; i++)
            //{
            //    if(gameData.Sprites[i].GetType() == typeof(Animal))
            //    {
            //        ((Animal)gameData.Sprites[i]).Path.Add(Animal.PositionToRectangle(worldPosition));
            //        ((Animal)gameData.Sprites[i]).IsMoving = true;
            //    }
            //}
        }

        return false;
    }
}