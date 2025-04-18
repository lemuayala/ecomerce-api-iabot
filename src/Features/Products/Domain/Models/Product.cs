public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
