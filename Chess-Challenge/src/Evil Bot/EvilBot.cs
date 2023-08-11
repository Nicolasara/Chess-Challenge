using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        private bool IsWhite;
        private readonly int[] PieceValues = new int[7] { 0, 1, 3, 3, 5, 9, 0 };

        private int CalculateScore(Board board, bool white)
        {
            int score = 0;
            foreach (PieceType pieceType in Enum.GetValues(typeof(PieceType)))
            {
                if (pieceType == PieceType.None || pieceType == PieceType.King)
                {
                    continue;
                }
                PieceList pieceList = board.GetPieceList(pieceType, white);
                for (int i = 0; i < pieceList.Count; i++)
                {
                    Piece piece = pieceList.GetPiece(i);
                    int pieceValue = PieceValues[(int)piece.PieceType];
                    score += pieceValue;
                }
            }
            return score;
        }
        // public EvaluationData whiteEval;
        // public EvaluationData blackEval;

        // Performs static evaluation of the current position.
        // The position is assumed to be 'quiet', i.e no captures are available that could drastically affect the evaluation.
        // The score that's returned is given from the perspective of whoever's turn it is to move.
        // So a positive score means the player who's turn it is to move has an advantage, while a negative score indicates a disadvantage.
        public double Evaluate(Board board)
        {
            if (board.IsDraw()) { return 0; }
            if (board.IsInCheckmate())
            {
                if (board.IsWhiteToMove == IsWhite)
                {
                    return double.NegativeInfinity;
                }
                else
                {
                    return double.PositiveInfinity;
                }
            }
            int whiteMaterialScore = CalculateScore(board, true);
            int blackMaterialScore = CalculateScore(board, false);

            int perspective = IsWhite ? 1 : -1;

            int eval = whiteMaterialScore - blackMaterialScore;
            return eval * perspective;
        }

        public Move? BestMove(Board board, int depth)
        {
            List<Move> bestMoves = new();
            double maxEvaluation = double.NegativeInfinity;


            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                double currentEvaluation = Minimax(board, depth - 1, double.NegativeInfinity, double.PositiveInfinity, false);
                board.UndoMove(move);
                if (currentEvaluation > maxEvaluation)
                {
                    maxEvaluation = currentEvaluation;
                    bestMoves = new List<Move> { move };
                }
                else if (currentEvaluation == maxEvaluation)
                {
                    bestMoves.Add(move);
                }
            }

            return bestMoves[new Random().Next(bestMoves.Count)];
        }

        private double Minimax(Board board, int depth, double alpha, double beta, bool isMaximizing)
        {
            if (depth == 0 || IsGameOver(board))
            {
                return Evaluate(board);
            }

            if (isMaximizing)
            {
                double maxEval = double.NegativeInfinity;
                foreach (Move move in board.GetLegalMoves())
                {
                    board.MakeMove(move);
                    double eval = Minimax(board, depth - 1, alpha, beta, false);
                    board.UndoMove(move);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                    {
                        break;  // Alpha-beta pruning
                    }
                }
                return maxEval;
            }
            else
            {
                double minEval = double.PositiveInfinity;
                foreach (Move move in board.GetLegalMoves())
                {
                    board.MakeMove(move);
                    double eval = Minimax(board, depth - 1, alpha, beta, true);
                    board.UndoMove(move);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                    {
                        break;  // Alpha-beta pruning
                    }
                }
                return minEval;
            }
        }


        private bool IsGameOver(Board board)
        {
            return board.IsDraw() || board.IsInCheckmate();
        }

        public Move Think(Board board, Timer timer)
        {
            IsWhite = board.IsWhiteToMove;
            int count = BitOperations.PopCount(board.AllPiecesBitboard);
            int depth = 4;
            if (count < 10)
            {
                depth = 6;
            }
            Move? bestMove = BestMove(board, depth);
            if (bestMove.HasValue)
            {
                return bestMove.Value;
            }
            else
            {
                return board.GetLegalMoves().First();
            }
        }
    }
}