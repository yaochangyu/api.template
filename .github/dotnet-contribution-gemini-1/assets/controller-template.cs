using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourProjectNamespace.Application.Features.FeatureName; // Adjust namespace as needed
using YourProjectNamespace.WebAPI.Contract; // Assuming FailureCode and Failure are here

namespace YourProjectNamespace.WebAPI.Features.FeatureName; // Adjust namespace as needed

[ApiController]
[Route("api/v1/[feature-name-plural]")] // Use kebab-case for URL
public class FeatureNameController(
    FeatureNameHandler handler) : ControllerBase
{
    /// <summary>
    /// Gets a list of FeatureName items.
    /// </summary>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>A list of FeatureName responses.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeatureNameResponse>>> GetFeatureNames(
        CancellationToken cancel = default)
    {
        var result = await handler.GetFeatureNamesAsync(cancel);
        if (result.IsFailure)
        {
            return BadRequest(result.Error); // Assuming Failure object is mapped to BadRequest
        }
        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a specific FeatureName item by ID.
    /// </summary>
    /// <param name="id">The ID of the FeatureName item.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>The FeatureName response if found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<FeatureNameResponse>> GetFeatureNameById(
        Guid id,
        CancellationToken cancel = default)
    {
        var result = await handler.GetFeatureNameByIdAsync(id, cancel);
        if (result.IsFailure)
        {
            if (result.Error.Code == FailureCode.NotFound) // Assuming a NotFound failure code
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new FeatureName item.
    /// </summary>
    /// <param name="request">The request body for creating a FeatureName.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>The created FeatureName response.</returns>
    [HttpPost]
    public async Task<ActionResult<FeatureNameResponse>> CreateFeatureName(
        CreateFeatureNameRequest request,
        CancellationToken cancel = default)
    {
        var result = await handler.CreateFeatureNameAsync(request, cancel);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return CreatedAtAction(nameof(GetFeatureNameById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Updates an existing FeatureName item.
    /// </summary>
    /// <param name="id">The ID of the FeatureName item to update.</param>
    /// <param name="request">The request body for updating a FeatureName.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateFeatureName(
        Guid id,
        UpdateFeatureNameRequest request,
        CancellationToken cancel = default)
    {
        // Optionally, ensure the ID in the route matches the ID in the request body
        // if (id != request.Id) return BadRequest("ID mismatch.");

        var result = await handler.UpdateFeatureNameAsync(id, request, cancel);
        if (result.IsFailure)
        {
            if (result.Error.Code == FailureCode.NotFound)
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    /// <summary>
    /// Deletes a FeatureName item by ID.
    /// </summary>
    /// <param name="id">The ID of the FeatureName item to delete.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFeatureName(
        Guid id,
        CancellationToken cancel = default)
    {
        var result = await handler.DeleteFeatureNameAsync(id, cancel);
        if (result.IsFailure)
        {
            if (result.Error.Code == FailureCode.NotFound)
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }
        return NoContent();
    }
}
