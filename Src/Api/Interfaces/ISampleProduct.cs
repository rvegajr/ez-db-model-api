namespace Api.Interfaces;

public interface ISampleProduct
{
    int Id { get; set; }
    string Name { get; set; }
    decimal Price { get; set; }
    string Description { get; set; }
}
