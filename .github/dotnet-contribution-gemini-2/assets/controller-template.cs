using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace {{namespace}};

[ApiController]
[Route("api/v1/{{feature-name-plural}}")]
public class {{feature-name}}Controller({{feature-name}}Handler handler) : ControllerBase
{
    /// <summary>
    /// Creates a new {{feature-name-singular}}.
    /// </summary>
    /// <param name="request">The request to create a {{feature-name-singular}}.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>The created {{feature-name-singular}}.</returns>
    [HttpPost(Name = "Create{{feature-name}}")]
    [ProducesResponseType(typeof({{feature-name}}Response), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create{{feature-name}}Async(
        [FromBody] Create{{feature-name}}Request request,
        CancellationToken cancel = default)
    {
        var result = await handler.CreateAsync(request, cancel);
        return result.Match<IActionResult>(
            value => CreatedAtRoute("Get{{feature-name}}ById", new { id = value.Id }, value),
            error => BadRequest(error)
        );
    }

    /// <summary>
    /// Gets a {{feature-name-singular}} by its ID.
    /// </summary>
    /// <param name="id">The ID of the {{feature-name-singular}} to get.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>The found {{feature-name-singular}}.</returns>
    [HttpGet("{id}", Name = "Get{{feature-name}}ById")]
    [ProducesResponseType(typeof({{feature-name}}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get{{feature-name}}ByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancel = default)
    {
        var result = await handler.GetByIdAsync(id, cancel);
        return result.Match<IActionResult>(
            Ok,
            _ => NotFound()
        );
    }
}
