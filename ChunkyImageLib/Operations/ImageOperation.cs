﻿using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;

internal class ImageOperation : IDrawOperation
{
    private SKMatrix transformMatrix;
    private ShapeCorners corners;
    private Surface toPaint;
    private bool imageWasCopied = false;
    private readonly SKPaint? customPaint;

    public bool IgnoreEmptyChunks => false;

    public ImageOperation(VecI pos, Surface image, SKPaint? paint = null, bool copyImage = true)
    {
        if (paint is not null)
            customPaint = paint.Clone();

        corners = new()
        {
            TopLeft = pos,
            TopRight = new(pos.X + image.Size.X, pos.Y),
            BottomRight = pos + image.Size,
            BottomLeft = new VecD(pos.X, pos.Y + image.Size.Y)
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

    public ImageOperation(ShapeCorners corners, Surface image, SKPaint? paint = null, bool copyImage = true)
    {
        if (paint is not null)
            customPaint = paint.Clone();

        this.corners = corners;
        transformMatrix = OperationHelper.CreateMatrixFromPoints(corners, image.Size);

        // copying is needed for thread safety
        if (copyImage)
            toPaint = new Surface(image);
        else
            toPaint = image;
        imageWasCopied = copyImage;
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        //customPaint.FilterQuality = chunk.Resolution != ChunkResolution.Full;
        float scaleMult = (float)chunk.Resolution.Multiplier();
        VecD trans = -chunkPos * ChunkPool.FullChunkSize;

        var scaleTrans = SKMatrix.CreateScaleTranslation(scaleMult, scaleMult, (float)trans.X * scaleMult, (float)trans.Y * scaleMult);
        var finalMatrix = SKMatrix.Concat(scaleTrans, transformMatrix);

        chunk.Surface.SkiaSurface.Canvas.Save();
        chunk.Surface.SkiaSurface.Canvas.SetMatrix(finalMatrix);
        chunk.Surface.SkiaSurface.Canvas.DrawSurface(toPaint.SkiaSurface, 0, 0, customPaint);
        chunk.Surface.SkiaSurface.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks()
    {
        return OperationHelper.FindChunksTouchingQuadrilateral(corners, ChunkPool.FullChunkSize);
    }

    public void Dispose()
    {
        if (imageWasCopied)
            toPaint.Dispose();
        customPaint?.Dispose();
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        if (verAxisX is not null && horAxisY is not null)
            return new ImageOperation
                (corners.AsMirroredAcrossVerAxis((int)verAxisX).AsMirroredAcrossHorAxis((int)horAxisY), toPaint, customPaint, imageWasCopied);
        if (verAxisX is not null)
            return new ImageOperation
                (corners.AsMirroredAcrossVerAxis((int)verAxisX), toPaint, customPaint, imageWasCopied);
        if (horAxisY is not null)
            return new ImageOperation
                (corners.AsMirroredAcrossHorAxis((int)horAxisY), toPaint, customPaint, imageWasCopied);
        return new ImageOperation(corners, toPaint, customPaint, imageWasCopied);
    }
}
