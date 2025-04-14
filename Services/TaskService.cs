using System;
using System.Collections.Generic;
using System.Linq;
using ToDoApp.DataAccess;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class TaskService
    {
        private readonly ITaskRepository _repository;

        public TaskService(ITaskRepository taskRepository)
        {
            _repository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        }

        public List<TaskItem> GetAllTasks()
        {
            try
            {
                return _repository.GetAllTasks();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving tasks.", ex);
            }
        }

        public List<TaskItem> GetActiveTask()
        {
            try
            {
                return _repository.GetAllTasks().Where(t => !t.IsCompleted).ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving active tasks.", ex);
            }
        }

        public List<TaskItem> GetCompletedTask()
        {
            try
            {
                return _repository.GetAllTasks().Where(t => t.IsCompleted).ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving completed tasks.", ex);
            }
        }

        public TaskItem AddTask(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Task title cannot be null or empty.", nameof(title));
            }
            try
            {
                var task = new TaskItem(title);
                _repository.AddTask(task);
                _repository.SaveChanges();
                return task;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while adding a task.", ex);
            }
        }

        public void UpdateTask(TaskItem task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task), "Task cannot be null.");
            }
            try
            {
                _repository.UpdateTask(task);
                _repository.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while updating the task.", ex);
            }
        }

        public void CompleteTask(Guid id)
        {
            try
            {
                var task = _repository.GetTaskById(id);
                if (task != null)
                { 
                    task.Complete();
                    _repository.UpdateTask(task);
                    _repository.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while completing the task.", ex);
            }
        }

        public void ResetTask(Guid id)
        {
            try
            {
                var task = _repository.GetTaskById(id);
                if (task != null)
                {
                    task.Reset();
                    _repository.UpdateTask(task);
                    _repository.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while resetting the task.", ex);
            }
        }

        public void DeleteTask(Guid id)
        {
            try
            {
                _repository.DeleteTask(id);
                _repository.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while deleting the task.", ex);
            }
        }
    }
}
