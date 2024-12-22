using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(option => option.UseInMemoryDatabase("TodoList"));
// builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Swagger Doc
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config => 
{
    config.DocumentName = "Todo API";
    config.Title = "Todo API - v1";
    config.Version = "v1";
});

var app = builder.Build();
// Config Swagger
if (app.Environment.IsDevelopment()) 
{
    app.UseOpenApi();
    app.UseSwaggerUi(config => 
    {
        config.DocumentTitle = "Todo API";
        config.Path = "/docs";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}


// Endpoints...
app.MapGet("/", () => "Hello World! Am learning C#");

RouteGroupBuilder todoGroup = app.MapGroup("/todoitems");

todoGroup.MapGet("/", GetAllTodos);
todoGroup.MapGet("/complete", GetCompleteTodos);
todoGroup.MapGet("/{id}", GetTodo);
todoGroup.MapPost("/", CreateTodo);
todoGroup.MapPut("/{id}", UpdateTodo);
todoGroup.MapDelete("/{id}", DeleteTodo);

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
}

static async Task<IResult> GetCompleteTodos(TodoDb db) {
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo))
            : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };

    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoGroup/{todoItem.Id}", todoItemDTO);
}

static async Task<IResult> UpdateTodo(int id, TodoItemDTO todoItemDTO, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}

app.Run();