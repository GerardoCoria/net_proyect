using net_proyect;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using net_proyect.Models;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<TareasContext>(p=> p.UseInMemoryDatabase("TareasDB"));
builder.Services.AddNpgsql<TareasContext>(builder.Configuration.GetConnectionString("TareasDB"));

var app = builder.Build();

app.MapGet("/", () => "Hola Otto y Ragnar!");

app.MapGet("/db", async([FromServices] TareasContext dbContext) =>
{
    dbContext.Database.EnsureCreated();
    return Results.Ok("base de datos en memoria " + dbContext.Database.IsInMemory());
});

app.MapGet("/api/tareas", async([FromServices] TareasContext dbContext) =>
{
    return Results.Ok(dbContext.Tareas);
});

app.MapGet("/api/tareas-importantes", async([FromServices] TareasContext dbContext) =>
{
    return Results.Ok(dbContext.Tareas.Where(p=>p.PriodidadTarea == net_proyect.Models.Prioridad.alta));
});


app.MapPost("/api/tareas", async([FromServices] TareasContext dbContext, [FromBody] Tarea tarea) =>
{
    tarea.TareaId = Guid.NewGuid();
    tarea.FechaCreacion = DateTime.UtcNow;
    await dbContext.AddAsync(tarea);
    //await dbContext.Tareas.AddAsync(tarea);
    await dbContext.SaveChangesAsync();
    return Results.Ok();
});

app.MapPut("/api/tareas/{id}", async([FromServices] TareasContext dbContext, [FromBody] Tarea tarea, [FromRoute] Guid id) =>
{
    var tareaActual = dbContext.Tareas.Find(id);
    if(tareaActual!=null)
    {
        tareaActual.CategoriaId = tarea.CategoriaId;
        tareaActual.Titulo = tarea.Titulo;
        tareaActual.PriodidadTarea = tarea.PriodidadTarea;
        tareaActual.Descripcion = tarea.Descripcion;

        await dbContext.SaveChangesAsync();
        return Results.Ok();
    }
    return Results.NotFound();
});

app.MapDelete("/api/tareas/{id}", async([FromServices] TareasContext dbContext, [FromRoute] Guid id) =>
{
    var tareaActual = dbContext.Tareas.Find(id);
    if(tareaActual!=null)
    {
        dbContext.Remove(tareaActual);
    
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    }
    return Results.NotFound();
});

app.Run();