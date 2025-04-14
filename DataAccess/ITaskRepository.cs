using System;
using System.Collections.Generic;
using ToDoApp.Models;

namespace ToDoApp.DataAccess
{
    public interface ITaskRepository
    {
        List<TaskItem> GetAllTasks();
        TaskItem GetTaskById(Guid id);
        void AddTask(TaskItem task);
        void UpdateTask(TaskItem task);
        void DeleteTask(Guid id);
        void SaveChanges();

    }
}
