using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random random = new Random();
        private SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        private int currentScore = 0;
        const int SnakeSquareSize = 20;
        const int SnakeStartLenght = 3;
        const int SnakeStartSpeed = 400;
        const int SnakeSpeedThreshold = 100;
        const int MaxHighscoreListEntryCount = 5;
        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.LightGreen;
        private List<SnakeBody> snakeParts = new List<SnakeBody>();
        private DispatcherTimer gameTickTimer = new DispatcherTimer();
        private UIElement snakeFood = null;
        private SolidColorBrush foodbrush = Brushes.Red;
        SnakeHighscore snakeHighscore = new SnakeHighscore();

       



        public enum SnakeDirection { Left, Right, Up, Down };
        public SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLenght;

        public MainWindow()
        {
            InitializeComponent();
            gameTickTimer.Tick += GameTickTimer_Tick;
            snakeHighscore.LoadHighscoreList();
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
            
        }

        private void DrawGameArea()
        {
            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;
            int rowCounter = 0;
            bool nextIsOdd = false;

            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = nextIsOdd ? Brushes.White : Brushes.Black
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                nextIsOdd = !nextIsOdd;
                nextX += SnakeSquareSize;
                if (nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }

                if (nextY >= GameArea.ActualHeight)
                    doneDrawingBackground = true;
            }
        }
        private void DrawSnake()
        {
            foreach (SnakeBody snakePart in snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }
        private void MoveSnake()
        {

            while (snakeParts.Count >= snakeLenght)
            {
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            }

            foreach (SnakeBody snakePart in snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }


            SnakeBody snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }


            snakeParts.Add(new SnakeBody()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });

            DrawSnake();
            DoCollisionCheck();

        }
        private void StartNewGame()
        {
            brdWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Collapsed;
            bdrEndOfGame.Visibility = Visibility.Collapsed;


            foreach (SnakeBody snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
            }
            snakeParts.Clear();
            if (snakeFood != null)
                GameArea.Children.Remove(snakeFood);


            currentScore = 0;
            snakeLenght = SnakeStartLenght;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakeBody() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);


            DrawSnake();
            DrawSnakeFood();


            UpdateGameStatus();

            gameTickTimer.IsEnabled = true;
            
        }
        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int foodX = random.Next(0, maxX) * SnakeSquareSize;
            int foodY = random.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakeBody snakeBody in snakeParts)
            {
                if ((snakeBody.Position.X == foodX) && (snakeBody.Position.Y == foodY))
                    return GetNextFoodPosition();
            }
            return new Point(foodX, foodY);

        }
        private void DrawSnakeFood()
        {
            Point foodpos = GetNextFoodPosition();
            snakeFood = new Ellipse()
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = foodbrush
            };
            GameArea.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodpos.Y);
            Canvas.SetLeft(snakeFood, foodpos.X);
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            SnakeDirection OriginalSnakeDirect = snakeDirection;
            switch (e.Key)
            {
                case System.Windows.Input.Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case System.Windows.Input.Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case System.Windows.Input.Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;
                case System.Windows.Input.Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case System.Windows.Input.Key.Space:
                    StartNewGame();
                    
                    break;

            }
            if (snakeDirection != OriginalSnakeDirect)
                MoveSnake();


        }
        private void DoCollisionCheck()
        {
            SnakeBody snakeHead = snakeParts[snakeParts.Count - 1];

            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(snakeFood)))
            {
                EatSnakeFood();
                return;
            }

            if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight) ||
                (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth))
            {
                EndGame();
            }

            foreach (SnakeBody snakeBodyPart in snakeParts.Take(snakeParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) &&
                    (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                    EndGame();
            }


        }
        private void EatSnakeFood()
        {
            speechSynthesizer.SpeakAsync("Delicious");
            snakeLenght++;
            currentScore++;
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(snakeFood);
            DrawSnakeFood();
            UpdateGameStatus();
        }
        private void UpdateGameStatus()
        {
            tbStatusScore.Text = currentScore.ToString();
            tbStatusSpeed.Text = gameTickTimer.Interval.TotalMilliseconds.ToString();
        }
        private void EndGame()
        {
            bool isNewHighscore = false;
            if (currentScore > 0)
            {
                int lowestHighscore = (snakeHighscore.HighscoreList.Count > 0 ? snakeHighscore.HighscoreList.Min(x => x.Score) : 0);
                if ((currentScore > lowestHighscore) || (snakeHighscore.HighscoreList.Count < MaxHighscoreListEntryCount))
                {
                    bdrNewHighscore.Visibility = Visibility.Visible;
                    txtPlayerName.Focus();
                    isNewHighscore = true;
                }
            }
            if (!isNewHighscore)
            {
                tbFinalScore.Text = currentScore.ToString();
                bdrEndOfGame.Visibility = Visibility.Visible;
            }
            gameTickTimer.IsEnabled = false;
            SpeakEndOfGameInfo(isNewHighscore);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void BtnShowHighscoreList_Click(object sender, EventArgs e)
        {
            brdWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }

        private void btnAddToHighscoreList_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = 0;
           
            if ((snakeHighscore.HighscoreList.Count > 0) && (currentScore < snakeHighscore.HighscoreList.Max(x => x.Score)))
            {
                SnakeHighscore justAbove = snakeHighscore.HighscoreList.OrderByDescending(x => x.Score).First(x => x.Score >= currentScore);
                if (justAbove != null)
                    newIndex = snakeHighscore.HighscoreList.IndexOf(justAbove) + 1;
            }
            
            snakeHighscore.HighscoreList.Insert(newIndex, new SnakeHighscore()
            {
                PlayerName = txtPlayerName.Text,
                Score = currentScore
            });
           
            while (snakeHighscore.HighscoreList.Count > MaxHighscoreListEntryCount)
                snakeHighscore.HighscoreList.RemoveAt(MaxHighscoreListEntryCount);

            snakeHighscore.SaveHighscoreList();

            bdrNewHighscore.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;

        }
        private void SpeakEndOfGameInfo(bool isNewHighscore)
        {
            PromptBuilder promptBuilder = new PromptBuilder();

            promptBuilder.StartStyle(new PromptStyle()
            {
                Emphasis = PromptEmphasis.Reduced,
                Rate = PromptRate.Slow,
                Volume = PromptVolume.ExtraLoud

            }) ;

            promptBuilder.AppendText("Oh no its seems like you ");
            promptBuilder.AppendBreak(TimeSpan.FromMilliseconds(200));
            promptBuilder.AppendText("Died ");
            promptBuilder.EndStyle();

            if (isNewHighscore)
            {
                promptBuilder.AppendBreak(TimeSpan.FromMilliseconds(500));
                promptBuilder.StartStyle(new PromptStyle()
                {
                    Emphasis = PromptEmphasis.Moderate,
                    Rate = PromptRate.Medium,
                    Volume = PromptVolume.ExtraSoft
                });
                promptBuilder.AppendText("NEW SCOREEEE");
                promptBuilder.AppendBreak(TimeSpan.FromMilliseconds(200));
                promptBuilder.AppendTextWithHint(currentScore.ToString(), SayAs.NumberCardinal);
                promptBuilder.EndStyle();
            }
            speechSynthesizer.SpeakAsync(promptBuilder);

        }
    }

}
