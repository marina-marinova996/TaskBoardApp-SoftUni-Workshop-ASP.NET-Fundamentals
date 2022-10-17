using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using System.Security.Claims;
using TaskBoardApp.Data;
using TaskBoardApp.Models.Task;
using Task = TaskBoardApp.Data.Entities.Task;

namespace TaskBoardApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly TaskBoardAppDbContext data;

        public TasksController (TaskBoardAppDbContext context)
        {
            this.data = context;
        }

        [HttpGet]
        public IActionResult Create()

        {
            TaskFormModel taskModel = new TaskFormModel()
            {
                Boards  = GetBoards()
            };

            return View(taskModel);
        }

        [HttpPost]
        public IActionResult Create(TaskFormModel taskFormModel)

        {
            if (!GetBoards().Any(b => b.Id == taskFormModel.BoardId))
            {
                this.ModelState.AddModelError(nameof(taskFormModel.BoardId), "Board does not exist.");
            }

            string currentUserId = GetUserId();

            Task task = new Task()
            {
                Title = taskFormModel.Title,
                Description = taskFormModel.Description,
                CreatedOn = DateTime.Now,
                BoardId = taskFormModel.BoardId,
                OwnerId = currentUserId,
            };

            this.data.Tasks.Add(task);
            this.data.SaveChanges();

            var boards = this.data.Boards;

            return RedirectToAction("All", "Boards");
        }

        public IActionResult Details(int id)
        {
            var task = this.data.Tasks
                            .Where(t => t.Id == id)
                            .Select(t => new TaskDetailsViewModel()
                            {
                                Id = t.Id,
                                Title = t.Title,
                                Description = t.Description,
                                CreatedOn = t.CreatedOn.ToString("dd/MM/yyyy HH:mm")
                            })
                            .FirstOrDefault();

            if (task == null)
            {
                return BadRequest();
            }

            return View(task);
        }

        [HttpGet]
        public IActionResult Edit (int id)
        {
            Task task = this.data.Tasks.Find(id);

            if (task == null)
            {
                return BadRequest();
            }

            string currentUserId = GetUserId();

            if (currentUserId != task.OwnerId)
            {
                return Unauthorized();
            }

            TaskFormModel model = new TaskFormModel()
            {
                Title = task.Title,
                Description = task.Description,
                BoardId = task.BoardId,
                Boards = GetBoards()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(int id, TaskFormModel model)
        {
            Task task = this.data.Tasks.Find(id);

            if (task == null)
            {
                return BadRequest();
            }

            string currentUserId = GetUserId();

            if (currentUserId != task.OwnerId)
            {
                return Unauthorized();
            }

            if(!GetBoards().Any(b=> b.Id == model.BoardId))
            {
                this.ModelState.AddModelError(nameof(model.BoardId), "Board does not exist.");
            }

            task.Title = model.Title;
            task.Description = model.Description;
            task.BoardId = model.BoardId;

            this.data.SaveChanges();
            return RedirectToAction("All", "Boards");

        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            Task task = this.data.Tasks.Find(id);

            if (task == null)
            {
                return BadRequest();
            }

            string currentUserId = GetUserId();

            if (currentUserId != task.OwnerId)
            {
                return Unauthorized();
            }

            TaskViewModel taskModel = new TaskViewModel()
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
            };

            return View(taskModel);
        }


            [HttpPost]
        public IActionResult Delete(TaskViewModel taskModel)
        {
            Task task = this.data.Tasks.Find(taskModel.Id);

            if (task == null)
            {
                return BadRequest();
            }

            string currentUserId = GetUserId();

            if (currentUserId != task.OwnerId)
            {
                return Unauthorized();
            }

            this.data.Tasks.Remove(task);
            this.data.SaveChanges();
            return RedirectToAction("All", "Boards");
        }
        private string GetUserId()
        {
           return this.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private IEnumerable<TaskBoardModel> GetBoards()
        {
            return this.data
                            .Boards
                            .Select(b => new TaskBoardModel()
                            {
                                 Id = b.Id,
                                 Name = b.Name
                            });
        }
    }
}
