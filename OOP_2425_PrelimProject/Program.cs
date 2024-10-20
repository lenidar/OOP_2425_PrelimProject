using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace OOP_2425_PrelimProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[,] board = new string[3, 3];
            Queue<int[]> playerMoves = new Queue<int[]>();
            Queue<int[]> computerMoves = new Queue<int[]>();
            int[] liveCoor = new int[2];
            int[] idealMove = new int[2];
            int winner = 0;
            bool playerTurn = true;

            int[] score = new int[] { 0, 0 };

            board = ResetBoard(board);
            DisplayBoard(board, score);
            while (true)
            {
                //board = ResetBoard(board); 
                //board = updateBoard(board, playerMoves, true);
                //board = updateBoard(board, computerMoves, false);

                if (playerTurn)
                {
                    playerMoves = updateQueue(playerMoves, ValidateMove(board));
                    board = ResetBoard(board);
                    board = updateBoard(board, playerMoves, true);
                    playerTurn = false;
                }
                else
                {
                    computerMoves = ComputerMove(board, computerMoves, playerMoves);
                    board = updateBoard(board, computerMoves, false);
                    playerTurn = true;
                }

                Console.Clear();
                DisplayBoard(board, score);
                //Console.ReadKey();

                if (AssesBoard(board, out winner))
                {
                    Console.Write("    Congratulations to the ");
                    if (winner == 1)
                    {
                        Console.WriteLine("Player for winning! Press any key to continue...");
                        score[0]++;
                        playerTurn = true;
                    }
                    else
                    {
                        Console.WriteLine("Computer for winning! Press any key to continue...");
                        score[1]++;
                        playerTurn = false;
                    }
                    Console.ReadKey();
                    Console.Clear();
                    playerMoves.Clear();
                    computerMoves.Clear();
                    board = ResetBoard(board);
                    DisplayBoard(board, score);
                }
            }
            Console.ReadKey();
        }

        static void DisplayBoard(string[,] board, int[] score)
        {
            int move = 2;
            int rowDisp = 0;

            Console.WriteLine($"    Player - {score[0]}\tComputer - {score[1]}\n");

            Console.WriteLine("       0     1     2   ");
            for(int x = 0; x < 13; x++)
            {
                if(x % 4 == 0)
                    Console.WriteLine($"    +-----+-----+-----+");
                else if (x == move)
                {
                    Console.WriteLine($"{rowDisp}   |  {board[rowDisp, 0]}  |  {board[rowDisp, 1]}  |  {board[rowDisp, 2]}  |");
                    move += 4;
                    rowDisp++;
                }
                else
                    Console.WriteLine($"    |     |     |     |");
            }
        }

        static string[,] ResetBoard(string[,] board)
        {
            for (int x = 0; x < board.GetLength(0); x++)
                for (int y = 0; y < board.GetLength(1); y++)
                    board[x, y] = $" ";

            return board;
        }

        static int[] PlayerMove()
        {
            string userInput = "";
            string[] unformat = new string[] { };
            int[] coord = new int[2];

            Console.WriteLine("\n    Player Move : <row>,<column>");
            Console.Write("    ");

            userInput = Console.ReadLine();
            unformat = userInput.Split(',');

            if(unformat.Length == coord.Length)
            {
                if (int.TryParse(unformat[0], out coord[0]) && int.TryParse(unformat[1], out coord[1]))
                {
                    if (!(coord[0] >= 0 && coord[0] < 3 && coord[1] >= 0 && coord[1] < 3))
                    {
                        Console.WriteLine("    False coordinates. Please try again...");
                        coord = PlayerMove();
                    }
                }
                else
                {
                    Console.WriteLine("    Non-number detected. Please try again...");
                    coord = PlayerMove();
                }
            }
            else
            {
                Console.WriteLine("    Format not followed. Please try again...");
                coord = PlayerMove();
            }

            return coord;
        }

        static int[] ValidateMove(string[,] board)
        {
            int[] liveCoor = PlayerMove();
            if (board[liveCoor[0], liveCoor[1]] == " ")
            {
                return liveCoor;
            }
            else
            {
                Console.WriteLine("    There is already something in the targeted space. Try again...\n");
                return ValidateMove(board);
            }
        }

        static Queue<int[]> updateQueue(Queue<int[]> move, int[] coords)
        {
            move.Enqueue(coords);
            if (move.Count > 3)
            {
                Console.WriteLine("Removing a move from queue");
                move.Dequeue();
            }
            return move;
        }

        static string[,] updateBoard(string[,] board, Queue<int[]> Moves, bool player)
        {
            string move = "X";
            //board = ResetBoard(board);
            if (player)
                move = "0";

            for(int x = 0; x < Moves.Count; x++)
            {
                board[Moves.Peek()[0], Moves.Peek()[1]] = move;
                Moves.Enqueue(Moves.Dequeue());
            }

            return board;
        }

        static Queue<int[]> ComputerMove(string[,] board, Queue<int[]> computerMoves, Queue<int[]> playerMoves)
        {
            List<int[]> availableSpaces = new List<int[]>();
            int[] idealBlock = new int[] { -1, -1 };
            int[] idealAttack = new int[] { -1, -1 };
            bool block = false;
            bool attack = false;
            Queue<int[]> temp = new Queue<int[]>(computerMoves);
            Queue<int[]> temp2 = new Queue<int[]>(playerMoves);
            List<int[]> playerHistory = new List<int[]>(playerMoves);
            List<int[]> computerHistory = new List<int[]>(computerMoves);
            Random rnd = new Random();

            Console.WriteLine("    Computer is assessing the players moves...");
            block = AssesBoard(temp2, out idealBlock);
            Console.WriteLine("    Computer is assessing its own moves...");
            attack = AssesBoard(temp, out idealAttack);

            temp = new Queue<int[]>(computerMoves);

            if (block)
                if (manualContains(computerHistory, idealBlock))
                    block = false;

            temp2 = new Queue<int[]>(playerMoves);

            if (attack)
                if(manualContains(playerHistory, idealAttack))
                    attack = false;


            if (!attack && !block)
            {

                temp = new Queue<int[]>(computerMoves);
                temp2 = new Queue<int[]>(playerMoves);

                // scan the board for available spaces.
                for (int x = 0; x < board.GetLength(0); x++)
                    for (int y = 0; y < board.GetLength(1); y++)
                        if(!manualContains(computerHistory,new int[] { x, y }) && !manualContains(playerHistory, new int[] { x, y }))
                            availableSpaces.Add(new int[] { x, y });

                //for (int x = 0; x < temp2.Count; x++)
                //    availableSpaces = manualRemove(availableSpaces, temp2.Dequeue());
                //for (int x = 0; x < temp.Count; x++)
                //    availableSpaces = manualRemove(availableSpaces, temp.Dequeue());

                // select random space lol
                idealAttack = availableSpaces[rnd.Next(availableSpaces.Count)];
                //Console.WriteLine($"Computer is randomly moving on {idealAttack[0]},{idealAttack[1]}");
                return updateQueue(computerMoves, idealAttack);
            }
            else
            {
                if (attack)
                {
                    //Console.WriteLine($"Computer is attacking on {idealAttack[0]},{idealAttack[1]}");
                    return updateQueue(computerMoves, idealAttack);
                }
                else
                {
                    //Console.WriteLine($"Computer is blocking on {idealBlock[0]},{idealBlock[1]}");
                    return updateQueue(computerMoves, idealBlock);
                }
            }


        }

        static bool AssesBoard(string[,] board, out int winner)
        {
            bool trueWin = false;
            int win = 0; // 0 = no one, 1 = player, 2 = computer

            // check for winners
            // checks for rows
            for (int x = 0; x < board.GetLength(0); x++)
            {
                if (board[x, 0] != " ")
                {
                    if (board[x,0] == board[x,1] && board[x, 0] == board[x, 2])
                    {
                        trueWin = true;
                        if (board[x, 0] == "X")
                            win = 2;
                        else if (board[x, 0] == "0")
                            win = 1;
                    }
                }
                if (trueWin)
                    break;
            }
            // check for columns
            if(!trueWin)
            {
                for(int x = 0; x < board.GetLength(1); x++)
                {
                    if (board[0, x] != " ")
                    {
                        if (board[0, x] == board[1, x] && board[0, x] == board[2, x])
                        {
                            trueWin = true;
                            if (board[0, x] == "X")
                                win = 2;
                            else if (board[0, x] == "0")
                                win = 1;
                        }
                    }
                    if (trueWin)
                        break;
                }
            }
            // check for diagonals
            if(!trueWin)
            {
                if (board[1,1] != " ")
                {
                    if (board[1, 1] == "X")
                        win = 2;
                    else if(board[1, 1] == "0")
                        win = 1;

                    if (board[0,0] == board[1,1] && board[1,1] == board[2,2])
                        trueWin = true;
                    else if (board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
                        trueWin = true;
                }

            }

            winner = win;
            return trueWin;
        }

        static bool AssesBoard(Queue<int[]> moveList, out int[] idealMove)
        {
            idealMove = new int[] { -1, -1};
            int[] anchor = new int[2];
            List<int[]> leftDiagonals = new List<int[]>();
            List<int[]> rightDiagonals = new List<int[]>();
            moveList = new Queue<int[]>(moveList);
            List<int[]> movePool = new List<int[]>();

            leftDiagonals.Add(new int[] { 0, 0 });
            leftDiagonals.Add(new int[] { 1, 1 });
            leftDiagonals.Add(new int[] { 2, 2 });
            rightDiagonals.Add(new int[] { 0, 2 });
            rightDiagonals.Add(new int[] { 1, 1 });
            rightDiagonals.Add(new int[] { 2, 0 });

            if (moveList.Count == 3)
                moveList.Dequeue();

            if (moveList.Count < 2)
                return false;

            movePool = new List<int[]>(moveList);

            //anchor = moveList.Dequeue();
            if (movePool[0][0] == movePool[1][0]) // going for row finish
            {
                idealMove[0] = movePool[0][0];
                for (int x = 0; x < 3; x++)
                    if (x != movePool[0][1] && x != movePool[1][1])
                    {
                        idealMove[1] = x;
                        return true;
                    }
            }
            else if(movePool[0][1] == movePool[1][1]) // going for column finish
            {
                idealMove[1] = movePool[0][1];
                for (int x = 0; x < 3; x++)
                    if (x != movePool[0][0] && x != movePool[1][0])
                    {
                        idealMove[0] = x;
                        return true;
                    }
            }
            else // check for diagonal finish
            {
                if (manualContains(leftDiagonals,movePool[0]) && manualContains(leftDiagonals, movePool[1]))
                {
                    //Console.WriteLine("Left Diagonal Block");
                    leftDiagonals = manualRemove(leftDiagonals, movePool[0]);
                    leftDiagonals = manualRemove(leftDiagonals, movePool[1]);
                    //leftDiagonals.Remove(movePool[0]);
                    //leftDiagonals.Remove(movePool[1]);
                    idealMove = leftDiagonals[0];
                    return true;
                }
                else if(manualContains(rightDiagonals, movePool[0]) && manualContains(rightDiagonals, movePool[1]))
                {
                    //Console.WriteLine("Right Diagonal Block");
                    rightDiagonals = manualRemove(rightDiagonals, movePool[0]);
                    rightDiagonals = manualRemove(rightDiagonals, movePool[1]);
                    //rightDiagonals.Remove(movePool[0]);
                    //rightDiagonals.Remove(movePool[1]);
                    idealMove = rightDiagonals[0];
                    return true;
                }
            }

            return false;
        }

        static void newWrite(string input, int mode, int consoleSize = 100)
        {
            int offset = 0;
            string newString = "";
            switch (mode)
            {
                case 0:
                    //Console.WriteLine(input);
                    offset = 0;
                    break;
                case 1:
                    //Console.WriteLine(center(input,consoleSize));
                    offset = (consoleSize / 2) - (input.Length / 2);
                    break;
                case 2:
                    //Console.WriteLine(rightJustify(input, consoleSize));
                    offset = consoleSize - input.Length;
                    break;
            }



            for (int x = 0; x < offset; x++)
                newString += " "; ;

            Console.WriteLine(newString + input);
        }

        static List<int[]> manualRemove(List<int[]> list, int[] valueToRemove)
        {
            int x = 0;
            bool remove = false;

            for(x = 0; x < list.Count; x++)
            {
                if (list[x][0] == valueToRemove[0] && list[x][1] == valueToRemove[1])
                {
                    remove = true;
                    break;
                }
            }

            if(remove)
                list.RemoveAt(x);

            return list;
        }

        static bool manualContains(List<int[]> list, int[] valueToFind)
        {
            foreach (int[] thing in list)
            {
                if (thing[0] == valueToFind[0] && thing[1] == valueToFind[1])
                    return true;
            }

            return false;
        }
    }
}
