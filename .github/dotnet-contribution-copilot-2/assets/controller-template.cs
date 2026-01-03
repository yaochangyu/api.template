using Microsoft.AspNetCore.Mvc;
using CSharpFunctionalExtensions;

namespace {{NAMESPACE}};

/// <summary>
/// {{FEATURE_NAME}} API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class {{FEATURE_NAME}}Controller : ControllerBase
{
    private readonly {{FEATURE_NAME}}Handler _handler;
    private readonly ILogger<{{FEATURE_NAME}}Controller> _logger;

    public {{FEATURE_NAME}}Controller(
        {{FEATURE_NAME}}Handler handler,
        ILogger<{{FEATURE_NAME}}Controller> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    /// <summary>
    /// 取得{{FEATURE_DISPLAY_NAME}}清單
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<{{FEATURE_NAME}}Response>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _handler.GetAllAsync();

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(500, new { error = result.Error });
    }

    /// <summary>
    /// 根據 ID 取得{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof({{FEATURE_NAME}}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _handler.GetByIdAsync(id);

        if (result.IsFailure)
        {
            return result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : StatusCode(500, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// 建立新的{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof({{FEATURE_NAME}}Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] Create{{FEATURE_NAME}}Request request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _handler.CreateAsync(request);

        if (result.IsFailure)
        {
            return result.Error.Contains("already exists", StringComparison.OrdinalIgnoreCase)
                ? Conflict(new { error = result.Error })
                : StatusCode(500, new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            result.Value);
    }

    /// <summary>
    /// 更新{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof({{FEATURE_NAME}}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] Update{{FEATURE_NAME}}Request request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _handler.UpdateAsync(id, request);

        if (result.IsFailure)
        {
            return result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : StatusCode(500, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// 刪除{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _handler.DeleteAsync(id);

        if (result.IsFailure)
        {
            return result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : StatusCode(500, new { error = result.Error });
        }

        return NoContent();
    }
}

// ==================== DTOs ====================

/// <summary>
/// 建立{{FEATURE_DISPLAY_NAME}}請求
/// </summary>
public record Create{{FEATURE_NAME}}Request
{
    // TODO: 加入必要屬性
    // 範例:
    // public string Name { get; init; } = string.Empty;
    // public string Email { get; init; } = string.Empty;
}

/// <summary>
/// 更新{{FEATURE_DISPLAY_NAME}}請求
/// </summary>
public record Update{{FEATURE_NAME}}Request
{
    // TODO: 加入必要屬性
}

/// <summary>
/// {{FEATURE_DISPLAY_NAME}}回應
/// </summary>
public record {{FEATURE_NAME}}Response
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // TODO: 加入必要屬性
}
