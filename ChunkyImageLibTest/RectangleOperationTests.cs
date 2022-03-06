﻿using ChunkyImageLib;
using ChunkyImageLib.Operations;
using SkiaSharp;
using System.Collections.Generic;
using Xunit;

namespace ChunkyImageLibTest
{
    public class RectangleOperationTests
    {
        const int chunkSize = ChunkPool.ChunkSize;
// to keep expected rectangles aligned
#pragma warning disable format
        [Fact]
        public void FindAffectedChunks_SmallStrokeOnly_FindsCorrectChunks()
        {
            var (x, y, w, h) = (0, 0, chunkSize, chunkSize);
            RectangleOperation operation = new(new(x, y, w, h, 1, SKColors.Black, SKColors.Transparent));

            HashSet<(int, int)> expected = new() { (0, 0) };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_2by2StrokeOnly_FindsCorrectChunks()
        {
            var (x, y, w, h) = (-chunkSize, -chunkSize, chunkSize * 2, chunkSize * 2);
            RectangleOperation operation = new(new(x, y, w, h, 1, SKColors.Black, SKColors.Transparent));

            HashSet<(int, int)> expected = new() { (-1, -1), (0, -1), (-1, 0), (0, 0) };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_3x3PositiveStrokeOnly_FindsCorrectChunks()
        {
            var (x, y, w, h) = (chunkSize + chunkSize / 2, chunkSize + chunkSize / 2, chunkSize * 2, chunkSize * 2);
            RectangleOperation operation = new(new(x, y, w, h, 1, SKColors.Black, SKColors.Transparent));

            HashSet<(int, int)> expected = new()
            {
                (1, 1), (2, 1), (3, 1), 
                (1, 2),         (3, 2), 
                (1, 3), (2, 3), (3, 3),
            };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_3x3NegativeStrokeOnly_FindsCorrectChunks()
        {
            var (x, y, w, h) = (-chunkSize * 3 + chunkSize / 2, -chunkSize * 3 + chunkSize / 2, chunkSize * 2, chunkSize * 2);
            RectangleOperation operation = new(new(x, y, w, h, 1, SKColors.Black, SKColors.Transparent));

            HashSet<(int, int)> expected = new()
            {
                (-4, -4), (-3, -4), (-2, -4),
                (-4, -3),           (-2, -3),
                (-4, -2), (-3, -2), (-2, -2),
            };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_3x3PositiveFilled_FindsCorrectChunks()
        {
            var (x, y, w, h) = (chunkSize + chunkSize / 2, chunkSize + chunkSize / 2, chunkSize * 2, chunkSize * 2);
            RectangleOperation operation = new(new(x, y, w, h, 1, SKColors.Black, SKColors.White));

            HashSet<(int, int)> expected = new()
            {
                (1, 1), (2, 1), (3, 1), 
                (1, 2), (2, 2), (3, 2),
                (1, 3), (2, 3), (3, 3),
            };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_ThickPositiveStroke_FindsCorrectChunks()
        {
            var (x, y, w, h) = (chunkSize / 2, chunkSize / 2, chunkSize * 4, chunkSize * 4);
            RectangleOperation operation = new(new(x, y, w, h, 32, SKColors.Black, SKColors.Transparent));

            HashSet<(int, int)> expected = new()
            {
                (0, 0), (1, 0), (2, 0), (3, 0), (4, 0),
                (0, 1), (1, 1), (2, 1), (3, 1), (4, 1),
                (0, 2), (1, 2),         (3, 2), (4, 2),
                (0, 3), (1, 3), (2, 3), (3, 3), (4, 3),
                (0, 4), (1, 4), (2, 4), (3, 4), (4, 4),
            };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_SmallButThick_FindsCorrectChunks()
        {
            var (x, y, w, h) = (chunkSize / 2, chunkSize / 2, 1, 1);
            RectangleOperation operation = new(new(x, y, w, h, 256, SKColors.Black, SKColors.White));

            HashSet<(int, int)> expected = new() { (0, 0) };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }
#pragma warning restore format
    }
}
