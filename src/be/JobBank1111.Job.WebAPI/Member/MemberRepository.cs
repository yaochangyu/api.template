﻿using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JobBank1111.Infrastructure;
using JobBank1111.Infrastructure.TraceContext;
using JobBank1111.Job.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace JobBank1111.Job.WebAPI.Member;

public class MemberRepository(
    ILogger<MemberRepository> logger,
    IContextGetter<TraceContext?> contextGetter,
    IDbContextFactory<MemberDbContext> dbContextFactory,
    TimeProvider timeProvider,
    IUuidProvider uuidProvider,
    IDistributedCache cache,
    JsonSerializerOptions jsonSerializerOptions)
{
    public async Task<int> InsertAsync(InsertMemberRequest request,
                                       CancellationToken cancel = default)
    {
        // throw new DbUpdateConcurrencyException("資料衝突了");

        var now = timeProvider.GetUtcNow();
        var traceContext = contextGetter.Get();
        var userId = traceContext.UserId;
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        var toDb = new DB.Member
        {
            Id = uuidProvider.NewId(),
            Name = request.Name,
            Age = request.Age,
            Email = request.Email,
            CreatedAt = now,
            CreatedBy = userId,
            ChangedAt = now,
            ChangedBy = userId
        };
        var entityEntry = dbContext.Members.Add(toDb);
        return await dbContext.SaveChangesAsync(cancel);
    }

    public async Task<Member> QueryEmailAsync(string email,
                                              CancellationToken cancel = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        var query = dbContext.Members
            .Where(p => p.Email == email)
            .Select(p => new Member
            {
                Id = p.Id,
                Name = p.Name,
                Age = p.Age,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                ChangedAt = p.ChangedAt,
                ChangedBy = p.ChangedBy
            });

        var result = await query.TagWith($"{nameof(MemberRepository)}.{nameof(this.QueryEmailAsync)}({email})")
            .AsNoTracking()
            .FirstOrDefaultAsync(cancel);
        return result;
    }

    public async Task<PaginatedList<GetMemberResponse>>
        GetMembersAsync(int pageIndex, int pageSize, bool noCache = false, CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Get();
        var userId = traceContext.UserId;
        PaginatedList<GetMemberResponse> result;
        var key = nameof(CacheKeys.MemberData);
        string cachedData = null;
        if (noCache == false) // 如果有快取，就從快取撈資料
        {
            cachedData = await cache.GetStringAsync(key, cancel);
            if (cachedData != null)
            {
                result = JsonSerializer.Deserialize<PaginatedList<GetMemberResponse>>(
                    cachedData, jsonSerializerOptions);
                return result;
            }
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

        var selector = dbContext.Members
            .Select(p => new GetMemberResponse { Id = p.Id, Name = p.Name, Age = p.Age, Email = p.Email })
            .AsNoTracking();

        var totalCount = selector.Count();
        var paging = selector.OrderBy(p => p.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize);
        var data = await paging
            .TagWith($"{nameof(MemberRepository)}.{nameof(this.GetMembersAsync)}")
            .ToListAsync(cancel);
        result = new PaginatedList<GetMemberResponse>(data, pageIndex, pageSize, totalCount);
        cachedData = JsonSerializer.Serialize(result, jsonSerializerOptions);
        cache.SetStringAsync(key, cachedData,
                             new DistributedCacheEntryOptions
                             {
                                 AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) //最好從組態設定讀取
                             }, cancel);

        return result;
    }

    public async Task<CursorPaginatedList<GetMemberResponse>>
        GetMembersAsync(int pageSize,
                        string nextPageToken,
                        bool noCache = true,
                        CancellationToken cancel = default)
    {
        // if (noCache) 永遠撈新的資料
        // else 撈快取的資料
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        var decodeResult = DecodePageToken(nextPageToken);
        var query = dbContext.Members
            .Select(p => p)
            .AsNoTracking();
        if (decodeResult.lastSequenceId > 0)
        {
            query = query.Where(p => p.SequenceId > decodeResult.lastSequenceId);
        }

        query = query.Take(pageSize + 1);
        var selector =
            query.Select(p => new GetMemberResponse
            {
                Id = p.Id,
                Name = p.Name,
                Age = p.Age,
                Email = p.Email,
                SequenceId = p.SequenceId
            });
        var results = await selector
            .TagWith($"{nameof(MemberRepository)}.{nameof(this.GetMembersAsync)}")
            .ToListAsync(cancel);

        // 是否有下一頁
        var hasNextPage = results.Count > pageSize;

        if (hasNextPage)
        {
            // 有下一頁，刪除最後一筆
            results.RemoveAt(results.Count - 1);

            // 產生下一頁的令牌
            var after = results.LastOrDefault();
            if (after != null)
            {
                nextPageToken = EncodePageToken(after.Id, after.SequenceId);
            }
            else
            {
                nextPageToken = null;
            }
        }

        return new CursorPaginatedList<GetMemberResponse>(results, nextPageToken, null);
    }

    // 將 Id 和 SequenceId 轉換為下一頁的令牌
    public static string EncodePageToken(string? lastId, long? lastSequenceId)
    {
        if (lastId == null || lastSequenceId == null)
        {
            return null;
        }

        var json = JsonSerializer.Serialize(new { lastId, lastSequenceId });
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    // 將下一頁的令牌解碼為 Id 和 SequenceId
    private static (string lastId, long lastSequenceId) DecodePageToken(string nextToken)
    {
        if (string.IsNullOrEmpty(nextToken))
        {
            return (null, 0);
        }

        string lastId = null;
        long lastSequenceId = 0;
        var base64Bytes = Convert.FromBase64String(nextToken);
        var json = Encoding.UTF8.GetString(base64Bytes);
        var jsonNode = JsonNode.Parse(json);
        var jsonObject = jsonNode.AsObject();
        if (jsonObject.TryGetPropertyValue("lastSequenceId", out var lastSequenceIdNode))
        {
            lastSequenceId = lastSequenceIdNode.GetValue<long>();
        }

        if (jsonObject.TryGetPropertyValue("lastId", out var lastIdNode))
        {
            lastId = lastIdNode.GetValue<string>();
        }

        return (lastId, lastSequenceId);
    }
}