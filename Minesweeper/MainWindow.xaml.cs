using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.AccessControl;
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
using System.Windows.Threading;

namespace Minesweeper
{
    /// <summary>
    /// Minesweeper Game
    /// </summary>
    public partial class MainWindow : Window
    {

        private const int ROWS = 2;
        private const int COLS = 2;
        private const int MINES = 1;


        private readonly Button[,] buttons = new Button[ROWS,COLS];
        private int [,] gameBoard = new int[ROWS, COLS];


        private int flagesPlaced = 0;
        private bool [,] flagPostions = new bool[ROWS, COLS];

        private int minesDetected = 0;

        private DispatcherTimer _timer = new DispatcherTimer();
        private int _seconds = 0;


        private Random rnd = new Random();
     
        public MainWindow()
        {
            InitializeComponent();
            Canvas canvas = (Canvas)FindName("GameBoard");


            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();


            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLS; j++)
                {
                    Button btn = new Button
                    {
                        Height = 50,
                        Width = 50,
                        IsEnabled = true,
                        
                };
                    btn.SetValue(Button.TagProperty, new Tuple<int, int>(i, j));
                    btn.Click += Button_Click_Left;
                    btn.PreviewMouseRightButtonDown += new MouseButtonEventHandler(Button_Click_Right);

                    Canvas.SetLeft(btn, j * 50);
                    Canvas.SetTop(btn, i * 50);
                    canvas.Children.Add(btn);
                    
                    buttons[i, j] = btn;

                }
            }

            PlaceMines();
            UpdateNumbers();

            //Render Texts
            TimerTextBlock.Text = "Timer: " + _seconds.ToString();
            FlaggsPlacedTextBlock.Text = "Flags: " + flagesPlaced.ToString();


        }

        private void Button_Click_Left(Object sender, RoutedEventArgs e)
        {

            // Left mouse button was clicked

            Button btn = (Button)sender;
            Tuple<int, int> cords = (Tuple<int, int>)btn.GetValue(Button.TagProperty);
            int row = cords.Item1;
            int col = cords.Item2;
               
               if (gameBoard[row, col] == -1)
                {

                    GameOver();
                    return;
                }
                Reveal(row, col);
        }

        private void Button_Click_Right(object sender, MouseButtonEventArgs e)
        {



                //Right mouse button was 
                Button btn = (Button)sender;
                Tuple<int, int> cords = (Tuple<int, int>)btn.GetValue(Button.TagProperty);
                int row = cords.Item1;
                int col = cords.Item2;


                if (flagPostions[row, col] == false)

                {

                if (flagesPlaced < MINES)
                {
                    flagPostions[row, col] = true;
                    flagesPlaced++;
                    FlaggsPlacedTextBlock.Text = "Flags: " + flagesPlaced.ToString();

                    // Place Flag on Spot
                    btn.Content = 'X';

                    //Check if a Bomb was detected 
                    if (CheckBombDetected(row, col) == true)
                    {
                        minesDetected++;
                    }


                    if (minesDetected == MINES)
                    {
                        //All Bombs were found

                        GameWon();
                    }
                }   
                else
                 {
                    MessageBox.Show("No more Flags available ! ");
                }
                }

                else
                {

                    // Check if a Bomb was flagged and now wants to be unflagged

                    if (CheckBombDetected(row, col) == true)
                    {
                        minesDetected--;
                    }

                    // Remove the Flag from the Position
                    flagPostions[row, col] = false;
                    flagesPlaced--;

                    btn.Content = " ";
                    FlaggsPlacedTextBlock.Text = "Flags: " + flagesPlaced.ToString();


                }

            

        
        }

        private void PlaceMines()
        {
            int minesPlaced = 0;
            while (minesPlaced < MINES)
            {

                int row = rnd.Next(ROWS);
                int column = rnd.Next(COLS);

                if (gameBoard[row, column] == -1)
                    continue;

                
                gameBoard[row, column] = -1;
                minesPlaced++;
                
            }
        }


      

        private void Reveal(int row, int column)
        {
            if (row < 0 || row >= ROWS || column < 0 || column >= COLS)
                return;

            Button btn = buttons[row, column];

            if (btn.IsEnabled == false)
                return;

            btn.IsEnabled = false;
            btn.Content = (gameBoard[row, column] == 0) ? " " : gameBoard[row, column];

            if (gameBoard[row, column] == 0)
            {
                Reveal(row - 1, column - 1);
                Reveal(row - 1, column);
                Reveal(row - 1, column + 1);
                Reveal(row, column - 1);
                Reveal(row, column + 1);
                Reveal(row + 1, column - 1);
                Reveal(row + 1, column);
                Reveal(row + 1, column + 1);
            }
        }


        private void UpdateNumbers()
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    if (gameBoard[row, col] == -1)
                        continue;

                    int mines = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (!IsValidIndex(row + i, col + j) || (i == 0 && j == 0))
                                continue;

                            if (gameBoard[row + i, col + j] == -1)
                                mines++;
                        }
                    }

                    gameBoard[row, col] = mines;
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _seconds++;
            TimerTextBlock.Text = "Timer: " + _seconds.ToString();
        }

        private bool CheckBombDetected(int row, int column)
        {
            if (gameBoard[row, column] == -1)
            {
                return true;
            }

            return false;
        }


        private void RevealAllNumbers()
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    if (gameBoard[row, col] > 0)
                    {
                        buttons[row, col].IsEnabled = false;
                        buttons[row, col].Content = gameBoard[row,col];
                    }

                    else if (gameBoard[row,col] == 0)
                    {

                        buttons[row, col].IsEnabled = false;
                        buttons[row, col].Content = "";
                    }
                }
            }
        }

        private void RevealAllBombs()
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    if (gameBoard[row, col] == -1)
                    {
                        buttons[row,col].Content = "X";
                    }
                }
            }
        }

        private void ResetBoard()
        {
            _seconds = 0;
            minesDetected = 0;
            flagesPlaced = 0;
            Array.Clear(buttons);
            Array.Clear(gameBoard);
            Array.Clear(flagPostions);
    }


        private void GameOver()
        {
            _timer.Stop();
            //Reveal all Bombs
            RevealAllBombs();
            MessageBox.Show("You Lost, Better luck next time !");
            ResetBoard();
        }

        private void GameWon()
        {
            _timer.Stop();
            //Reveal All Numbers
            RevealAllNumbers();
            MessageBox.Show("Congratulations you have found all Bombs !");
            ResetBoard();
        }
        private bool IsValidIndex(int row, int column)
        {
            return row >= 0 && row < ROWS && column >= 0 && column < COLS;
        }
    }
}
