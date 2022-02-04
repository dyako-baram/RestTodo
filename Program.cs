using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/todos",async (TodoDb todoDb)=> await todoDb.Todos.AsNoTracking().ToListAsync());
app.MapGet("/todos/{id:int}", async (int id,TodoDb todoDb)=>
{
    var result=await todoDb.Todos.FirstOrDefaultAsync(x=>x.Id==id);
    if (result is null)
        return Results.NotFound();
    return Results.Ok(result);
});
app.MapGet("/todos/{title}", async (string title,TodoDb todoDb)=>
{
    var result=await todoDb.Todos.AsNoTracking().Where(x=>x.Title.Contains(title)).ToListAsync();
    if (result is null)
        return Results.NotFound();
    return Results.Ok(result);
});

app.MapPost("/todos/", async (Todo todo,TodoDb todoDb)=> {
    todoDb.Todos.Add(todo);
    await todoDb.SaveChangesAsync();

    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo todo, TodoDb todoDb) =>
{
    var findTodo = await todoDb.Todos.FindAsync(id);

    if (findTodo is null) return Results.NotFound();

    findTodo.Title = todo.Title;
    findTodo.IsComplete = todo.IsComplete;

    await todoDb.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb todoDb) =>
{
    if (await todoDb.Todos.FindAsync(id) is Todo todo)
    {
        todoDb.Todos.Remove(todo);
        await todoDb.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

app.Run();

class Todo
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public bool IsComplete { get; set; }
}

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}