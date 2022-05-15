﻿using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Root;

namespace PixiEditor.ChangeableDocument.Actions.Root.SymmetryPosition;
public class EndSetSymmetryPosition_Action : IEndChangeAction
{
    bool IEndChangeAction.IsChangeTypeMatching(Change change)
    {
        return change is SymmetryPosition_UpdateableChange;
    }
}
