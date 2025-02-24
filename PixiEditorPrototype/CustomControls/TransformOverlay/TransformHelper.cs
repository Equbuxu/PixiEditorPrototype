﻿using System;
using System.Windows;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;
internal static class TransformHelper
{
    public const double AnchorSize = 10;
    public const double MoveHandleSize = 16;

    public static Rect ToAnchorRect(VecD pos, double zoomboxScale)
    {
        double scaled = AnchorSize / zoomboxScale;
        return new Rect(pos.X - scaled / 2, pos.Y - scaled / 2, scaled, scaled);
    }

    public static Rect ToHandleRect(VecD pos, double zoomboxScale)
    {
        double scaled = MoveHandleSize / zoomboxScale;
        return new Rect(pos.X - scaled / 2, pos.Y - scaled / 2, scaled, scaled);
    }

    public static VecD ToVecD(Point pos) => new VecD(pos.X, pos.Y);
    public static Point ToPoint(VecD vec) => new Point(vec.X, vec.Y);

    public static ShapeCorners SnapToPixels(ShapeCorners corners)
    {
        corners.TopLeft = corners.TopLeft.Round();
        corners.TopRight = corners.TopRight.Round();
        corners.BottomLeft = corners.BottomLeft.Round();
        corners.BottomRight = corners.BottomRight.Round();
        return corners;
    }

    private static double GetSnappingAngle(double angle)
    {
        return Math.Round(angle * 8 / (Math.PI * 2)) * (Math.PI * 2) / 8;
    }
    public static double FindSnappingAngle(ShapeCorners corners, double desiredAngle)
    {
        var desTop = (corners.TopLeft - corners.TopRight).Rotate(desiredAngle).Angle;
        var desRight = (corners.TopRight - corners.BottomRight).Rotate(desiredAngle).Angle;
        var desBottom = (corners.BottomRight - corners.BottomLeft).Rotate(desiredAngle).Angle;
        var desLeft = (corners.BottomLeft - corners.TopLeft).Rotate(desiredAngle).Angle;

        var deltaTop = GetSnappingAngle(desTop) - desTop;
        var deltaRight = GetSnappingAngle(desRight) - desRight;
        var deltaLeft = GetSnappingAngle(desLeft) - desLeft;
        var deltaBottom = GetSnappingAngle(desBottom) - desBottom;

        var minDelta = deltaTop;
        if (Math.Abs(minDelta) > Math.Abs(deltaRight))
            minDelta = deltaRight;
        if (Math.Abs(minDelta) > Math.Abs(deltaLeft))
            minDelta = deltaLeft;
        if (Math.Abs(minDelta) > Math.Abs(deltaBottom))
            minDelta = deltaBottom;
        return minDelta + desiredAngle;
    }

    public static VecD OriginFromCorners(ShapeCorners corners)
    {
        var maybeOrigin = TwoLineIntersection(
            GetAnchorPosition(corners, Anchor.Top),
            GetAnchorPosition(corners, Anchor.Bottom),
            GetAnchorPosition(corners, Anchor.Left),
            GetAnchorPosition(corners, Anchor.Right)
            );
        return maybeOrigin ?? corners.TopLeft.Lerp(corners.BottomRight, 0.5);
    }

    public static VecD? TwoLineIntersection(VecD line1Start, VecD line1End, VecD line2Start, VecD line2End)
    {
        const double epsilon = 0.0001;

        VecD line1delta = line1End - line1Start;
        VecD line2delta = line2End - line2Start;

        // both lines are vertical, no intersections
        if (Math.Abs(line1delta.X) < epsilon && Math.Abs(line2delta.X) < epsilon)
            return null;

        // y = mx + c
        double m1 = line1delta.Y / line1delta.X;
        double m2 = line2delta.Y / line2delta.X;

        // line 1 is vertical (m1 is infinity)
        if (Math.Abs(line1delta.X) < epsilon)
        {
            double c2 = line2Start.Y - line2Start.X * m2;
            return new(line1Start.X, m2 * line1Start.X + c2);
        }

        // line 2 is vertical
        if (Math.Abs(line2delta.X) < epsilon)
        {
            double c1 = line1Start.Y - line1Start.X * m1;
            return new(line2Start.X, m1 * line2Start.X + c1);
        }

        // lines are parallel
        if (Math.Abs(m1 - m2) < epsilon)
            return null;

        {
            double c1 = line1Start.Y - line1Start.X * m1;
            double c2 = line2Start.Y - line2Start.X * m2;
            double x = (c1 - c2) / (m2 - m1);
            return new(x, m1 * x + c1);
        }
    }

    public static bool IsCorner(Anchor anchor)
    {
        return anchor is Anchor.TopLeft or Anchor.TopRight or Anchor.BottomRight or Anchor.BottomLeft;
    }

    public static bool IsSide(Anchor anchor)
    {
        return anchor is Anchor.Left or Anchor.Right or Anchor.Top or Anchor.Bottom;
    }

    public static Anchor GetOpposite(Anchor anchor)
    {
        return anchor switch
        {
            Anchor.TopLeft => Anchor.BottomRight,
            Anchor.TopRight => Anchor.BottomLeft,
            Anchor.BottomLeft => Anchor.TopRight,
            Anchor.BottomRight => Anchor.TopLeft,
            Anchor.Top => Anchor.Bottom,
            Anchor.Left => Anchor.Right,
            Anchor.Right => Anchor.Left,
            Anchor.Bottom => Anchor.Top,
            _ => throw new ArgumentException($"{anchor} is not a corner or a side"),
        };
    }

    /// <summary>
    /// The first anchor would be on your left if you were standing on the side and looking inside the shape; the second anchor is to the right.
    /// </summary>
    public static (Anchor leftAnchor, Anchor rightAnchor) GetCornersOnSide(Anchor side)
    {
        return side switch
        {
            Anchor.Left => (Anchor.TopLeft, Anchor.BottomLeft),
            Anchor.Right => (Anchor.BottomRight, Anchor.TopRight),
            Anchor.Top => (Anchor.TopRight, Anchor.TopLeft),
            Anchor.Bottom => (Anchor.BottomLeft, Anchor.BottomRight),
            _ => throw new ArgumentException($"{side} is not a side anchor"),
        };
    }

    /// <summary>
    /// The first corner would be on your left if you were standing on the passed corner and looking inside the shape; the second corner is to the right.
    /// </summary>
    public static (Anchor, Anchor) GetNeighboringCorners(Anchor corner)
    {
        return corner switch
        {
            Anchor.TopLeft => (Anchor.TopRight, Anchor.BottomLeft),
            Anchor.TopRight => (Anchor.BottomRight, Anchor.TopLeft),
            Anchor.BottomLeft => (Anchor.TopLeft, Anchor.BottomRight),
            Anchor.BottomRight => (Anchor.BottomLeft, Anchor.TopRight),
            _ => throw new ArgumentException($"{corner} is not a corner anchor"),
        };
    }

    public static ShapeCorners UpdateCorner(ShapeCorners original, Anchor corner, VecD newPos)
    {
        if (corner == Anchor.TopLeft)
            original.TopLeft = newPos;
        else if (corner == Anchor.BottomLeft)
            original.BottomLeft = newPos;
        else if (corner == Anchor.TopRight)
            original.TopRight = newPos;
        else if (corner == Anchor.BottomRight)
            original.BottomRight = newPos;
        else
            throw new ArgumentException($"{corner} is not a corner");
        return original;
    }

    public static VecD GetAnchorPosition(ShapeCorners corners, Anchor anchor)
    {
        return anchor switch
        {
            Anchor.TopLeft => corners.TopLeft,
            Anchor.BottomRight => corners.BottomRight,
            Anchor.TopRight => corners.TopRight,
            Anchor.BottomLeft => corners.BottomLeft,
            Anchor.Top => corners.TopLeft.Lerp(corners.TopRight, 0.5),
            Anchor.Bottom => corners.BottomLeft.Lerp(corners.BottomRight, 0.5),
            Anchor.Left => corners.TopLeft.Lerp(corners.BottomLeft, 0.5),
            Anchor.Right => corners.BottomRight.Lerp(corners.TopRight, 0.5),
            _ => throw new ArgumentException($"{anchor} is not a corner or a side"),
        };
    }

    public static Anchor? GetAnchorInPosition(VecD pos, ShapeCorners corners, VecD origin, double zoomboxScale)
    {
        VecD topLeft = corners.TopLeft;
        VecD topRight = corners.TopRight;
        VecD bottomLeft = corners.BottomLeft;
        VecD bottomRight = corners.BottomRight;

        // corners
        if (IsWithinAnchor(topLeft, pos, zoomboxScale))
            return Anchor.TopLeft;
        if (IsWithinAnchor(topRight, pos, zoomboxScale))
            return Anchor.TopRight;
        if (IsWithinAnchor(bottomLeft, pos, zoomboxScale))
            return Anchor.BottomLeft;
        if (IsWithinAnchor(bottomRight, pos, zoomboxScale))
            return Anchor.BottomRight;

        // sides
        if (IsWithinAnchor((bottomLeft - topLeft) / 2 + topLeft, pos, zoomboxScale))
            return Anchor.Left;
        if (IsWithinAnchor((bottomRight - topRight) / 2 + topRight, pos, zoomboxScale))
            return Anchor.Right;
        if (IsWithinAnchor((topLeft - topRight) / 2 + topRight, pos, zoomboxScale))
            return Anchor.Top;
        if (IsWithinAnchor((bottomLeft - bottomRight) / 2 + bottomRight, pos, zoomboxScale))
            return Anchor.Bottom;

        // origin
        if (IsWithinAnchor(origin, pos, zoomboxScale))
            return Anchor.Origin;
        return null;
    }

    public static bool IsWithinAnchor(VecD anchorPos, VecD mousePos, double zoomboxScale)
    {
        var delta = (anchorPos - mousePos).Abs();
        double scaled = AnchorSize / zoomboxScale / 2;
        return delta.X < scaled && delta.Y < scaled;
    }

    public static bool IsWithinTransformHandle(VecD handlePos, VecD mousePos, double zoomboxScale)
    {
        var delta = (handlePos - mousePos).Abs();
        double scaled = MoveHandleSize / zoomboxScale / 2;
        return delta.X < scaled && delta.Y < scaled;
    }

    public static VecD GetDragHandlePos(ShapeCorners corners, double zoomboxScale)
    {
        VecD max = new(
            Math.Max(Math.Max(corners.TopLeft.X, corners.TopRight.X), Math.Max(corners.BottomLeft.X, corners.BottomRight.X)),
            Math.Max(Math.Max(corners.TopLeft.Y, corners.TopRight.Y), Math.Max(corners.BottomLeft.Y, corners.BottomRight.Y)));
        return max + new VecD(MoveHandleSize / zoomboxScale, MoveHandleSize / zoomboxScale);
    }
}
