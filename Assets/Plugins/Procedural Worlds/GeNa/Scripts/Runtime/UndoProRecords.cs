using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
namespace GeNa.Core
{
    /// <summary>
    /// Class storing the complete custom undo record for a scene, including the custom operations
    /// </summary>
    [ExecuteInEditMode]
    public class UndoProRecords : MonoBehaviour
    {
        private Scene scene;

        // relativeIndex: -inf -> 0 = Undo; 1 -> +inf = Redo
        public List<UndoProRecord> undoProRecords = new List<UndoProRecord>();
        public UndoState undoState;
        public List<UndoProRecord> proUndoStack
        {
            get { return undoProRecords.Where((UndoProRecord record) => record.relativeStackPos <= 0).OrderByDescending((UndoProRecord record) => record.relativeStackPos).ToList(); }
        }
        public List<UndoProRecord> proRedoStack
        {
            get { return undoProRecords.Where((UndoProRecord record) => record.relativeStackPos > 0).OrderByDescending((UndoProRecord record) => record.relativeStackPos).ToList(); }
        }
        private void OnEnable()
        {
            scene = SceneManager.GetActiveScene();
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
        private void Update()
        {
            Scene curScene = SceneManager.GetActiveScene();
            if (curScene != scene)
                GeNaEvents.Destroy(gameObject);
        }
        /// <summary>
        /// Clears the UndoProRecords in the redo-stack
        /// </summary>
        public void ClearRedo()
        {
            for (int recCnt = 0; recCnt < undoProRecords.Count; recCnt++)
            {
                if (undoProRecords[recCnt].relativeStackPos > 0)
                {
                    undoProRecords[recCnt].dispose?.Invoke();
                    undoProRecords.RemoveAt(recCnt);
                    recCnt--;
                }
            }
        }
        /// <summary>
        /// Updates the internal record-stack accounting for the given amount of default undo entries
        /// </summary>
        public void UndoRecordsAdded(int addedRecordsCount)
        {
            if (undoState.redoRecords.Count == 0)
                ClearRedo();
            //Debug.Log ("Shifted records by " + addedRecordsCount + " because of new added undo records!");
            for (int recCnt = 0; recCnt < undoProRecords.Count; recCnt++)
            {
                UndoProRecord record = undoProRecords[recCnt];
                if (record.relativeStackPos <= 0)
                    record.relativeStackPos -= addedRecordsCount;
            }
        }
        /// <summary>
        /// Updates onternal records to represent the undo/redo operation represented by a opShift (negative: Undo; positive: Redo);
        /// Returns the records which are affected by this undo/redo operation (switched from Undo-stack to redo or vice versa)
        /// </summary>
        public List<UndoProRecord> PerformOperationInternal(int opShift, int anomalyAddedCount)
        {
            if (opShift == 0)
                return new List<UndoProRecord>();

            // Anomaly
            bool opDir = opShift <= 0;
            //if (anomalyAddedCount < 0)
            //	opDir = !opDir;
            int anomalyShift = (opDir ? anomalyAddedCount : -anomalyAddedCount);
            List<UndoProRecord> operatedRecords = new List<UndoProRecord>();
            for (int recCnt = 0; recCnt < undoProRecords.Count; recCnt++)
            {
                UndoProRecord record = undoProRecords[recCnt];
                int prevInd = record.relativeStackPos;
                record.relativeStackPos -= opShift;
                if (anomalyAddedCount != 0 && isUndo(prevInd) != opDir) // Anomaly
                    record.relativeStackPos += anomalyShift;
                if (isUndo(prevInd) != isUndo(record.relativeStackPos)) // affected by this undo/redo operation
                    operatedRecords.Add(record);
            }
            return operatedRecords;
        }
        private bool isUndo(int index)
        {
            return index <= 0;
        }
    }
    /// <summary>
    /// Represents a custom operation record in the custom recording
    /// </summary>
    [Serializable]
    public class UndoProRecord
    {
        public string label;
        public int relativeStackPos;
        public SerializableAction perform;
        public SerializableAction undo;
        public SerializableAction dispose;
        public UndoProRecord(Action PerformAction, Action UndoAction, Action DisposeAction, string Label, int stackPosition)
        {
            label = Label;
            relativeStackPos = stackPosition;
            perform = new SerializableAction(PerformAction);
            undo = new SerializableAction(UndoAction);
            dispose = new SerializableAction(DisposeAction);
        }
    }
    /// <summary>
    /// Stores the internal undo state during a frame
    /// </summary>
    [Serializable]
    public class UndoState
    {
        public List<string> redoRecords = new List<string>();
        public List<string> undoRecords = new List<string>();
    }
}