using System.ComponentModel.DataAnnotations;

public class CreateProductRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public IFormFile? ImageFile { get; set; }
    public List<string>? Tags { get; set; }
    public string? Metadata { get; set; }
}