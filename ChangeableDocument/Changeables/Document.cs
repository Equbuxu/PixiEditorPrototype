﻿using ChangeableDocument.Changeables.Interfaces;
using ChunkyImageLib.DataHolders;

namespace ChangeableDocument.Changeables
{
    internal class Document : IChangeable, IReadOnlyDocument
    {
        public IReadOnlyFolder ReadOnlyStructureRoot => StructureRoot;
        public IReadOnlySelection ReadOnlySelection => Selection;
        IReadOnlyStructureMember? IReadOnlyDocument.FindMember(Guid guid) => FindMember(guid);
        IReadOnlyList<IReadOnlyStructureMember> IReadOnlyDocument.FindMemberPath(Guid guid) => FindMemberPath(guid);
        IReadOnlyStructureMember IReadOnlyDocument.FindMemberOrThrow(Guid guid) => FindMemberOrThrow(guid);
        (IReadOnlyStructureMember, IReadOnlyFolder) IReadOnlyDocument.FindChildAndParentOrThrow(Guid guid) => FindChildAndParentOrThrow(guid);

        public static Vector2i DefaultSize { get; } = new Vector2i(64, 64);
        internal Folder StructureRoot { get; } = new() { GuidValue = Guid.Empty };
        internal Selection Selection { get; } = new();
        public Vector2i Size { get; set; } = DefaultSize;

        public StructureMember FindMemberOrThrow(Guid guid) => FindMember(guid) ?? throw new Exception("Could not find member with guid " + guid.ToString());
        public StructureMember? FindMember(Guid guid)
        {
            var list = FindMemberPath(guid);
            return list.Count > 0 ? list[0] : null;
        }

        public (StructureMember, Folder) FindChildAndParentOrThrow(Guid childGuid)
        {
            var path = FindMemberPath(childGuid);
            if (path.Count < 2)
                throw new Exception("Couldn't find child and parent");
            return (path[0], (Folder)path[1]);
        }

        public List<StructureMember> FindMemberPath(Guid guid)
        {
            var list = new List<StructureMember>();
            if (FillMemberPath(StructureRoot, guid, list))
                list.Add(StructureRoot);
            return list;
        }

        private bool FillMemberPath(Folder folder, Guid guid, List<StructureMember> toFill)
        {
            if (folder.GuidValue == guid)
            {
                return true;
            }
            foreach (var member in folder.Children)
            {
                if (member is Layer childLayer && childLayer.GuidValue == guid)
                {
                    toFill.Add(member);
                    return true;
                }
                if (member is Folder childFolder)
                {
                    if (FillMemberPath(childFolder, guid, toFill))
                    {
                        toFill.Add(childFolder);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
