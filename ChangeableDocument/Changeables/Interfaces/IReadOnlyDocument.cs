﻿namespace ChangeableDocument.Changeables.Interfaces
{
    public interface IReadOnlyDocument
    {
        IReadOnlyFolder ReadOnlyStructureRoot { get; }
        IReadOnlySelection ReadOnlySelection { get; }
        IReadOnlyStructureMember? FindMember(Guid guid);
        IReadOnlyStructureMember FindMemberOrThrow(Guid guid);
        (IReadOnlyStructureMember, IReadOnlyFolder) FindChildAndParentOrThrow(Guid guid);
        IReadOnlyList<IReadOnlyStructureMember> FindMemberPath(Guid guid);
    }
}
