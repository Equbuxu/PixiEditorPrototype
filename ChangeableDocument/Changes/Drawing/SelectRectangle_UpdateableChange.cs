﻿using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChangeableDocument.Changes.Drawing
{
    internal class SelectRectangle_UpdateableChange : IUpdateableChange
    {
        private bool originalIsEmpty;
        private Vector2i pos;
        private Vector2i size;
        private CommitedChunkStorage? originalSelectionState;
        public SelectRectangle_UpdateableChange(Vector2i pos, Vector2i size)
        {
            Update(pos, size);
        }
        public void Initialize(Document target)
        {
            originalIsEmpty = target.Selection.IsEmptyAndInactive;
        }

        public void Update(Vector2i pos, Vector2i size)
        {
            this.pos = pos;
            this.size = size;
        }

        public IChangeInfo? ApplyTemporarily(Document target)
        {
            var oldChunks = target.Selection.SelectionImage.FindAffectedChunks();
            target.Selection.SelectionImage.CancelChanges();
            target.Selection.IsEmptyAndInactive = false;
            target.Selection.SelectionImage.DrawRectangle(new ShapeData(pos, size, 0, SKColors.Transparent, Selection.SelectionColor));

            oldChunks.UnionWith(target.Selection.SelectionImage.FindAffectedChunks());
            return new Selection_ChangeInfo() { Chunks = oldChunks };
        }

        public IChangeInfo? Apply(Document target)
        {
            var changes = ApplyTemporarily(target);
            originalSelectionState = new CommitedChunkStorage(target.Selection.SelectionImage, ((Selection_ChangeInfo)changes!).Chunks!);
            target.Selection.SelectionImage.CommitChanges();
            target.Selection.IsEmptyAndInactive = target.Selection.SelectionImage.CheckIfCommitedIsEmpty();
            return changes;
        }

        public IChangeInfo? Revert(Document target)
        {
            if (originalSelectionState == null)
                throw new Exception("No stored chunks to revert to");

            target.Selection.IsEmptyAndInactive = originalIsEmpty;
            originalSelectionState.ApplyChunksToImage(target.Selection.SelectionImage);
            originalSelectionState.Dispose();
            originalSelectionState = null;
            var changes = new Selection_ChangeInfo() { Chunks = target.Selection.SelectionImage.FindAffectedChunks() };
            target.Selection.SelectionImage.CommitChanges();
            return changes;
        }

        public void Dispose()
        {
            originalSelectionState?.Dispose();
        }
    }
}
