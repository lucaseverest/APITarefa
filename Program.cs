using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("TasksDB"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

app.MapGet("/tasks", async (AppDbContext db) => await db.Tasks.ToListAsync());

app.MapGet("/tasks/{id}", async (int id, AppDbContext db) => await db.Tasks.FindAsync(id) is Task task ? Results.Ok(task) : Results.NotFound());

app.MapGet("/tasks/done", async (AppDbContext db) => await db.Tasks.Where(t => t.IsDone).ToListAsync());

app.MapPut("/tasks/{id}", async (int id, Task inputTask, AppDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();
    task.Name = inputTask.Name;
    task.IsDone = inputTask.IsDone;

    await db.SaveChangesAsync();
    return Results.Ok();

});

app.MapDelete("/tasks/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Tasks.FindAsync(id) is Task task)
    {
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    return Results.NotFound();
});


app.MapPost("/tasks", async (Task task, AppDbContext db) =>
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});


app.Run();


class Task
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsDone { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }
    public DbSet<Task> Tasks => Set<Task>();
}