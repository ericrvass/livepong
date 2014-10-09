using System;
using System.Collections.Generic;

using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Utilities;
using FlatRedBall.Input;
using FlatRedBall.Math.Geometry;

using Microsoft.Xna.Framework;
#if !FRB_MDX
using System.Linq;

using WiimoteLib;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#endif
using FlatRedBall.Screens;

namespace LivePongFRB
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Sprite background;
        Text countText;
        Sprite winSprite;
        Sprite[] winStars;
        Sprite scoreSprite;
        Sprite countSprite;
        Ball gameBall;
        Player player1;
        Player player2;
        static bool showPositions = false;
        static bool animatePaddle = true;
        static int starCount = 30;
        WiimoteCollection mWC;
        static float sensitivity = 2;
        static int winScore = 5;
        static int initialBallSpeed = 20;
        int gameStartCount = 0;
        double gameTimeMarker = 0;
        bool animateScore = false;

        struct Player
        {
            public Sprite sprite;
            public Sprite headSprite;
            public Sprite leftArmSprite;
            public Sprite rightArmSprite;
            public int playerNumber;
            public int score;
            public Text positionText;
            public Text scoreText;
            public Player(int number)
            {
                this.sprite = SpriteManager.AddSprite("paddle.png");
                this.sprite.Alpha = 0.0f;
                this.headSprite = SpriteManager.AddSprite("cathead2.png");
                this.headSprite.Z = 2;
                this.headSprite.ScaleX = 2.5f;
                this.headSprite.ScaleY = 2.5f;
                
                this.leftArmSprite = SpriteManager.AddSprite("catleftarm2.png");
                this.leftArmSprite.Z = 1;
                this.leftArmSprite.ScaleX = 1.2f;
                this.leftArmSprite.ScaleY = 2.4f;
                
                this.rightArmSprite = SpriteManager.AddSprite("catrightarm2.png");
                this.rightArmSprite.Z = 1;
                this.rightArmSprite.ScaleX = 1.2f;
                this.rightArmSprite.ScaleY = 2.4f;
                
                if (number == 2)
                {
                    this.headSprite.X = 1;
                    this.headSprite.RotationZ = MathHelper.ToRadians(90);
                    this.leftArmSprite.RotationZ = MathHelper.ToRadians(180);
                    this.leftArmSprite.Y = 3;
                    this.rightArmSprite.RotationZ = MathHelper.ToRadians(0);
                    this.rightArmSprite.Y = -3;
                }
                else
                {
                    this.headSprite.X = -0.8f;
                    this.headSprite.RotationZ = MathHelper.ToRadians(270);
                    this.leftArmSprite.RotationZ = MathHelper.ToRadians(180);
                    this.leftArmSprite.Y = 3;
                    this.rightArmSprite.RotationZ = MathHelper.ToRadians(0);
                    this.rightArmSprite.Y = -3;
                }
                this.headSprite.AttachTo(this.sprite, true);
                this.leftArmSprite.AttachTo(this.sprite, true);
                this.rightArmSprite.AttachTo(this.sprite, true);
                //this.sprite.ScaleX = this.sprite.Texture.Width / 25.0F;
                //this.sprite.ScaleY = this.sprite.Texture.Height / 15.0F;
                this.sprite.ScaleY = this.sprite.ScaleY * 4;
                this.score = 0;
                string fontTextureFile = "GameFont_0.png";
                string fontPatternFile = "GameFont.fnt";
                string contentManagerName = "Global";
                BitmapFont customFont = new BitmapFont(fontTextureFile, fontPatternFile,
                    contentManagerName);

                this.scoreText = TextManager.AddText(this.score.ToString(), customFont);
                this.scoreText.Scale = 1.0f;
                this.scoreText.Red = 92;
                this.scoreText.Green = 255;
                this.scoreText.Blue = 127;
                this.scoreText.ColorOperation = ColorOperation.Modulate;
                if (number == 2)
                {
                    this.sprite.X = -20;
                    this.sprite.Y = 0;
                    this.scoreText.X = 1.2f;
                    this.scoreText.Y = 15.7f;
                }
                else
                {
                    this.sprite.X = 20;
                    this.sprite.Y = 0;
                    this.scoreText.X = -1.6f;
                    this.scoreText.Y = 15.7f;
                }
                this.playerNumber = number;
                AxisAlignedRectangle rectangle = new AxisAlignedRectangle(this.sprite.ScaleX, this.sprite.ScaleY);
                this.sprite.SetCollision(rectangle);
                if (showPositions)
                {
                    this.positionText = TextManager.AddText(this.sprite.X.ToString() + "," + this.sprite.Y.ToString());
                    this.positionText.X = this.sprite.X;
                    this.positionText.Y = this.sprite.Y;
                }
                else
                {
                    this.positionText = null;
                }
            }
            public void incrementScore()
            {
                this.score += 1;
                this.scoreText.DisplayText = this.score.ToString();
            }
            public void updatePositionText() {
                if (showPositions)
                {
                    this.positionText.DisplayText = this.sprite.X.ToString() + "," + this.sprite.Y.ToString();
                    this.positionText.X = this.sprite.X;
                    this.positionText.Y = this.sprite.Y;
                }
            }
            public void remove()
            {
                if (this.sprite != null)
                {
                    SpriteManager.RemoveSprite(this.sprite);
                    this.sprite = null;
                    SpriteManager.RemoveSprite(this.headSprite);
                    this.headSprite = null;
                    SpriteManager.RemoveSprite(this.leftArmSprite);
                    this.leftArmSprite = null;
                    SpriteManager.RemoveSprite(this.rightArmSprite);
                    this.rightArmSprite = null;
                }
                if (this.scoreText != null)
                {
                    TextManager.RemoveText(this.scoreText);
                    this.scoreText = null;
                }
            }
        }

        struct Ball
        {
            public Sprite sprite;
            public Text positionText;
            public Ball(int number)
            {
                this.sprite = SpriteManager.AddSprite("yarnball.png");
                this.sprite.ScaleX = this.sprite.ScaleX * 2;
                this.sprite.ScaleY = this.sprite.ScaleY * 2;
                this.sprite.RotationZVelocity = 1;
                this.positionText = null;

                this.resetPosition();
                this.startBall();

                AxisAlignedRectangle rectangle = new AxisAlignedRectangle(this.sprite.ScaleX, this.sprite.ScaleY);
                this.sprite.SetCollision(rectangle);
                if (showPositions)
                {
                    this.positionText = TextManager.AddText(this.sprite.X.ToString() + "," + this.sprite.Y.ToString());
                    this.positionText.X = this.sprite.X;
                    this.positionText.Y = this.sprite.Y;
                }
            }
            public void resetPosition()
            {
                this.sprite.X = 0;
                this.sprite.Y = 0;
                this.sprite.XVelocity = 0;
                this.sprite.YVelocity = 0;
            }
            public void startBall()
            {
                Random random = new Random();
                if (random.Next(0, 2) == 0)
                {
                    this.sprite.XVelocity = initialBallSpeed * -1;
                }
                else
                {
                    this.sprite.XVelocity = initialBallSpeed;
                }
                this.sprite.YVelocity = random.Next(-10, 11);
                //this.sprite.YAcceleration = -2;
            }
            public void updatePositionText()
            {
                if (showPositions)
                {
                    this.positionText.DisplayText = this.sprite.X.ToString() + "," + this.sprite.Y.ToString();
                    this.positionText.X = this.sprite.X;
                    this.positionText.Y = this.sprite.Y;
                }
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
			
#if WINDOWS_PHONE || ANDROID || IOS

			// Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#else
            graphics.PreferredBackBufferHeight = 600;
#endif
            
        }

        protected void UpdateInput()
        {
            if (InputManager.Keyboard.KeyDown(Keys.Escape))
            {
                FlatRedBallServices.Game.Exit();
            }
            if (InputManager.Keyboard.KeyDown(Keys.Up) && player1.sprite.Y < 10)
            {
                player1.sprite.YVelocity += 2;
                if (animatePaddle)
                {
                    player1.sprite.RotationZ = MathHelper.ToRadians(355);
                    //player1.headSprite.RelativeRotationZ = MathHelper.ToRadians(270);
                }
            }
            else if (InputManager.Keyboard.KeyDown(Keys.Down) && player1.sprite.Y > -10)
            {
                player1.sprite.YVelocity += -2;
                if (animatePaddle)
                {
                    player1.sprite.RotationZ = MathHelper.ToRadians(5);
                    //player1.headSprite.RelativeRotationZ = MathHelper.ToRadians(260);
                }
            }
            else
            {
                player1.sprite.YVelocity = 0;
                player1.sprite.RotationZ = MathHelper.ToRadians(0);
                //player1.headSprite.RelativeRotationZ = MathHelper.ToRadians(270);
            }
            if (InputManager.Keyboard.KeyDown(Keys.W) && player2.sprite.Y < 10)
            {
                player2.sprite.YVelocity += 2;
                if (animatePaddle)
                {
                    player2.sprite.RotationZ = MathHelper.ToRadians(5);
                }
            }
            else if (InputManager.Keyboard.KeyDown(Keys.S) && player2.sprite.Y > -10)
            {
                player2.sprite.YVelocity += -2;
                if (animatePaddle)
                {
                    player2.sprite.RotationZ = MathHelper.ToRadians(355);
                }
            }
            else
            {
                player2.sprite.YVelocity = 0;
                player2.sprite.RotationZ = MathHelper.ToRadians(0);
            }
        }

        protected void UpdateWiimotes()
        {
            int i = 1;
            foreach (Wiimote wm in mWC) {
                IRState irs = wm.WiimoteState.IRState;
                float xmidpoint = Math.Min(irs.Midpoint.X * sensitivity, 1);
                if (xmidpoint > 0)
                {
                    if (i == 1)
                    {
                        float position = ((xmidpoint * 20) - 10) * 1;
                        //player1.scoreText.DisplayText = position.ToString();
                        player1.sprite.Y = position;
                    }
                    else
                    {
                        float position = ((xmidpoint * 20) - 10) * -1;
                        //player2.scoreText.DisplayText = position.ToString();
                        player2.sprite.Y = position;
                    }
                }
                i++;
            }
        }

        protected void CheckCollisions()
        {
            if (gameBall.sprite != null && player1.sprite != null && gameBall.sprite.CollideAgainst(player1.sprite)) 
            {
                gameBall.sprite.XVelocity = gameBall.sprite.XVelocity * -1;
                if (gameBall.sprite.XVelocity > 0)
                    gameBall.sprite.XVelocity += 1.5f;
                else
                    gameBall.sprite.XVelocity += -1.5f;

                if (gameBall.sprite.Y < player1.sprite.Y)
                    gameBall.sprite.YVelocity = (player1.sprite.Y - gameBall.sprite.Y) * -2;
                else
                    gameBall.sprite.YVelocity = (gameBall.sprite.Y - player1.sprite.Y) * 2;
            }
            else if (gameBall.sprite != null && player2.sprite != null && gameBall.sprite.CollideAgainst(player2.sprite))
            {
                gameBall.sprite.XVelocity = gameBall.sprite.XVelocity * -1;
                if (gameBall.sprite.XVelocity > 0)
                    gameBall.sprite.XVelocity += 1.5f;
                else
                    gameBall.sprite.XVelocity += -1.5f;

                if (gameBall.sprite.Y < player2.sprite.Y)
                    gameBall.sprite.YVelocity = (player2.sprite.Y - gameBall.sprite.Y) * -2;
                else
                    gameBall.sprite.YVelocity = (gameBall.sprite.Y - player2.sprite.Y) * 2;
                 
            }
            if (gameBall.sprite != null && gameBall.sprite.Y > 14)
            {
                gameBall.sprite.YVelocity = gameBall.sprite.YVelocity * -1;
            }
            else if (gameBall.sprite != null && gameBall.sprite.Y < -14)
            {
                gameBall.sprite.YVelocity = gameBall.sprite.YVelocity * -1;
            }
            if (gameBall.sprite != null && gameBall.sprite.X > 20)
            {
                //score player 1
                gameBall.resetPosition();
                gameBall.sprite.Alpha = 0.0f;
                player1.incrementScore();
                if (player1.score >= winScore)
                {
                    //player 1 wins!
                    ProcessWin(player1.playerNumber);
                }
                else
                {
                    AnimateScore(player1.playerNumber);
                }
            }
            else if (gameBall.sprite.X < -20)
            {
                //score player 2
                gameBall.resetPosition();
                gameBall.sprite.Alpha = 0.0f;
                player2.incrementScore();
                if (player2.score >= winScore)
                {
                    //player 2 wins!
                    ProcessWin(player2.playerNumber);
                }
                else
                {
                    AnimateScore(player2.playerNumber);
                }
            }
        }

        protected override void Initialize()
        {
            Renderer.UseRenderTargets = false;
            FlatRedBallServices.InitializeFlatRedBall(this, graphics);
            int desiredWidth = 800;
            int desiredHeight = 600;
            FlatRedBallServices.GraphicsOptions.SetFullScreen(desiredWidth, desiredHeight);
            background = SpriteManager.AddSprite("background2.png");
            background.ScaleX = 22.5f;
            background.ScaleY = 17;

            InitializeWiiMotes();

            //ScreenManager.Start(typeof(SomeScreen).FullName);

            StartNewGame();

            base.Initialize();
        }
        private void InitializeWiiMotes()
        {
            // find all wiimotes connected to the system
            mWC = new WiimoteCollection();
            int index = 1;

            try
            {
                mWC.FindAllWiimotes();
            }
            catch (WiimoteNotFoundException ex)
            {
                
            }
            catch (WiimoteException ex)
            {
               
            }
            catch (Exception ex)
            {
                
            }

            foreach (Wiimote wm in mWC)
            {

                // connect it and set it up as always
                wm.WiimoteChanged += wm_WiimoteChanged;
                wm.WiimoteExtensionChanged += wm_WiimoteExtensionChanged;
                try
                {
                    wm.Connect();
                    if (wm.WiimoteState.ExtensionType != ExtensionType.BalanceBoard)
                        wm.SetReportType(InputReport.IRExtensionAccel, IRSensitivity.Maximum, true);

                    wm.SetLEDs(index++);
                }
                catch (Exception ex)
                {

                }
            }
        }

        void wm_WiimoteChanged(object sender, WiimoteChangedEventArgs e)
        {
            
        }

        void wm_WiimoteExtensionChanged(object sender, WiimoteExtensionChangedEventArgs e)
        {
 

            if (e.Inserted)
                ((Wiimote)sender).SetReportType(InputReport.IRExtensionAccel, true);
            else
                ((Wiimote)sender).SetReportType(InputReport.IRAccel, true);
        }

        protected override void Update(GameTime gameTime)
        {
            FlatRedBallServices.Update(gameTime);

            if (gameBall.sprite != null)
            {
                ScoreStateManage();

                UpdateInput();

                UpdateWiimotes();

                CheckCollisions();

                if (showPositions)
                {
                    player1.updatePositionText();
                    player2.updatePositionText();
                    gameBall.updatePositionText();
                }
            }
            else
            {
                WinStateManage();
                GameStartStateManage();
                
            }
            FlatRedBall.Screens.ScreenManager.Activity();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            FlatRedBallServices.Draw();

            base.Draw(gameTime);
        }

        private void ProcessWin(int playerNumber)
        {
            EndGame();
            Random random = new Random();
            //win sprite
            if (playerNumber == 1)
            {
                winSprite = SpriteManager.AddSprite("deadfish.png");
                winSprite.ScaleX = 2;
                winSprite.ScaleY = 2;
                winSprite.X = 16;
                winSprite.XAcceleration = -20;
                winSprite.RotationZVelocity = random.Next(-5, 5);
                winStars = new Sprite[starCount];
                for (int i = 0; i < starCount; i++)
                {
                    winStars[i] = SpriteManager.AddSprite("deadfish.png");
                    winStars[i].ScaleX = 2;
                    winStars[i].ScaleY = 2;
                    winStars[i].X = 20;
                    winStars[i].XAcceleration = random.Next(-30, -10);
                    winStars[i].YAcceleration = random.Next(-20, 21);
                    winStars[i].RotationZVelocity = random.Next(-5, 5);
                }
            }
            else
            {
                winSprite = SpriteManager.AddSprite("deadfish.png");
                winSprite.ScaleX = 2;
                winSprite.ScaleY = 2;
                winSprite.X = -20;
                winSprite.XAcceleration = 20;
                winSprite.RotationZVelocity = random.Next(-5, 5);
                winStars = new Sprite[starCount];
                for (int i = 0; i < starCount; i++)
                {
                    winStars[i] = SpriteManager.AddSprite("deadfish.png");
                    winStars[i].ScaleX = 2;
                    winStars[i].ScaleY = 2;
                    winStars[i].X = -16;
                    winStars[i].XAcceleration = random.Next(10, 30);
                    winStars[i].YAcceleration = random.Next(-20, 21);
                    winStars[i].RotationZVelocity = random.Next(-5, 5);
                }
            }
            winSprite.YAcceleration = random.Next(-20, 21);
            gameTimeMarker = TimeManager.CurrentTime;
            
        }

        private void AnimateScore(int playerNumber)
        {
            animateScore = true;
            scoreSprite = SpriteManager.AddSprite("win.png");
            scoreSprite.Alpha = 0.0f;
            scoreSprite.AlphaRate = 1;
            scoreSprite.ScaleX = 10;
            scoreSprite.ScaleY = 10;
            gameTimeMarker = TimeManager.CurrentTime;
        }

        private void EndGame()
        {
            if (gameBall.sprite != null)
            {
                SpriteManager.RemoveSprite(gameBall.sprite);
                gameBall.sprite = null;
            }
            player1.remove();
            player2.remove();
        }

        private void StartNewGame()
        {
            gameStartCount = 4;
            // countdown animation
            /**string fontTextureFile = "GameFont_0.png";
            string fontPatternFile = "GameFont.fnt";
            string contentManagerName = "Global";
            BitmapFont customFont = new BitmapFont(fontTextureFile, fontPatternFile,
                contentManagerName);
            countText = TextManager.AddText("3", customFont);
            countText.Scale = 1.0f;
            countText.Red = 92;
            countText.Green = 255;
            countText.Blue = 127;
            countText.ColorOperation = ColorOperation.Modulate;
             * */
            countSprite = SpriteManager.AddSprite("three.png");
            countSprite.ScaleX = 8;
            countSprite.ScaleY = 8;
            gameTimeMarker = TimeManager.CurrentTime;
        }

        private void GameStartStateManage()
        {
            if (gameStartCount > 0 && TimeManager.CurrentTime - gameTimeMarker > 1.0) 
            {
                SpriteManager.RemoveSprite(countSprite);
                countSprite = null;
                if (gameStartCount == 4)
                {
                    countSprite = SpriteManager.AddSprite("two.png");
                    countSprite.ScaleX = 8;
                    countSprite.ScaleY = 8;
                    
                }
                else if (gameStartCount == 3)
                {
                    countSprite = SpriteManager.AddSprite("one.png");
                    countSprite.ScaleX = 8;
                    countSprite.ScaleY = 8;
                    
                }
                else if (gameStartCount == 2)
                {
                    countSprite = SpriteManager.AddSprite("kill.png");
                    countSprite.ScaleX = 8;
                    countSprite.ScaleY = 8;
                }
                else
                {
                    gameBall = new Ball(1);
                    player1 = new Player(1);
                    player2 = new Player(2);
                }
                gameStartCount--;
                gameTimeMarker = TimeManager.CurrentTime;
            }
        }

        private void WinStateManage()
        {
            if (winSprite != null && TimeManager.CurrentTime - gameTimeMarker > 2.0f)
            {
                SpriteManager.RemoveSprite(winSprite);
                winSprite = null;
                for (int i = 0; i < starCount; i++)
                {
                    SpriteManager.RemoveSprite(winStars[i]);
                    winStars[i] = null;
                }
                StartNewGame();
            }
        }

        private void ScoreStateManage()
        {
            if (animateScore && TimeManager.CurrentTime - gameTimeMarker > 2.0f)
            {
                SpriteManager.RemoveSprite(scoreSprite);
                scoreSprite = null;
                gameBall.startBall();
                gameBall.sprite.Alpha = 1.0f;
                animateScore = false;
            }
            else if (animateScore && TimeManager.CurrentTime - gameTimeMarker > 1.0f)
            {
                scoreSprite.AlphaRate = -1;
            }
        }
    }
}
