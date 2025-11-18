using System;
using System.Collections.Generic;

namespace EPSConverter.Models
{
    public class UndoRedoManager
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();
        private const int MaxUndoSteps = 50;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public event EventHandler? StateChanged;

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();

            // Limit undo stack size
            while (_undoStack.Count > MaxUndoSteps)
            {
                var items = new ICommand[_undoStack.Count];
                _undoStack.CopyTo(items, 0);
                _undoStack.Clear();
                for (int i = 1; i < items.Length; i++)
                {
                    _undoStack.Push(items[i]);
                }
            }

            OnStateChanged();
        }

        public void Undo()
        {
            if (!CanUndo)
                return;

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            OnStateChanged();
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            OnStateChanged();
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnStateChanged();
        }

        private void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class TransformCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Action _undo;

        public TransformCommand(Action execute, Action undo)
        {
            _execute = execute;
            _undo = undo;
        }

        public void Execute() => _execute();
        public void Undo() => _undo();
    }
}
