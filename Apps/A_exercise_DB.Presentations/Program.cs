using A_exercise_DB.Presentations.Configs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationDependencies(
    builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var swaggerEnabled =
    builder.Configuration.GetValue<bool>(
        "Swagger:Enabled");

// Development環境、または設定で有効化された場合にSwaggerを公開
if (app.Environment.IsDevelopment()
    || swaggerEnabled)
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 現在の本番環境はNginxもHTTP運用のため、
// ProductionではHTTPSリダイレクトしない
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();