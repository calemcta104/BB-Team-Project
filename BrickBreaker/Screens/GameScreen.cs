﻿/*  
 *  Created by: Calem
 *  Project: Brick Breaker
 *  Date: 
 */ 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Xml;

namespace BrickBreaker
{
    public partial class GameScreen : UserControl
    {
        #region global values

        //player1 button control keys - DO NOT CHANGE
        Boolean leftArrowDown, rightArrowDown;

        // Game values
        int level;
        int playerLives;
        int playerScore; // many need to change if player score gets too high

        // Paddle and Ball objects
        Paddle paddle;
        Ball ball;

        // list of all blocks for current level
        List<Block> blocks = new List<Block>();

        // Brushes
        SolidBrush paddleBrush = new SolidBrush(Color.White);
        SolidBrush ballBrush = new SolidBrush(Color.White);
        SolidBrush blockBrush = new SolidBrush(Color.Red);

        //List that will build highscores using a class to then commit them to a XML file
        List<score> highScoreList = new List<score>();
        string highScore;

        #endregion

        public GameScreen()
        {
            InitializeComponent();
            OnStart();
        }

        public void OnStart()
        {
            //set life counter
            playerLives = 3;

            // display life and score values
            scoreLabel.Text = playerScore + "";
            lifeLabel.Text = playerLives + "";

            //set all button presses to false.
            leftArrowDown = rightArrowDown = false;

            // setup starting paddle values and create paddle object
            int paddleWidth = 80;
            int paddleHeight = 20;
            int paddleX = ((this.Width / 2) - (paddleWidth / 2));
            int paddleY = (this.Height - paddleHeight) - 60;
            int paddleSpeed = 8;
            paddle = new Paddle(paddleX, paddleY, paddleWidth, paddleHeight, paddleSpeed, Color.White);

            #region ball variables
            // setup starting ball values
            int ballX = this.Width / 2 - 10;
            int ballY = this.Height - paddle.height - 80;

            // Creates a new ball
            int xSpeed = 6;
            int ySpeed = 6;
            int ballSize = 20;
            //starts ball moving up and right
            bool ballRight = true;
            bool ballUp = true;
            ball = new Ball(ballX, ballY, xSpeed, ySpeed, ballSize, ballRight, ballUp);
            #endregion

            #region Creates blocks for generic level. Need to replace with code that loads levels.

            //TODO - replace all the code in this region eventually with code that loads levels from xml files
            blocks.Clear();
            int x = 10;

            while (blocks.Count() < 12)
            {
                x += 57;
                Block b1 = new Block(x, 10, 1, Color.White);
                blocks.Add(b1);
            }

            #endregion

            // start the game engine loop
            gameTimer.Enabled = true;
        }

        public void CalemMethod()
        {
            // Move ball
            ball.Move();

            // Check for collision with top and side walls
            ball.WallCollision(this);

            // Check for ball hitting bottom of screen
            if (ball.BottomCollision(this))
            {
                playerLives--;
                lifeLabel.Text = playerLives + "";

                //Move paddle to middle
                paddle.x = (this.Width / 2 - paddle.width);
                // Moves the ball back to origin
                ball.x = ((paddle.x - (ball.size / 2)) + (paddle.width / 2));
                ball.y = (this.Height - paddle.height) - 85;                

                if (playerLives == 0)
                {
                    gameTimer.Enabled = false;
                    OnEnd();
                }
            }

            // Check for collision of ball with paddle, (incl. paddle movement)
            ball.PaddleCollision(paddle, leftArrowDown, rightArrowDown);
        }

        public void DeclanMethod()
        {
            // Check if ball has collided with any blocks
            foreach (Block b in blocks)
            {
                if (ball.BlockCollision(b)) // block health decreases when hit by ball
                {
                    b.hp--;

                    if (b.hp > 0) // player score increases when the ball hits a block
                    {
                        playerScore = playerScore + 50; // update score
                        scoreLabel.Text = playerScore + ""; // display updated score
                    }
                    else if (b.hp == 0) // remove block from screen if its health is zero
                    {
                        playerScore = playerScore + 100; // update score
                        scoreLabel.Text = playerScore + ""; // display updated score
                        blocks.Remove(b);
                    }

                    if (blocks.Count == 0) // go to next level if player finishes current level
                    {
                        gameTimer.Enabled = false;
                        OnEnd();
                    }

                    break;
                }
            }
        }

        private void GameScreen_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //player 1 button presses
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = true;
                    break;
                case Keys.Right:
                    rightArrowDown = true;
                    break;
                default:
                    break;
            }
        }

        private void GameScreen_KeyUp(object sender, KeyEventArgs e)
        {
            //player 1 button releases
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = false;
                    break;
                case Keys.Right:
                    rightArrowDown = false;
                    break;
                default:
                    break;
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            DeclanMethod();

            // Move the paddle
            if (leftArrowDown && paddle.x > 0)
            {
                paddle.Move("left");
            }
            if (rightArrowDown && paddle.x < (this.Width - paddle.width))
            {
                paddle.Move("right");
            }


            CalemMethod();


            //redraw the screen
            Refresh();
        }

        public void OnEnd()
        {
            HighScoreRead();
            HighScoreWrite();

            // Goes to the game over screen
            Form form = this.FindForm();
            MenuScreen ps = new MenuScreen();
            
            ps.Location = new Point((form.Width - ps.Width) / 2, (form.Height - ps.Height) / 2);

            form.Controls.Add(ps);
            form.Controls.Remove(this);
        }

        public void GameScreen_Paint(object sender, PaintEventArgs e)
        {
            // Draws paddle
            paddleBrush.Color = paddle.colour;
            e.Graphics.FillRectangle(paddleBrush, paddle.x, paddle.y, paddle.width, paddle.height);

            // Draws blocks
            foreach (Block b in blocks)
            {
                e.Graphics.FillRectangle(blockBrush, b.x, b.y, b.width, b.height);
            }

            // Draws ball
            e.Graphics.FillRectangle(ballBrush, ball.x, ball.y, ball.size, ball.size);
        }

        public void HighScoreRead()
        {
            XmlReader reader = XmlReader.Create("highScores.xml", null);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Text)
                {
                    reader.ReadToNextSibling("score");
                    string numScore = reader.ReadString();

                    score s = new score(numScore);
                    highScoreList.Add(s);

                    //highScoreLabel.Text += s.numScore + "\n";
                }
            }
            
            // remove the lowest high score if there are already 10 scores when adding a new score 
            if (highScoreList.Count > 10)
            {
                highScoreList.RemoveAt(10);
            }

            reader.Close();
        }

        public void HighScoreWrite()
        {
            // create write for xml file
            XmlWriter writer = XmlWriter.Create("highScores.xml", null);          

            // start writer
            writer.WriteStartElement("Highscores");

            // write every score in high score list
            foreach (score s in highScoreList)
            {
                writer.WriteStartElement("playerScore");

                writer.WriteElementString("score", s.numScore);

                writer.WriteEndElement();
            }

            // end and close writer
            writer.WriteEndElement();
            writer.Close();
        }
    }
}
