using System;
using System.Threading;

namespace CengCheckers
{
    struct Point
    {
        public Point(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public int x;
        public int y;
    }

    public class Program
    {
        // Board sizes and helper variables for rendering.
        const int BoardWidth = 8;
        const int BoardHeight = 8;
        readonly static Point RoundInfoPosition = new Point(25, 1);
        readonly static Point CursorOffset = new Point(2, 2);
        static Point Cursor = new Point(BoardWidth - 2, BoardHeight - 2);

        // Main game variables.
        static char[,] board = new char[BoardHeight, BoardWidth];
        // Turn = true means that it's human's turn.
        static bool Turn = true;
        static int Round = 1;
        static bool GameIsFinished = false;
        readonly static Point UninitializedPoint = new Point(-1, -1);
        static Point SelectedPiece = UninitializedPoint;

        public static void Main(string[] args)
        {
            Point lastMovedPiece = UninitializedPoint;
            bool playerStepped = false;
            bool playerMoved = false;

            // This procedure is called only for once, read `SwapCells()` for more details.
            InitializeAndRenderGame();

            while (!GameIsFinished)
            {

                if (!Turn)
                {
                    DecideMoveForComputer();

                    lastMovedPiece = UninitializedPoint;
                    SelectedPiece = UninitializedPoint;
                    playerStepped = false;
                    playerMoved = false;

                    GameIsFinished = IsGameFinished();
                    if (GameIsFinished) break;

                    NextTurn();
                }
                else
                {
                    Console.SetCursorPosition(2 * (Cursor.x + CursorOffset.x), Cursor.y + CursorOffset.y);

                    ConsoleKey key = Console.ReadKey(true).Key;

                    switch (key)
                    {
                        // User moves the cursor.
                        //
                        // Setting cursor values in this way allows us to link board
                        // edges opposite to their opposites, that means if the cursor
                        // goes out of any edge, it starts from the opposite edge, it
                        // sort of teleports.
                        case ConsoleKey.UpArrow: Cursor.y = (Cursor.y + BoardHeight - 1) % BoardHeight; continue;
                        case ConsoleKey.DownArrow: Cursor.y = (Cursor.y + 1) % BoardHeight; continue;
                        case ConsoleKey.LeftArrow: Cursor.x = (Cursor.x + BoardWidth - 1) % BoardWidth; continue;
                        case ConsoleKey.RightArrow: Cursor.x = (Cursor.x + 1) % BoardWidth; continue;

                        // User selects their pieces.
                        case ConsoleKey.Z:
                            if (board[Cursor.y, Cursor.x] == 'x') SelectedPiece = Cursor;
                            break;

                        // User ends their turn.
                        case ConsoleKey.C:
                            GameIsFinished = IsGameFinished();
                            if (GameIsFinished) break;

                            NextTurn();
                            break;

                        // User tries to move a piece.
                        case ConsoleKey.X:
                            if (SelectedPiece.Equals(UninitializedPoint) || board[Cursor.y, Cursor.x] != '.') continue;

                            int xDist = Math.Abs(SelectedPiece.x - Cursor.x);
                            int yDist = Math.Abs(SelectedPiece.y - Cursor.y);
                            Point mid = new Point((SelectedPiece.x + Cursor.x) / 2, (SelectedPiece.y + Cursor.y) / 2);

                            bool cursorIsAdjacent = (xDist == 1 && yDist == 0) || (yDist == 1 && xDist == 0);
                            bool canJumpToCursor = (yDist == 0 && xDist == 2 && board[Cursor.y, mid.x] != '.') || (xDist == 0 && yDist == 2 && board[mid.y, Cursor.x] != '.');

                            if (canJumpToCursor && (lastMovedPiece.Equals(UninitializedPoint) || SelectedPiece.Equals(lastMovedPiece)) && !playerStepped)
                            {
                                SwapCells(Cursor, SelectedPiece);
                                SelectedPiece = Cursor;
                                lastMovedPiece = Cursor;
                                playerMoved = true;
                            }
                            else if (cursorIsAdjacent && !playerMoved)
                            {
                                SwapCells(Cursor, SelectedPiece);
                                SelectedPiece = Cursor;
                                lastMovedPiece = Cursor;
                                playerMoved = true;
                                playerStepped = true;
                            }
                            break;
                    }
                }
            }

            Console.SetCursorPosition(RoundInfoPosition.x, RoundInfoPosition.y + 2);
            Console.Write($"Winner: { (Turn ? 'x' : 'o') }");
            Console.SetCursorPosition(0, CursorOffset.y + BoardWidth + 1);

            Console.ReadKey();
        }

        static void NextTurn()
        {
            Turn = !Turn;
            Round++;
            PrintRoundAndTurn();
        }

        static bool IsGameFinished()
        {
            if (Turn)
            {
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        if (board[i, j] != 'x') return false;
            }
            else
            {
                for (int i = BoardHeight - 3; i < BoardHeight; i++)
                    for (int j = BoardWidth - 3; j < BoardWidth; j++)
                        if (board[i, j] != 'o') return false;
            }

            return true;
        }

        static void InitializeAndRenderGame()
        {
            for (int i = 0; i < BoardHeight; i++)
            {
                for (int j = 0; j < BoardWidth; j++)
                {
                    if (i < 3 && j < 3)
                        board[i, j] = 'o';
                    else if (i > 4 && j > 4)
                        board[i, j] = 'x';
                    else
                        board[i, j] = '.';
                }
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine("    1 2 3 4 5 6 7 8");
            Console.WriteLine("  +-----------------+");
            for (int i = 0; i < BoardHeight; i++)
            {
                Console.Write((i + 1) + " | ");
                for (int j = 0; j < BoardWidth; j++)
                {
                    Console.Write(board[i, j] + " ");
                }
                Console.WriteLine("|");
            }
            Console.WriteLine("  +-----------------+");

            PrintRoundAndTurn();
        }

        static void PrintRoundAndTurn()
        {
            Point position = new Point(Console.CursorLeft, Console.CursorTop);

            Console.SetCursorPosition(RoundInfoPosition.x, RoundInfoPosition.y);
            Console.Write("Round: " + Round);
            Console.SetCursorPosition(RoundInfoPosition.x, RoundInfoPosition.y + 1);
            Console.Write("Turn: " + (Turn ? 'x' : 'o'));

            Console.SetCursorPosition(position.x, position.y);
        }

        static void SwapCells(Point cell1, Point cell2)
        {
            char temp = board[cell1.y, cell1.x];
            board[cell1.y, cell1.x] = board[cell2.y, cell2.x];
            board[cell2.y, cell2.x] = temp;

            // Since the only dynamic event in this game is the movement of the pieces,
            // by only rendering changes on swapped cells, we can achieve correct rendering of the
            // game board without any overhead. We avoid overheads because rendering the
            // board from scratch every time is expensive.
            Console.SetCursorPosition(2 * (CursorOffset.x + cell1.x), CursorOffset.y + cell1.y);
            Console.Write(board[cell1.y, cell1.x]);
            Console.SetCursorPosition(2 * (CursorOffset.x + cell2.x), CursorOffset.y + cell2.y);
            Console.Write(board[cell2.y, cell2.x]);
        }

        static bool IsEmpty(Point cell)
        {
            return 0 <= cell.x && cell.x < BoardWidth
               && 0 <= cell.y && cell.y < BoardHeight
               && board[cell.y, cell.x] == '.';
        }

        static Point AddPoints(Point a, Point b)
        {
            return new Point(a.x + b.x, a.y + b.y);
        }

        static int Distance(Point a, Point b)
        {
            return Math.Abs(a.x - b.x + a.y - b.y);
        }

        // [Point, Point] is a move sequence where the items are absolute coordinates, or points.
        // The first item can not be UninitializedPoint when the second item is a valid Point.
        //
        // This function prioritizes double jumps over single jumps, single jumps over steps.
        //
        // To avoid stacks of computer pieces more than three, we don't make moves to axes
        // that contain maximum of three computer pieces.
        static Point[] CalculateMoveForPiece(Point piece)
        {
            Point[] moves = { UninitializedPoint, UninitializedPoint };

            Point stepRight = new Point(1, 0);
            Point stepBottom = new Point(0, 1);
            Point jumpRight = new Point(2, 0);
            Point jumpBottom = new Point(0, 2);

            Point adjacentRight = AddPoints(piece, stepRight);
            Point adjacentBottom = AddPoints(piece, stepBottom);
            Point distantRight = AddPoints(piece, jumpRight);
            Point distantBottom = AddPoints(piece, jumpBottom);

            // Try to jump.
            if (!IsEmpty(adjacentRight) && IsEmpty(distantRight) && !DidColumnExceedThreshold(distantRight.x))
                moves[0] = distantRight;
            else if (!IsEmpty(adjacentBottom) && IsEmpty(distantBottom) && !DidRowExceedThreshold(distantBottom.y))
                moves[0] = distantBottom;

            // If jumped, try to jump once more.
            if (!moves[0].Equals(UninitializedPoint))
            {
                Point nextJumpRight = AddPoints(moves[0], jumpRight);
                Point nextJumpBottom = AddPoints(moves[0], jumpBottom);

                if (!IsEmpty(AddPoints(moves[0], stepRight)) && IsEmpty(nextJumpRight) && !DidColumnExceedThreshold(nextJumpRight.x))
                    moves[1] = nextJumpRight;
                else if (!IsEmpty(AddPoints(moves[0], stepBottom)) && IsEmpty(nextJumpBottom) && !DidRowExceedThreshold(nextJumpBottom.y))
                    moves[1] = nextJumpBottom;

                return moves;
            }

            // Try to step, if none of the above is successful.
            if (IsEmpty(adjacentRight) && !DidColumnExceedThreshold(adjacentRight.x))
                moves[0] = adjacentRight;
            else if (IsEmpty(adjacentBottom) && !DidRowExceedThreshold(adjacentBottom.y))
                moves[0] = adjacentBottom;

            return moves;
        }

        // Check if the row has three or more computer pieces.
        static bool DidRowExceedThreshold(int y)
        {
            int counter = 0;
            for (int x = 0; x < BoardWidth; x++)
                if (board[y, x] == 'o') counter++;
            return counter >= 3;
        }

        // Check if the column has three or more computer pieces.
        static bool DidColumnExceedThreshold(int x)
        {
            int counter = 0;
            for (int y = 0; y < BoardHeight; y++)
                if (board[y, x] == 'o') counter++;
            return counter >= 3;
        }

        static Point[] FindComputerPieces()
        {
            Point[] pieces = new Point[9];
            int index = 0;

            for (int j = 0; j < BoardWidth; j++)
                for (int i = 0; i < BoardHeight; i++)
                {
                    if (board[i, j] != 'o') continue;
                    pieces[index] = new Point(j, i);
                    index++;
                }

            return pieces;
        }

        static void DecideMoveForComputer()
        {
            Point[] pieces = FindComputerPieces();

            int bestBoardwiseGain = 0;
            int bestPiecewiseGain = 0;
            Point bestPiece = UninitializedPoint;
            Point[] bestMove = { UninitializedPoint, UninitializedPoint };

            foreach (Point piece in pieces)
            {
                Point[] moves = CalculateMoveForPiece(piece);

                foreach (Point destination in moves)
                {
                    if (destination.Equals(UninitializedPoint)) continue;

                    // Priorizite best moves to their alternatives that are closer
                    // to bottom-right of the board.
                    int boardwiseGain = destination.x + destination.y;
                    int piecewiseGain = Distance(piece, destination);

                    if (piecewiseGain > bestPiecewiseGain || (piecewiseGain == bestPiecewiseGain && boardwiseGain >= bestBoardwiseGain))
                    {
                        bestBoardwiseGain = boardwiseGain;
                        bestPiecewiseGain = piecewiseGain;
                        bestPiece = piece;
                        bestMove = moves;
                    }
                }
            }

            foreach (Point destination in bestMove)
            {
                if (destination.Equals(UninitializedPoint)) continue;

                SwapCells(bestPiece, destination);
                bestPiece = destination;
                Thread.Sleep(500);
            }
        }
    }
}
