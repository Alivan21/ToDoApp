using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
