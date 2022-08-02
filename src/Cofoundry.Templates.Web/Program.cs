﻿var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseLocalConfigFile();

builder.Services
    .AddControllersWithViews()
    .AddCofoundry(builder.Configuration);

var app = builder.Build();

if (!builder.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCofoundry();

app.Run();
