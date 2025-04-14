using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ToDoApp.DataAccess;
using ToDoApp.Services;
using ToDoApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ToDoApp.UI
{
    public partial class MainForm : Form
    {
        private readonly TaskService _taskService;
        public MainForm()
        {
            InitializeComponent();

            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ToDoApp",
                "tasks.dat"
                );
            var repository = new FileTaskRepository(appDataPath);
            _taskService = new TaskService(repository);
            // Setup UI Event handlers
            Load += MainForm_Load;

        }

        private void InitializeComponent()
        {
            this.Text = "To-Do App";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(700, 500);
            this.Icon = SystemIcons.Application;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                RowCount = 3,
                ColumnCount = 1,
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            this.Controls.Add(mainPanel);

            var addTaskPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                Margin = new Padding(0, 0, 0, 5)
            };
            addTaskPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            addTaskPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            addTaskPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            mainPanel.Controls.Add(addTaskPanel, 0, 0);

            var txtNewTask = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 0, 5, 0),
            };
            txtNewTask.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    AddTask();
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            };
            addTaskPanel.Controls.Add(txtNewTask, 0, 0);

            var btnAdd = new Button
            {
                Text = "Add Task",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 0, 5, 0),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => AddTask();
            addTaskPanel.Controls.Add(btnAdd, 1, 0);

            var cboFilter = new ComboBox { 
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Items = { "All", "Active", "Completed" },
                SelectedIndex = 0
            };
            cboFilter.SelectedIndexChanged += (s, e) => FilterTasks();
            addTaskPanel.Controls.Add(cboFilter, 2, 0);

            var lstTasks = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                CheckBoxes = true,
                GridLines = false,
                Font = new Font("Segoe UI", 10F),
            };
            lstTasks.Columns.Add("Task", -2);
            lstTasks.Columns.Add("Status", 100);
            lstTasks.Columns.Add("Created", 150);
            lstTasks.Columns.Add("Due Date", 150);

            lstTasks.ItemChecked += (s, e) =>
            {
                if (e.Item.Tag is TaskItem task)
                {
                    if (e.Item.Checked && !task.IsCompleted)
                    {
                        _taskService.CompleteTask(task.Id);
                        e.Item.ForeColor = Color.Gray;
                        e.Item.Font = new Font(lstTasks.Font, FontStyle.Strikeout);
                    }
                    else if (!e.Item.Checked && task.IsCompleted)
                    {
                        _taskService.ResetTask(task.Id);
                        e.Item.ForeColor = SystemColors.WindowText;
                        e.Item.Font = new Font(lstTasks.Font, FontStyle.Regular);
                    }
                }
            };
            mainPanel.Controls.Add(lstTasks, 0, 1);

            var taskContextMenu = new ContextMenuStrip();

            var editMenuItem = new ToolStripMenuItem("Edit Task");
            editMenuItem.Click += (s, e) =>
            {
                if (lstTasks.SelectedItems.Count > 0 && lstTasks.SelectedItems[0].Tag is TaskItem task)
                {
                    EditTask(task, lstTasks.SelectedItems[0]);
                }
            };
            taskContextMenu.Items.Add(editMenuItem);

            var deleteMenuItem = new ToolStripMenuItem("Delete Task");
            deleteMenuItem.Click += (s, e) =>
            {
                if (lstTasks.SelectedItems.Count > 0 && lstTasks.SelectedItems[0].Tag is TaskItem task)
                {
                    DeleteTask(task, lstTasks.SelectedItems[0]);
                }
            };
            taskContextMenu.Items.Add(deleteMenuItem);

            lstTasks.ContextMenuStrip = taskContextMenu;

            var statusPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                Margin = new Padding(0, 5, 0, 0)
            };
            statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            mainPanel.Controls.Add(statusPanel, 0, 2);

            // Active count label
            var lblActiveCount = new Label
            {
                Text = "Active: 0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F)
            };
            statusPanel.Controls.Add(lblActiveCount, 0, 0);

            // Completed count label
            var lblCompletedCount = new Label
            {
                Text = "Completed: 0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F)
            };
            statusPanel.Controls.Add(lblCompletedCount, 1, 0);

            // Clear completed button
            var btnClearCompleted = new Button
            {
                Text = "Clear Completed",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(232, 17, 35),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnClearCompleted.FlatAppearance.BorderSize = 0;
            btnClearCompleted.Click += (s, e) => ClearCompletedTasks();
            statusPanel.Controls.Add(btnClearCompleted, 2, 0);

            // Store references to controls we'll need to access later
            this.txtNewTask = txtNewTask;
            this.lstTasks = lstTasks;
            this.cboFilter = cboFilter;
            this.lblActiveCount = lblActiveCount;
            this.lblCompletedCount = lblCompletedCount;
        }

        private TextBox txtNewTask;
        private ListView lstTasks;
        private ComboBox cboFilter;
        private Label lblActiveCount;
        private Label lblCompletedCount;

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshTaskList();
        }

        private void RefreshTaskList()
        {
            try
            {
                lstTasks.Items.Clear();
                List<TaskItem> tasks;

                // Get tasks based on filter
                switch (cboFilter.SelectedIndex)
                {
                    case 1: // Active
                        tasks = _taskService.GetActiveTask();
                        break;
                    case 2: // Completed
                        tasks = _taskService.GetCompletedTask();
                        break;
                    default: // All
                        tasks = _taskService.GetAllTasks();
                        break;
                }

                // Add tasks to list view
                foreach (var task in tasks)
                {
                    var item = new ListViewItem(task.Title);
                    item.SubItems.Add(task.CreatedAt.ToString("g"));
                    item.Tag = task;
                    item.Checked = task.IsCompleted;
                    item.SubItems.Add(task.IsCompleted ? task.CompletedAt?.ToString("g") : "Not completed");

                    if (task.IsCompleted)
                    {
                        item.ForeColor = Color.Gray;
                        item.Font = new Font(lstTasks.Font, FontStyle.Strikeout);
                    }

                    lstTasks.Items.Add(item);
                }

                // Auto-size columns
                lstTasks.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                // Update status
                var allTasks = _taskService.GetAllTasks();
                var activeTasks = allTasks.Count(t => !t.IsCompleted);
                var completedTasks = allTasks.Count(t => t.IsCompleted);

                lblActiveCount.Text = $"Active: {activeTasks}";
                lblCompletedCount.Text = $"Completed: {completedTasks}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error refreshing task list: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void AddTask()
        {
            try
            {
                string title = txtNewTask.Text.Trim();
                if (!string.IsNullOrEmpty(title))
                {
                    _taskService.AddTask(title);
                    txtNewTask.Clear();
                    RefreshTaskList();
                    txtNewTask.Focus();
                }
                else
                {
                    MessageBox.Show("Task title cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                MessageBox.Show("An error occurred while adding the task.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterTasks()
        {
            RefreshTaskList();
        }

        private void EditTask(TaskItem task, ListViewItem listViewItem)
        {
            try {
                string newTitle = Microsoft.VisualBasic.Interaction.InputBox(
                    "Edit task:",
                    "Edit Task",
                    task.Title);
                if (!string.IsNullOrEmpty(newTitle))
                {
                    task.Title = newTitle;
                    _taskService.UpdateTask(task);
                    listViewItem.Text = newTitle;
                    RefreshTaskList();
                }
            } catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error editing task: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void DeleteTask(TaskItem task, ListViewItem item)
        {
            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this task?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _taskService.DeleteTask(task.Id);
                    lstTasks.Items.Remove(item);
                    RefreshTaskList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error deleting task: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ClearCompletedTasks()
        {
            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to clear all completed tasks?",
                    "Confirm Clear",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var completedTasks = _taskService.GetCompletedTask();
                    foreach (var task in completedTasks)
                    {
                        _taskService.DeleteTask(task.Id);
                    }
                    RefreshTaskList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error clearing completed tasks: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    
    }
}
