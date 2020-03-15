using System;

namespace Battleship.Domain.GridDomain
{
    public static class GridCoordinateArrayExtensions
    {
        public static bool HasAnyOutOfBounds(this GridCoordinate[] coordinates, int gridSize)
        {
            throw new NotImplementedException("HasAnyOutOfBounds method of GridCoordinateArrayExtensions class is not implemented");
        }

        public static bool AreAligned(this GridCoordinate[] coordinates)
        {
            throw new NotImplementedException("AreAligned method of GridCoordinateArrayExtensions class is not implemented");
        }

        public static bool AreHorizontallyAligned(this GridCoordinate[] coordinates)
        {
            throw new NotImplementedException("AreHorizontallyAligned method of GridCoordinateArrayExtensions class is not implemented");
        }

        public static bool AreVerticallyAligned(this GridCoordinate[] coordinates)
        {
            throw new NotImplementedException("AreVerticallyAligned method of GridCoordinateArrayExtensions class is not implemented");
        }

        public static bool AreLinked(this GridCoordinate[] coordinates)
        {
            throw new NotImplementedException("AreLinked method of GridCoordinateArrayExtensions class is not implemented");
        }

        public static string Print(this GridCoordinate[] coordinates)
        {
            return $"[{string.Join<GridCoordinate>(", ", coordinates)}]";
        }
    }
}