namespace Api.Controllers.Entity;

/// <summary>
/// Example of a simple controller that only needs CRUD operations.
/// No custom repository needed - just use GenericController and IGenericRepository.
/// </summary>
[ApiController]
[Route("[controller]")]
public class SimpleSampleProductController : GenericController<SampleProduct, int>
{
    public SimpleSampleProductController(IGenericRepository<SampleProduct, int> repository) 
        : base(repository)
    {
    }
}
