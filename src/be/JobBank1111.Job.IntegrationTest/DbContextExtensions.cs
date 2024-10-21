﻿using System.Data;
using JobBank1111.Job.DB;
using JobBank1111.Testing.Common;
using Microsoft.EntityFrameworkCore;

namespace JobBank1111.Job.WebAPI.IntegrationTest;

public static class DbContextExtensions
{
    public static void ClearAllData(this MemberDbContext dbContext)
    {
        SqlServerGenerateScript.OnlySupportLocal(dbContext.Database.GetConnectionString());
        dbContext.Database.ExecuteSqlRaw(SqlServerGenerateScript.ClearAllRecord());
    }

    public static async Task Initial(this MemberDbContext dbContext)
    {
        SqlServerGenerateScript.OnlySupportLocal(dbContext.Database.GetConnectionString());
        // dbContext.Database.EnsureDeleted();

        var migrations = dbContext.Database.GetMigrations();
        if (migrations != null && migrations.Any())
        {
            dbContext.Database.Migrate();
        }
        else
        {
            dbContext.Database.EnsureCreated();
        }

        await dbContext.Seed();
    }

    public static async Task Seed(this MemberDbContext dbContext)
    {
        SqlServerGenerateScript.OnlySupportLocal(dbContext.Database.GetConnectionString());

        var dbConnection = dbContext.Database.GetDbConnection();
        if (dbConnection.State != ConnectionState.Open)
        {
            await dbConnection.OpenAsync();
        }

        //讀取資料夾的所有 sql 檔案，並執行
        var sqlFiles = Directory.GetFiles("DB/Scripts", "*.sql");

        foreach (var sqlFile in sqlFiles)
        {
            var sql = await File.ReadAllTextAsync(sqlFile);
            await using var cmd = dbConnection.CreateCommand();

            // Iterate the string array and execute each one.
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();

            // dbContext.Database.ExecuteSqlRaw(sql);
        }
    }
}