﻿using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Services
{
    public class MoveFormatter: IMoveFormatter
    {
        #region Implementation of IMoveFormatter

        public string Format(MoveBase move)
        {
            var format = $"{FormatInternal(move)}";
            return move.IsCheck ? $"{format}+" : format;
        }

        private string FormatInternal(MoveBase move)
        {
            if (move is Attack attack)
            {
                return FormatAttack(attack);
            }

            if (move is SmallCastle)
            {
                return "0 - 0";
            }

            if (move is BigCastle)
            {
                return "0 - 0 - 0";
            }

            return FormatMove(move);
        }

        private string FormatMove(MoveBase move)
        {
            var figure = GetFigure(move);

            return $"{figure} {move.From.AsString()} - {move.To.AsString()}";
        }

        private string FormatAttack(Attack attack)
        {
            var figure = GetFigure(attack);

            return $"{figure} {attack.From.AsString()} x {attack.To.AsString()}";
        }

        #endregion

        private static string GetFigure(MoveBase m)
        {
            string figure = string.Empty;

            switch (m.Piece)
            {
                case Pieces.WhiteKing:
                case Pieces.BlackKing:
                    figure = "K";
                    break;
                case Pieces.WhiteKnight:
                case Pieces.BlackKnight:
                    figure = "N";
                    break;
                case Pieces.WhiteBishop:
                case Pieces.BlackBishop:
                    figure = "B";
                    break;
                case Pieces.WhiteRook:
                case Pieces.BlackRook:
                    figure = "R";
                    break;
                case Pieces.WhiteQueen:
                case Pieces.BlackQueen:
                    figure = "Q";
                    break;
            }

            return figure;
        }
    }
}
