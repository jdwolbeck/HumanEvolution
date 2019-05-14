﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

/// <summary>
/// This is the main type for your game.
/// </summary>
public class Game1 : Game
{
    //Framework variables
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private InputState _inputState;
    private Player _player;
    private SpriteFont _diagFont;
    //Game variables
    private GameData _gameData;
    private Borders _borders;
    private Random _rand;
    private Thread _AiThread;
    private AiThread _AiThreadClass;
    List<int> _fpsTotals;
    private int _frames;
    private double _elapsedSeconds;
    private int _fps;
    private double _elapsedSecondsSinceTick;
    private double _elapsedTimeSinceFoodGeneration;
    private float _tickSeconds;
    private float _elapsedTicksSinceIntervalProcessing;
    //Constants
    private const int BORDER_WIDTH = 10;
    private const float TICKS_PER_SECOND = 30; //How many ticks per second we should have
    private const bool ENABLE_DEBUG = false;
    //Colors
    private Color MAP_COLOR = Color.AliceBlue;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferHeight = 900;
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.SynchronizeWithVerticalRetrace = false; //Unlock fps
        this.IsFixedTimeStep = false; //Unlock fps

        IsMouseVisible = true;

        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        base.Initialize();
    }
    protected override void LoadContent()
    {
        if (_AiThread != null && _AiThread.IsAlive)
        {
            _AiThread.Abort();
            Thread.Sleep(500); //Sleep for half a second and wait for the thread to end
        }

        //Load settings at the beginning
        _gameData = new GameData();
        _gameData.Settings = SettingsHelper.ReadSettings("Settings.json");

        Global.Camera.ViewportWidth = _graphics.GraphicsDevice.Viewport.Width;
        Global.Camera.ViewportHeight = _graphics.GraphicsDevice.Viewport.Height;
        Global.Camera.CenterOn(new Vector2(Global.Camera.ViewportWidth / 2, Global.Camera.ViewportHeight / 2));
        Global.Camera.Initialize(_gameData.Settings.WorldSize);

        //Init variables
        InitVariables();

        // Create a new SpriteBatch, which can be used to draw textures.
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //Load the Font from the Content object. Use the Content Pipeline under the  "Content" folder to add assets to game
        _diagFont = Content.Load<SpriteFont>("DiagnosticsFont");

        //Load in a simple white pixel
        Texture2D whitePixel = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        Color[] color = new Color[1];
        color[0] = Color.White;
        whitePixel.SetData(color);

        //_gameData.Textures.Add(new TextureContainer() { Texture = _whitePixel, Name = TextureName.WhitePixel }); //Adding texture when using the Container as a list
        _gameData.Textures.WhitePixel = whitePixel;
        _gameData.Textures.ParticleCollisionWestTopSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactWestTop");
        _gameData.Textures.ParticleCollisionWestBottomSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactWestBottom");
        _gameData.Textures.ParticleCollisionEastTopSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactEastTop");
        _gameData.Textures.ParticleCollisionEastBottomSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactEastBottom");
        _gameData.Textures.ParticleCollisionNorthLeftSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactNorthLeft");
        _gameData.Textures.ParticleCollisionNorthRightSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactNorthRight");
        _gameData.Textures.ParticleCollisionSouthLeftSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactSouthLeft");
        _gameData.Textures.ParticleCollisionSouthRightSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactSouthRight");
        _gameData.Textures.ClickExplosionSpriteSheet = Content.Load<Texture2D>(@"Animations\ClickExplosion1");
        _gameData.Textures.MiniMapFrame = Content.Load<Texture2D>(@"Minimap\MinimapFrame");
        _gameData.Textures.MiniMapBuildingTexture = Content.Load<Texture2D>(@"Minimap\MinimapBuilding");
        _gameData.Textures.MiniMapObjectDiamondTexture = Content.Load<Texture2D>(@"Minimap\MinimapMovingObjectDiamond");
        _gameData.Textures.MiniMapObjectCircleTexture = Content.Load<Texture2D>(@"Minimap\MinimapMovingObjectCircle");

        _rand = new Random();
        _gameData.CollisionAnimFactory = new CollisionParticleAnimationFactory(_gameData.Textures.ParticleCollisionEastBottomSpriteSheet, _gameData.Textures.ParticleCollisionEastTopSpriteSheet, _gameData.Textures.ParticleCollisionWestBottomSpriteSheet, _gameData.Textures.ParticleCollisionWestTopSpriteSheet, _gameData.Textures.ParticleCollisionNorthLeftSpriteSheet, _gameData.Textures.ParticleCollisionNorthRightSpriteSheet, _gameData.Textures.ParticleCollisionSouthLeftSpriteSheet, _gameData.Textures.ParticleCollisionSouthRightSpriteSheet, 4, 40);
        _gameData.OnClickAnimFactory = new OnClickAnimationFactory(_gameData.Textures.ClickExplosionSpriteSheet, 4, 30);

        //Generate the Map
        _borders = new Borders();
        _borders.Texture = whitePixel;
        _borders.LeftWall = new Vector2(0, 0);
        _borders.RightWall = new Vector2(_gameData.Settings.WorldSize, 0);
        _borders.TopWall = new Vector2(0, 0);
        _borders.BottomWall = new Vector2(0, _gameData.Settings.WorldSize);
        _borders.LeftWallRectangle = new Rectangle((int)_borders.LeftWall.X - BORDER_WIDTH, (int)_borders.LeftWall.Y, BORDER_WIDTH, _gameData.Settings.WorldSize + BORDER_WIDTH);
        _borders.RightWallRectangle = new Rectangle((int)_borders.RightWall.X, (int)_borders.RightWall.Y - BORDER_WIDTH, BORDER_WIDTH, _gameData.Settings.WorldSize + BORDER_WIDTH);
        _borders.TopWallRectangle = new Rectangle((int)_borders.TopWall.X - BORDER_WIDTH, (int)_borders.TopWall.Y - BORDER_WIDTH, _gameData.Settings.WorldSize + BORDER_WIDTH, BORDER_WIDTH);
        _borders.BottomWallRectangle = new Rectangle((int)_borders.BottomWall.X - BORDER_WIDTH, (int)_borders.BottomWall.Y, _gameData.Settings.WorldSize + (BORDER_WIDTH * 2), BORDER_WIDTH);

        //Initialize the Grid
        int gridWidth = (int)Math.Ceiling((double)_gameData.Settings.WorldSize / _gameData.Settings.GridCellSize);

        _gameData.MapGridData = new GridData[gridWidth, gridWidth];

        //Loop through grid and set Rectangle on each cell, named iterators x,y to help avoid confusion
        for (int y = 0; y < _gameData.MapGridData.GetLength(0); y++)
        {
            for (int x = 0; x < _gameData.MapGridData.GetLength(1); x++)
            {
                _gameData.MapGridData[x, y] = new GridData();
                _gameData.MapGridData[x, y].Sprites = new List<SpriteBase>();

                Rectangle rec = new Rectangle();
                rec.X = x * _gameData.Settings.GridCellSize;
                rec.Y = y * _gameData.Settings.GridCellSize;
                rec.Width = _gameData.Settings.GridCellSize;
                rec.Height = _gameData.Settings.GridCellSize;

                _gameData.MapGridData[x, y].CellRectangle = rec;
            }
        }

        _gameData.MiniMap = new MiniMap(_gameData.Textures.WhitePixel, _gameData.Textures.MiniMapFrame, 20);
        _gameData.MiniMap.SetPosition(_graphics.GraphicsDevice, new Vector2(_gameData.Settings.WorldSize, _gameData.Settings.WorldSize), Anchor.BottomRight, new Vector2(200, 200));

        _AiThreadClass = new AiThread(_gameData, _rand);
        _AiThread = new Thread(new ThreadStart(_AiThreadClass.Start));
        _AiThread.Start();

        //SpawnScenerioTestObjs();
        SpawnScenerioHumanObey();

        //for (int i = 0; i < 500; i++)
        //{
        //    SpawnTestObject();
        //}
        //for (int i = 0; i < 200; i++)
        //{
        //    SpawnSampleBuilding();
        //}
    }
    protected override void UnloadContent()
    {
        // TODO: Unload any non ContentManager content here
    }
    protected override void OnExiting(object sender, EventArgs args)
    {
        bool needSleep = false;

        if (_AiThread != null && _AiThread.IsAlive)
        {
            _AiThread.Abort();
            needSleep = true;
        }

        if (needSleep)
        {
            Thread.Sleep(500);
        }

        base.OnExiting(sender, args);
    }
    protected override void Update(GameTime gameTime)
    {
        bool tick = false;

        if (_inputState.IsExitGame(PlayerIndex.One))
        {
            Exit();
        }
        else
        {
            _inputState.Update();
            if(!_player.HandleInput(_inputState, ref _gameData)) //If the Player class returns false then pass the input on to the camera
                Global.Camera.HandleInput(_inputState, PlayerIndex.One, gameTime, ref _gameData);

            _elapsedSecondsSinceTick += gameTime.ElapsedGameTime.TotalSeconds;
            _elapsedTimeSinceFoodGeneration += gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedSecondsSinceTick > _tickSeconds)
            {
                _elapsedSecondsSinceTick = _elapsedSecondsSinceTick - _tickSeconds; //Start the next tick with the overage
                tick = true;
            }

            //During a tick do all creature processing
            if (tick)
            {
                UpdateTick(gameTime);
            }
            else //Off tick processing
            {
                UpdateOffTick(gameTime);
            }
        }

        //This must be after movement caluclations occur for the creatures otherwise the camera will glitch back and forth
        if (_gameData.Focus != null)
        {
            Global.Camera.CenterOn(_gameData.Focus.Position);
        }

        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        //FPS Logic
        if (_elapsedSeconds >= 1)
        {
            _fps = _frames;
            _frames = 0;
            _elapsedSeconds = 0;
            //_fpsTotals.Add(_fps);

            //TODO remove this later
            ////Used for Comparing FPS in Dev
            //if (gameTime.TotalGameTime.TotalSeconds > 30)
            //{
            //    double avg = _fpsTotals.Average();
            //}
        }
        _frames++;
        _elapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;

        GraphicsDevice.Clear(MAP_COLOR);

        //DRAW IN THE WORLD
        DrawWorldObjects();

        //DRAW HUD INFOMATION
        DrawHUD();

        base.Draw(gameTime);
    }

    //Private functions
    private void InitVariables()
    {
        _fpsTotals = new List<int>();
        _inputState = new InputState();
        _player = new Player();
        _fps = 0;
        _frames = 0;
        _elapsedSeconds = 0.0;
        _tickSeconds = 1 / TICKS_PER_SECOND;
    }

    //Update Tick functions
    private void UpdateTick(GameTime gameTime)
    {
        _elapsedTicksSinceIntervalProcessing++;

        UpdateTickSprites(gameTime);
        UpdateTickMiniMap(gameTime);
    }
    private void UpdateTickSprites(GameTime gameTime)
    {
        for (int i = _gameData.Sprites.Count - 1; i >= 0; i--)
        {
            _gameData.Sprites[i].UpdateTick(gameTime, ref _gameData);
            _gameData.Sprites[i].Update(gameTime, ref _gameData); //Movement done in Update
        }
        UpdateTickHandleObjectsToBeDrawn(gameTime);
    }
    private void UpdateTickHandleObjectsToBeDrawn(GameTime gameTime)
    {
        for (int i = 0; i < _gameData.Sprites.Count(); i++)
        {
            if (_gameData.Sprites[i].IsAlive && Global.Camera.VisibleArea.Contains(_gameData.Sprites[i].Position))
            {
                _gameData.Sprites[i].DrawObject = true;
            }
            else
            {
                _gameData.Sprites[i].DrawObject = false;
            }
        }
    } //Increase FPS by not drawing offscreen objects
    private void UpdateTickMiniMap(GameTime gameTime)
    {
        _gameData.MiniMap.UpdateMap(_gameData);
    }

    //Update OffTick functions
    private void UpdateOffTick(GameTime gameTime)
    {
        UpdateOffTickHandleCollisionsAndMovement(gameTime); //Collisions And Movement
        UpdateOffTickAnimations(gameTime); //Run animation
        UpdateOffTickHandleCameraChange(gameTime);

        //Every second interval processing only when it is not a TICK. When things only need to be updated once every X seconds
        if (_elapsedTicksSinceIntervalProcessing >= TICKS_PER_SECOND * 5)
        {
            UpdateOffTickInterval(gameTime);
        }
    }
    private void UpdateOffTickHandleCollisionsAndMovement(GameTime gameTime)
    {
        //Border Collision Detection
        for (int i = 0; i < _gameData.Sprites.Count; i++)
        {
            if (_gameData.Sprites[i].IsAlive && _gameData.Sprites[i].IsMovable)
            {
                //Change rotation on wall collision
                if (_gameData.Sprites[i].Bounds.Left <= 0 || _gameData.Sprites[i].Bounds.Right >= _gameData.Settings.WorldSize)
                {
                    if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                    {
                        _gameData.Sprites[i].Rotation = (((float)Math.PI * 2) - _gameData.Sprites[i].Rotation);
                    }
                }
                if (_gameData.Sprites[i].Bounds.Top <= 0 || _gameData.Sprites[i].Bounds.Bottom >= _gameData.Settings.WorldSize)
                {
                    if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                    {
                        _gameData.Sprites[i].Rotation = (((float)Math.PI) - _gameData.Sprites[i].Rotation);
                    }
                }

                //Stationary object collision
                foreach (Point p in _gameData.Sprites[i].GridPositions)
                {
                    for (int k = (_gameData.MapGridData[p.X, p.Y].Sprites.Count() - 1); k >= 0; k--)
                    {
                        if (!_gameData.MapGridData[p.X, p.Y].Sprites[k].IsMovable && _gameData.Sprites[i].Bounds.Intersects(_gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds))
                        {
                            Vector2 offset = CollisionDetection.GetIntersectionDepth(_gameData.Sprites[i].Bounds, _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds);

                            //Add a animation for the collision before we adjust the rotation only if the object is on screen
                            if (_gameData.MapGridData[p.X, p.Y].Sprites[k].DrawObject)
                                _gameData.Animations.AddRange(_gameData.CollisionAnimFactory.Build(_gameData.Sprites[i].Bounds, _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds, offset, _gameData.Sprites[i].Direction, _gameData.Sprites[i].Scale, Color.Gray));

                            if (Math.Abs(offset.X) < Math.Abs(offset.Y))
                            {
                                if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                    _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                                {
                                    _gameData.Sprites[i].Rotation = (((float)Math.PI * 2) - _gameData.Sprites[i].Rotation);
                                    _gameData.Sprites[i].Position = new Vector2(_gameData.Sprites[i].Position.X + offset.X, _gameData.Sprites[i].Position.Y);
                                    _gameData.Sprites[i].Scale -= 0.1f;
                                    if (_gameData.Sprites[i].Scale < 0f)
                                        _gameData.Sprites[i].IsAlive = false;
                                }
                            }
                            else
                            {
                                if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                    _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                                {
                                    _gameData.Sprites[i].Rotation = (((float)Math.PI) - _gameData.Sprites[i].Rotation);
                                    _gameData.Sprites[i].Position = new Vector2(_gameData.Sprites[i].Position.X, _gameData.Sprites[i].Position.Y + offset.Y);
                                    _gameData.Sprites[i].Scale -= 0.1f;
                                    if (_gameData.Sprites[i].Scale < 0f)
                                        _gameData.Sprites[i].IsAlive = false;
                                }
                            }
                        }
                    }
                }

                //Moving Object Collision
                foreach (Point p in _gameData.Sprites[i].GridPositions)
                {
                    for (int k = (_gameData.MapGridData[p.X, p.Y].Sprites.Count() - 1); k >= 0; k--)
                    {
                        if (_gameData.Sprites[i] != _gameData.MapGridData[p.X, p.Y].Sprites[k] && _gameData.Sprites[i].Bounds.Intersects(_gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds))
                        {

                        }
                    }
                }

                _gameData.Sprites[i].Update(gameTime, ref _gameData); //Movement done in Update
            }
        }
    }
    private void UpdateOffTickAnimations(GameTime gameTime)
    {
        for (int i = 0; i < _gameData.Animations.Count(); i++)
        {
            _gameData.Animations[i].Update(gameTime);
        }
    }
    private void UpdateOffTickHandleCameraChange(GameTime gameTime)
    {
        if (Global.Camera.CameraChange)
        {
            Global.Camera.CameraChange = false;
            _gameData.MiniMap.UpdateCamera(Global.Camera.VisibleArea);
        }
    }

    //Update OffTickInterval functions
    private void UpdateOffTickInterval(GameTime gameTime)
    {
        _elapsedTicksSinceIntervalProcessing = 0;
        UpdateOffTickIntervalCleanupAnimations(gameTime);
    }
    private void UpdateOffTickIntervalCleanupAnimations(GameTime gameTime)
    {
        //Cleanup Dead sprites
        for(int i = _gameData.Sprites.Count - 1; i >= 0; i--)
        {
            if (!_gameData.Sprites[i].IsAlive)
            {
                _gameData.RemoveSpriteFromGrid(_gameData.Sprites[i], _gameData.Sprites[i].GridPositions);
                _gameData.AddDeadSpriteToList(_gameData.Sprites[i]);
                _gameData.Sprites.Remove(_gameData.Sprites[i]);
            }
        }

        //Cleanup old animations
        for (int i = _gameData.Animations.Count() - 1; i >= 0; i--)
        {
            if (_gameData.Animations[i].IsAnimationComplete)
            {
                _gameData.Animations[i] = null;
                _gameData.Animations.RemoveAt(i);
            }
        }
    }

    //Draw World functions
    public void DrawWorldObjects()
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Global.Camera.TranslationMatrix);
        DrawSprites();
        DrawAnimations();
        DrawBorders();
        DrawDebugData();
        _spriteBatch.End();
    }
    private void DrawSprites()
    {
        //Draw the stationary objects first
        for (int i = 0; i < _gameData.Sprites.Count; i++)
        {
            _gameData.Sprites[i].Draw(_spriteBatch);

            if (ENABLE_DEBUG)
            {
                if (_gameData.Sprites[i] == _gameData.Focus)
                    _gameData.Sprites[i].DrawDebugDataForSprite(_spriteBatch, false);

                _gameData.Sprites[i].DrawDebugOutlineForSprite(_spriteBatch);
            }
        }
        //Draw the objects that can move next
        for (int i = 0; i < _gameData.Sprites.Count; i++)
        {
            if (_gameData.Sprites[i].DrawObject && _gameData.Sprites[i].IsMovable)
                _gameData.Sprites[i].Draw(_spriteBatch);

            if (ENABLE_DEBUG)
            {
                if (_gameData.Sprites[i] == _gameData.Focus)
                    _gameData.Sprites[i].DrawDebugDataForSprite(_spriteBatch, false);

                _gameData.Sprites[i].DrawDebugOutlineForSprite(_spriteBatch);
            }
        }
    }
    private void DrawAnimations()
    {
        foreach (AnimationBase a in _gameData.Animations)
        {
            a.Draw(_spriteBatch);
        }
    }
    private void DrawBorders()
    {
        _spriteBatch.Draw(_borders.Texture, _borders.LeftWallRectangle, Color.SaddleBrown);
        _spriteBatch.Draw(_borders.Texture, _borders.RightWallRectangle, Color.SaddleBrown);
        _spriteBatch.Draw(_borders.Texture, _borders.TopWallRectangle, Color.SaddleBrown);
        _spriteBatch.Draw(_borders.Texture, _borders.BottomWallRectangle, Color.SaddleBrown);
    }
    private void DrawDebugData()
    {
        if (ENABLE_DEBUG)
        {
            //Draw Map Grid
            for (int i = 0; i < _gameData.MapGridData.GetLength(0); i++)
            {
                _spriteBatch.Draw(_gameData.Textures.WhitePixel, new Rectangle(i * _gameData.Settings.GridCellSize, 0, 1, _gameData.Settings.WorldSize), Color.Red);
            }
            for (int i = 0; i < _gameData.MapGridData.GetLength(1); i++)
            {
                _spriteBatch.Draw(_gameData.Textures.WhitePixel, new Rectangle(0, i * _gameData.Settings.GridCellSize, _gameData.Settings.WorldSize, 1), Color.Red);
            }
        }
    }

    //Draw HUD
    private void DrawHUD()
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
        DrawMiniMap();
        DrawFps();
        _spriteBatch.End();
    }
    private void DrawFps()
    {
        _spriteBatch.DrawString(_diagFont, "FPS: " + _fps, new Vector2(10, 10), Color.Black); //FPS Counter in top left corner
    }
    private void DrawMiniMap()
    {
        _gameData.MiniMap.Draw(_spriteBatch);
    }

    //Debug Test functions
    private void SpawnTestObject()
    {
        SpriteBase sprite;

        int randNum = _rand.Next(0, 10);
        if (randNum > 7)
        {
            sprite = new Truck();
            sprite.Texture = BuildSampleImage(_graphics.GraphicsDevice);
            sprite.Scale = (float)(_rand.NextDouble() * 3);
            sprite.Color = Color.Aqua;
            sprite.ScreenDepth = 1f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        }
        else if (randNum > 2)
        {
            sprite = new Car();
            sprite.Texture = BuildSampleImage(_graphics.GraphicsDevice);
            sprite.Scale = (float)(_rand.NextDouble() * 1.5);
            sprite.Color = Color.DodgerBlue;
            sprite.ScreenDepth = 0.5f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        }
        else
        {
            sprite = new Car();
            sprite.Texture = BuildSampleImage(_graphics.GraphicsDevice);
            sprite.Scale = (float)(_rand.NextDouble() * 0.9);
            sprite.Color = Color.LightSeaGreen;
            sprite.ScreenDepth = 0.25f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        }

        if (sprite.Scale < 0.5f)
            sprite.Scale = 0.5f;

        sprite.IsAlive = true;
        sprite.WorldSize = _gameData.Settings.WorldSize;
        sprite.Speed = 150f;
        sprite.Rotation = MathHelper.ToRadians(_rand.Next(0, 360));
        sprite.Position = new Vector2(_rand.Next((int)sprite.AdjustedSize.X, _gameData.Settings.WorldSize - (int)sprite.AdjustedSize.X), _rand.Next((int)sprite.AdjustedSize.Y, _gameData.Settings.WorldSize - (int)sprite.AdjustedSize.Y));
        sprite.GetGridPositionsForSpriteBase(_gameData);

        //Debug Properties
        sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        sprite.DebugFont = _diagFont;

        _gameData.Sprites.Add(sprite);
        _gameData.AddSpriteToGrid(sprite);
    }
    private void SpawnSampleBuilding()
    {
        SpriteBase sprite;

        sprite = new Building();
        sprite.Texture = BuildSampleImageBuilding(_graphics.GraphicsDevice);
        sprite.Scale = (float)(_rand.NextDouble() * 3);

        if (sprite.Scale < 0.5f)
            sprite.Scale = 0.5f;

        sprite.Color = Color.Peru;
        sprite.ScreenDepth = 1f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        sprite.IsAlive = true;
        sprite.WorldSize = _gameData.Settings.WorldSize;
        sprite.Speed = 0f;
        sprite.Rotation = 0;
        sprite.Position = new Vector2(_rand.Next((int)sprite.AdjustedSize.X, _gameData.Settings.WorldSize - (int)sprite.AdjustedSize.X), _rand.Next((int)sprite.AdjustedSize.Y, _gameData.Settings.WorldSize - (int)sprite.AdjustedSize.Y));
        sprite.GetGridPositionsForSpriteBase(_gameData);

        //Debug Properties
        sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        sprite.DebugFont = _diagFont;

        _gameData.Sprites.Add(sprite);
        _gameData.AddSpriteToGrid(sprite);
    }
    public void SpawnScenerioTestObjs()
    {
        SpriteBase sprite;

        sprite = new Truck();
        sprite.Texture = BuildSampleImage(_graphics.GraphicsDevice);
        sprite.Scale = (float)(_rand.NextDouble() * 3);
        sprite.Color = Color.Aqua;
        sprite.ScreenDepth = 1f;

        if (sprite.Scale < 0.5f)
            sprite.Scale = 0.5f;

        sprite.IsAlive = true;
        sprite.WorldSize = _gameData.Settings.WorldSize;
        sprite.Speed = 150f;
        sprite.Rotation = MathHelper.ToRadians(88);
        sprite.Position = new Vector2(sprite.WorldSize - (sprite.Bounds.Width * 4), 500);
        sprite.GetGridPositionsForSpriteBase(_gameData);

        //Debug Properties
        sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        sprite.DebugFont = _diagFont;

        _gameData.Sprites.Add(sprite);
        _gameData.AddSpriteToGrid(sprite);


        //*************************
        //Building
        //*************************

        //sprite = new Building();
        //sprite.Scale = 3.2f;
        //sprite.Color = Color.Black;
        //sprite.ScreenDepth = 1f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        //sprite.IsAlive = true;
        //sprite.WorldSize = _gameData.Settings.WorldSize;
        //sprite.Texture = BuildSampleImageBuilding(_graphics.GraphicsDevice);
        //sprite.Speed = 0f;
        //sprite.Rotation = 0f;
        //sprite.Position = new Vector2(450,500);
        //sprite.GetGridPositionsForSpriteBase(GRID_CELL_SIZE, _gameData);

        ////Debug Properties
        //sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        //sprite.DebugFont = _diagFont;

        //_gameData.Sprites.Add(sprite);
        //_gameData.AddSpriteToGrid(sprite);

        //sprite = new Building();
        //sprite.Scale = 2.8f;
        //sprite.Color = Color.Black;
        //sprite.ScreenDepth = 1f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        //sprite.IsAlive = true;
        //sprite.WorldSize = _gameData.Settings.WorldSize;
        //sprite.Texture = BuildSampleImageBuilding(_graphics.GraphicsDevice);
        //sprite.Speed = 0f;
        //sprite.Rotation = 0f;
        //sprite.Position = new Vector2(800, 500);
        //sprite.GetGridPositionsForSpriteBase(GRID_CELL_SIZE, _gameData);

        ////Debug Properties
        //sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        //sprite.DebugFont = _diagFont;

        //_gameData.Sprites.Add(sprite);
        //_gameData.AddSpriteToGrid(sprite);
    }
    public void SpawnScenerioHumanObey()
    {
        //***********************
        //Hunter
        //***********************
        for (int i = 0; i < 2000; i++)
        {
            Wolf hunter = new Wolf();
            hunter.AnimalAi = new AiChase(hunter);
            hunter.ThinkingCooldownMs = 500;
            hunter.Texture = BuildSampleImage(_graphics.GraphicsDevice);
            hunter.MiniMapTexture = _gameData.Textures.MiniMapObjectDiamondTexture;
            hunter.MiniMapScale = 1f;
            hunter.Scale = (float)(_rand.NextDouble() * 5);
            hunter.Color = Color.Black;
            hunter.ScreenDepth = 1f;

            if (hunter.Scale < 4f)
                hunter.Scale = 4f;

            hunter.IsAlive = true;
            hunter.WorldSize = _gameData.Settings.WorldSize;
            hunter.Speed = 400f;
            hunter.Rotation = MathHelper.ToRadians(88);
            hunter.Position = new Vector2(_rand.Next((int)hunter.AdjustedSize.X, _gameData.Settings.WorldSize - (int)hunter.AdjustedSize.X), _rand.Next((int)hunter.AdjustedSize.Y, _gameData.Settings.WorldSize - (int)hunter.AdjustedSize.Y));
            hunter.GetGridPositionsForSpriteBase(_gameData);

            //Debug Properties
            hunter.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
            hunter.DebugFont = _diagFont;

            _gameData.Sprites.Add(hunter);
            _gameData.AddSpriteToGrid(hunter);
        }

        //***********************
        //Prey
        //***********************
        for (int i = 0; i < 1000; i++)
        {
            Truck prey = new Truck();
            prey.Texture = BuildSampleImage(_graphics.GraphicsDevice);
            prey.MiniMapTexture = _gameData.Textures.MiniMapObjectCircleTexture;
            prey.MiniMapScale = 0.5f;
            prey.Scale = (float)(_rand.NextDouble() * 2);
            prey.Color = new Color((float)_rand.NextDouble(), (float)_rand.NextDouble(), (float)_rand.NextDouble());
            prey.ScreenDepth = 1f;

            if (prey.Scale < 0.5f)
                prey.Scale = 0.5f;

            prey.IsAlive = true;
            prey.WorldSize = _gameData.Settings.WorldSize;
            prey.Speed = 450f;
            prey.Rotation = MathHelper.ToRadians(_rand.Next(0, 360));
            prey.Position = new Vector2(_rand.Next((int)prey.AdjustedSize.X, _gameData.Settings.WorldSize - (int)prey.AdjustedSize.X), _rand.Next((int)prey.AdjustedSize.Y, _gameData.Settings.WorldSize - (int)prey.AdjustedSize.Y));
            prey.GetGridPositionsForSpriteBase(_gameData);
            prey.IsMoving = true; //This Object is dumb, need to manually set IsMoving to true

            //Debug Properties
            prey.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
            prey.DebugFont = _diagFont;

            _gameData.Sprites.Add(prey);
            _gameData.AddSpriteToGrid(prey);
        }

        //***********************
        //Buildings
        //***********************
        for (int i = 0; i < 0; i++)
        {
            Building building = new Building();
            building.Texture = BuildSampleImageBuilding(_graphics.GraphicsDevice);
            building.MiniMapTexture = _gameData.Textures.MiniMapBuildingTexture;
            building.MiniMapScale = 1f;
            building.MiniMapOpacity = 0.35f;
            building.Scale = (float)(_rand.NextDouble() * 2);
            building.Color = Color.White;
            building.ScreenDepth = 1f;

            if (building.Scale < 0.8f)
                building.Scale = 0.8f;

            building.IsAlive = true;
            building.WorldSize = _gameData.Settings.WorldSize;
            building.Speed = 0f;
            building.Rotation = 0f;
            building.Position = new Vector2(_rand.Next((int)building.AdjustedSize.X, _gameData.Settings.WorldSize - (int)building.AdjustedSize.X), _rand.Next((int)building.AdjustedSize.Y, _gameData.Settings.WorldSize - (int)building.AdjustedSize.Y));
            building.GetGridPositionsForSpriteBase(_gameData);

            //Debug Properties
            building.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
            building.DebugFont = _diagFont;

            _gameData.Sprites.Add(building);
            _gameData.AddSpriteToGrid(building);
        }
    }
    private Texture2D BuildSampleImage(GraphicsDevice device)
    {
        int IMAGE_WIDTH = 32;
        int IMAGE_HEIGHT = 32;
        Texture2D texture = new Texture2D(device, IMAGE_WIDTH, IMAGE_HEIGHT);

        List<Color> pixels = new List<Color>();
        for (int i = 0; i < (IMAGE_WIDTH * IMAGE_HEIGHT); i++)
        {
            pixels.Add(Color.White);
        }

        texture.SetData(pixels.ToArray());

        return texture;
    }
    private Texture2D BuildSampleImageBuilding(GraphicsDevice device)
    {
        int IMAGE_WIDTH = 64;
        int IMAGE_HEIGHT = 64;
        Texture2D texture = new Texture2D(device, IMAGE_WIDTH, IMAGE_HEIGHT);

        List<Color> pixels = new List<Color>();
        int border = 5;

        for (int i = 0; i < IMAGE_HEIGHT; i++)
        {
            for (int k = 0; k < IMAGE_WIDTH; k++)
            {
                if ((i < border || i >= (IMAGE_HEIGHT - border)) || (k < border || k >= (IMAGE_WIDTH - border)))
                {
                    pixels.Add(Color.Black);
                }
                else
                {
                    pixels.Add(Color.White);
                }
            }
        }

        texture.SetData(pixels.ToArray());

        return texture;
    }
}