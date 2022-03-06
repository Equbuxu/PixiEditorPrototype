﻿using ChangeableDocument;
using ChangeableDocument.Actions;
using ChangeableDocument.ChangeInfos;
using PixiEditorPrototype.ViewModels;
using StructureRenderer;
using StructureRenderer.RenderInfos;
using System.Collections.Generic;

namespace PixiEditorPrototype.Models
{
    internal class ActionAccumulator
    {
        private bool executing = false;

        private List<IAction> queuedActions = new();
        private DocumentChangeTracker tracker;
        private DocumentViewModel document;
        private DocumentUpdater documentUpdater;
        private Renderer renderer;

        public ActionAccumulator(DocumentChangeTracker tracker, DocumentUpdater updater, DocumentViewModel document)
        {
            this.tracker = tracker;
            this.documentUpdater = updater;
            this.document = document;

            renderer = new(tracker);
        }

        public void AddAction(IAction action)
        {
            queuedActions.Add(action);
            TryExecuteAccumulatedActions();
        }

        public async void TryExecuteAccumulatedActions()
        {
            if (executing || queuedActions.Count == 0)
                return;
            executing = true;
            while (queuedActions.Count > 0)
            {
                var toExecute = queuedActions;
                queuedActions = new List<IAction>();

                var result = await tracker.ProcessActions(toExecute);

                foreach (IChangeInfo? info in result)
                {
                    documentUpdater.ApplyChangeFromChangeInfo(info);
                }

                document.FinalBitmap.Lock();
                var renderResult = await renderer.ProcessChanges(result!, document.FinalBitmapSurface, document.FinalBitmap.PixelWidth, document.FinalBitmap.PixelHeight);

                foreach (IRenderInfo info in renderResult)
                {
                    if (info is DirtyRect_RenderInfo dirtyRect)
                    {
                        document.FinalBitmap.AddDirtyRect(new(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height));
                    }
                }
                document.FinalBitmap.Unlock();
            }

            executing = false;
        }
    }
}
