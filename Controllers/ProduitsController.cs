using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProduitsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new string[] { "Produit1", "Produit2" });
    }
}
