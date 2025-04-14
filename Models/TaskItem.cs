using System;

namespace ToDoApp.Models
{
    [Serializable]
    public class TaskItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public TaskItem(string title)
        {
            Id = Guid.NewGuid();
            Title = title;
            IsCompleted = false;
            CreatedAt = DateTime.Now;
            CompletedAt = null;
        }

        public void Complete()
        {
            IsCompleted = true;
            CompletedAt = DateTime.Now;
        }

        public void Reset()
        {
            IsCompleted = false;
            CompletedAt = null;
        }

        public override string ToString()
        {
            return $"{Title} - {(IsCompleted ? "Completed" : "Pending")} - Created at: {CreatedAt} {(IsCompleted ? $"- Completed at: {CompletedAt}" : "")}";
        }
    }
}
