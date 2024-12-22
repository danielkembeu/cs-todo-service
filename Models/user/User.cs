public class User {

    public int Id { get; set; }
    public string Fullname { get; set; }
    public ICollection<Todo> Todos { get; set; }
}