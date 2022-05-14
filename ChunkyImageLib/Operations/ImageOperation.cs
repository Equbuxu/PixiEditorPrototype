﻿using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;

internal record class ImageOperation : IDrawOperation
{
    private SKMatrix transformMatrix;
    private ShapeCorners corners;
    private Surface toPaint;
    private bool imageWasCopied = false;
    private static SKPaint ReplacingPaint = new() { BlendMode = SKBlendMode.Src };

    public bool IgnoreEmptyChunks => false;

    public ImageOperation(Vector2i pos, Surface image, bool copyImage = true)
    {
        corners = new()
        {
            TopLeft = pos,
            TopRight = new(pos.X + image.Size.X, pos.Y),
            BottomRight = pos + image.Size,
            BottomLeft = new Vector2d(pos.X, pos.Y + image.Size.Y)
        };
        transformMatrix = SKMatrix.CreateIdentity();
        transformMatrix.TransX = pos.X;
        transformMatrix.TransY = pos.Y;

        // copying is needed for thread safety
        if (copyImage)
            toPaint = new Surface(image);
        else
            toPaint = image;
        imageWasCopied = copyImage;
    }

    public ImageOperation(ShapeCorners corners, Surface image, bool copyImage = true)
    {
        this.corners = corners;
        transformMatrix = OperationHelper.CreateMatrixFromPoints(corners, image.Size);

        // copying is needed for thread safety
        if (copyImage)
            toPaint = new Surface(image);
        else
            toPaint = image;
        imageWasCopied = copyImage;
    }

    public void DrawOnChunk(Chunk chunk, Vector2i chunkPos)
    {
        float scaleMult = (float)chunk.Resolution.Multiplier();
        Vector2d trans = -chunkPos * ChunkPool.FullChunkSize;

        var scaleTrans = SKMatrix.CreateScaleTranslation(scaleMult, scaleMult, (float)trans.X * scaleMult, (float)trans.Y * scaleMult);
        var finalMatrix = SKMatrix.Concat(scaleTrans, transformMatrix);

        chunk.Surface.SkiaSurface.Canvas.Save();
        chunk.Surface.SkiaSurface.Canvas.SetMatrix(finalMatrix);
        chunk.Surface.SkiaSurface.Canvas.DrawSurface(toPaint.SkiaSurface, 0, 0, ReplacingPaint);
        chunk.Surface.SkiaSurface.Canvas.Restore();
    }

    public HashSet<Vector2i> FindAffectedChunks()
    {
        return OperationHelper.FindChunksTouchingQuadrilateral(corners, ChunkPool.FullChunkSize);
    }

    public void Dispose()
    {
        if (imageWasCopied)
            toPaint.Dispose();
    }
}
