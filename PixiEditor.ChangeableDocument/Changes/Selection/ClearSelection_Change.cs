﻿using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Selection;

internal class ClearSelection_Change : Change
{
    private SKPath? originalPath;

    [GenerateMakeChangeAction]
    public ClearSelection_Change() { }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (target.Selection.SelectionPath.IsEmpty)
            return new Error();
        originalPath = new SKPath(target.Selection.SelectionPath);
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        (var toDispose, target.Selection.SelectionPath) = (target.Selection.SelectionPath, new SKPath());
        toDispose.Dispose();

        ignoreInUndo = false;
        return new Selection_ChangeInfo(new SKPath());
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        (var toDispose, target.Selection.SelectionPath) = (target.Selection.SelectionPath, new SKPath(originalPath));
        toDispose.Dispose();

        return new Selection_ChangeInfo(new SKPath(originalPath));
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
    }
}
