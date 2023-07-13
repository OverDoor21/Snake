using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int SnakeSquareSize = 20;
        private SolidColorBrush snakeBodyBrush = Brushes.Red;
        private SolidColorBrush snakeHeadBrush = Brushes.OrangeRed;
        private List<SnakeBody> snakeParts = new List<SnakeBody>();

        public enum SnakeDirection { Left,Right,Up,Down};
        public SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLenght;

        public MainWindow()
        {
            InitializeComponent();
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
            foreach(SnakeBody snakeBody in snakeParts)
            {
                if(snakeBody.uIElement == null)
                {
                    snakeBody.uIElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakeBody.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakeBody.uIElement);
                    Canvas.SetTop(snakeBody.uIElement, snakeBody.Position.Y);
                    Canvas.SetBottom(snakeBody.uIElement, snakeBody.Position.X);

                }
            }
        }
        private void MoveSnake()
        {
            while(snakeParts.Count >= snakeLenght)
            {
                GameArea.Children.Remove(snakeParts[0].uIElement);
                snakeParts.RemoveAt(0); 
            }

            foreach(SnakeBody snakePart in snakeParts)
            {
                (snakePart.uIElement as Rectangle).Fill = snakeBodyBrush;
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




        }

    }
    
}
