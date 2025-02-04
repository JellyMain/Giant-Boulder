using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [InitializeOnLoad]
    public class GeNaUndoRedoEditor
    {
        static GeNaUndoRedoEditor()
        {
            UndoProManager.EnableUndoPro();
            GeNaUndoRedo.onRecordUndo -= OnRecordUndo;
            GeNaUndoRedo.onRecordUndo += OnRecordUndo;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }
        private static Queue<UndoEntity> undoQueue = new Queue<UndoEntity>();
        private static Queue<Action> postActionQueue = new Queue<Action>();
        private static void Update()
        {
            while (undoQueue.Count > 0)
            {
                UndoEntity entity = undoQueue.Dequeue();
                RecordUndo(entity);
            }
            while (postActionQueue.Count > 0)
            {
                Action action = postActionQueue.Dequeue();
                action?.Invoke();
            }
        }
        private static void ProcessEntity(UndoEntity undoEntity)
        {
            switch (undoEntity)
            {
                case SplineEntity splineEntity:
                    if (splineEntity.Spline != null)
                        Undo.RegisterCompleteObjectUndo(splineEntity.Spline, splineEntity.name);
                    return;
                case ResourceEntity resourceEntity:
                    if (resourceEntity.GameObject != null)
                        Undo.RegisterCreatedObjectUndo(resourceEntity.GameObject, resourceEntity.name);
                    return;
                case GameObjectEntity gameObjectEntity:
                    GameObject gameObject = gameObjectEntity.m_gameObject;
                    if (gameObject != null)
                    {
                        if (gameObjectEntity.m_destroy)
                        {
                            bool isPartOfAnyPrefab = PrefabUtility.IsPartOfAnyPrefab(gameObject);
                            if (isPartOfAnyPrefab)
                                gameObject = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
                            Undo.DestroyObjectImmediate(gameObject);
                        }
                        else
                        {
                            Undo.RegisterCreatedObjectUndo(gameObject, gameObject.name);
                        }
                    }
                    return;
                default:
                    RecordUndoPro(undoEntity, true);
                    return;
            }
        }
        private static void OnRecordUndo(UndoEntity undoEntity, Action postAction)
        {
            // Do not record undo stack in play mode
            if (Application.isPlaying)
                return;
            undoQueue.Enqueue(undoEntity);
            postActionQueue.Enqueue(postAction);
        }
        public static void RecordUndoPro(UndoEntity undoEntity, bool updateRecords)
        {
            UndoProManager.RecordOperation(undoEntity.Perform, 
                undoEntity.Undo, 
                undoEntity.Dispose,
                undoEntity.name,
                undoEntity.mergeBefore,
                undoEntity.mergeAfter, 
                updateRecords);
        }

        public static void RecordUndo(UndoEntity undoEntity)
        {
            switch (undoEntity)
            {
                case UndoRecord undoRecord:
                    int group = undoRecord.Group;
                    Stack<UndoEntity> entities = undoRecord.Entities;
                    foreach (UndoEntity entity in entities)
                        ProcessEntity(entity);
                    RecordUndoPro(undoEntity, true);
                    Undo.CollapseUndoOperations(group);
                    break;
                default:
                    ProcessEntity(undoEntity);
                    break;
            }
        }
    }
}