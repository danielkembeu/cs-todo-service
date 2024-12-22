// La classe qui me permettra d'interagir avec une BD.
using Microsoft.EntityFrameworkCore;

class TodoDb: DbContext {

    public TodoDb(DbContextOptions<TodoDb> options): base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}