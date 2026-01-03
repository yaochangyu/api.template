using Reqnroll;
using JobBank1111.Job.DB;
using Microsoft.EntityFrameworkCore;

namespace {{namespace}};

[Binding]
public class {{feature-name}}Steps(ScenarioContext scenarioContext) : Steps
{
    private readonly ScenarioContext _scenarioContext = scenarioContext;

    /// <summary>
    /// Pre-populates the database with {{feature-name}} data from a table.
    /// This step relies on the generic "Given資料庫已存在..." step in BaseStep.cs
    /// to handle the actual database insertion. This is just a placeholder
    /// to show how you might define a more specific step if needed.
    /// </summary>
    /// <param name="table">The table containing the data to insert.</param>
    [Given(@"資料庫已存在 {{feature-name}} 資料")]
    public async Task Given資料庫已存在Specific資料(Table table)
    {
        var userId = _scenarioContext.GetUserId();
        var now = _scenarioContext.GetUtcNow() ?? DateTimeOffset.UtcNow;
        
        // Create a set of entities from the feature file table
        var entities = table.CreateSet<{{feature-name}}Entity>(row => new {{feature-name}}Entity
        {
            // Example of mapping properties. Adjust to your entity.
            Id = row.ContainsKey("Id") ? Guid.Parse(row["Id"]) : Guid.NewGuid(),
            Name = row["Name"],
            // Add other properties from your table
            CreatedAt = now,
            CreatedBy = userId,
            ChangedAt = now,
            ChangedBy = userId
        });

        // Get DbContext and save data
        await using var dbContext = await _scenarioContext.GetMemberDbContextFactory().CreateDbContextAsync();
        await dbContext.Set<{{feature-name}}Entity>().AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Verifies that the database contains the expected {{feature-name}} data.
    /// </summary>
    /// <param name="table">The table with the expected data.</param>
    [Then(@"預期資料庫已存在 {{feature-name}} 資料為")]
    public async Task Then預期資料庫已存在Specific資料為(Table table)
    {
        await using var dbContext = await _scenarioContext.GetMemberDbContextFactory().CreateDbContextAsync();
        var actual = await dbContext.Set<{{feature-name}}Entity>().AsNoTracking().ToListAsync();
        
        // Compare the expected data from the feature file with the actual data in the DB
        table.CompareToSet(actual);
    }
}
