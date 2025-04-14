using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ToDoApp.Models;

namespace ToDoApp.DataAccess
{
    public class FileTaskRepository: ITaskRepository
    {
        private List<TaskItem> _tasks;
        private readonly string _filePath;
        private readonly object _lock = new object();

        public FileTaskRepository(string filePath)
        {
            _filePath = filePath;
            _tasks = new List<TaskItem>();
            LoadTasks();
        }

        public List<TaskItem> GetAllTasks()
        {
            lock (_lock)
            {
                return _tasks.ToList();
            }
        }

        public TaskItem GetTaskById(Guid id)
        {
            lock (_lock)
            {
                return _tasks.FirstOrDefault(t => t.Id == id);
            }
        }

        public void AddTask(TaskItem task)
        {
            if(task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            lock (_lock)
            {
                _tasks.Add(task);
            }
        }

        public void UpdateTask(TaskItem task) { 
            if(task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            lock(_lock)
            {
                var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    int index = _tasks.IndexOf(existingTask);
                    _tasks[index] = task;
                }
            }
        }

        public void DeleteTask(Guid id)
        {
            lock (_lock)
            {
                var task = _tasks.FirstOrDefault(t => t.Id == id);
                if (task != null)
                {
                    _tasks.Remove(task);
                }
            }
        }

        public void SaveChanges()
        {
            lock (_lock)
            {
                try
                {
                    string directory = Path.GetDirectoryName(_filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    using (var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                    {
                        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        formatter.Serialize(fileStream, _tasks);
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error saving tasks to file", ex);
                }
            }
        }

        public void LoadTasks()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_filePath))
                    {
                        using (var fileStream = new FileStream(_filePath, FileMode.Open))
                        {
                            var formatter = new BinaryFormatter();
                            _tasks = (List<TaskItem>)formatter.Deserialize(fileStream);
                        }
                    } else
                    {
                        _tasks = new List<TaskItem>();
                    }
                }
                catch
                {
                    _tasks = new List<TaskItem>();
                }
            }
        }
    }
}
