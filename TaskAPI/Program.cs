using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

//Instanciação, inicia a aplicação.
var builder = WebApplication.CreateBuilder(args);

//Definições do serviço Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Registro do Contexto como serviço.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("TasksDb"));

//Builda a aplicação.
var app = builder.Build();

// Configure the HTTP request pipeline.
//Habilita ambiente de desenvolvimento.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//A partir daqui defino os mapeamentos - Get, Post, Put, Delete ou - (Crud [Create, Read, Update, and Delete]).

app.MapGet("/", () => "Olá mundo.");

//app.MapGet("ReceitaWS", async () =>
//    await new HttpClient().GetStringAsync("https://receitaws.com.br/v1/cnpj/08582146000102")
//); Implementação para chamar o método Get na ReceitaWS.

//app.MapGet("Frases", async () =>
//    await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes")
//); Chama frases aleatórias da internet.

app.MapGet("/", () => "Olá mundo.");

app.MapGet("/tasks", async (AppDbContext db) => 
    await db.Tasks.ToListAsync());

app.MapGet("/tasks/{id}", async (int id, AppDbContext db) =>
    await db.Tasks.FindAsync(id) is Task task ? Results.Ok(task) : Results.NotFound());

app.MapGet("/tasks/finished", async (AppDbContext db) =>
    await db.Tasks.Where(t => t.IsFinished).ToListAsync());

app.MapPost("/tasks", async (Task task, AppDbContext db) =>
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapPut("/tasks/{id}", async(int id, Task taskInput, AppDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);

    if (task is null) return Results.NotFound();

    task.Name = taskInput.Name;
    task.IsFinished = taskInput.IsFinished;

    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapDelete("/tasks/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Tasks.FindAsync(id) is Task task)
    {
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return Results.Ok(task);
    }

    return Results.NotFound();
});

app.Run();

class Task
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsFinished { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Task> Tasks => Set<Task>();
}