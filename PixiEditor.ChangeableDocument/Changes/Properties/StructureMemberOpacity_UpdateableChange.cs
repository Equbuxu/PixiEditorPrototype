﻿using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;

internal class StructureMemberOpacity_UpdateableChange : UpdateableChange
{
    private Guid memberGuid;

    private float originalOpacity;
    private float newOpacity;

    [GenerateUpdateableChangeActions]
    public StructureMemberOpacity_UpdateableChange(Guid memberGuid, float opacity)
    {
        this.memberGuid = memberGuid;
        newOpacity = opacity;
    }

    [UpdateChangeMethod]
    public void Update(float opacity)
    {
        newOpacity = opacity;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document document)
    {
        if (!document.TryFindMember(memberGuid, out var member))
            return new Error();
        originalOpacity = member.Opacity;
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target) => Apply(target, false, out _);

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document document, bool firstApply, out bool ignoreInUndo)
    {
        if (originalOpacity == newOpacity)
        {
            ignoreInUndo = true;
            return new None();
        }

        var member = document.FindMemberOrThrow(memberGuid);
        member.Opacity = newOpacity;

        ignoreInUndo = false;
        return new StructureMemberOpacity_ChangeInfo(memberGuid, newOpacity);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document document)
    {
        if (originalOpacity == newOpacity)
            return new None();

        var member = document.FindMemberOrThrow(memberGuid);
        member.Opacity = originalOpacity;

        return new StructureMemberOpacity_ChangeInfo(memberGuid, originalOpacity);
    }

    public override bool IsMergeableWith(Change other)
    {
        if (other is not StructureMemberOpacity_UpdateableChange same)
            return false;
        return same.memberGuid == memberGuid;
    }
}
