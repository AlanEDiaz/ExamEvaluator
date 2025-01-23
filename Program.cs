using ExamEvaluation.Api.Services;
using ExamEvaluator.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IEvaluationService, ExamEvaluationService>();
builder.Services.AddSingleton<IChatCompletionService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiKey = configuration["SemanticKernel:ApiKey"];
    var apiUrl = configuration["SemanticKernel:ApiUrl"];
    var chatDeploymentName = configuration["SemanticKernel:ChatDeploymentName"];
    return new AzureOpenAIChatCompletionService(chatDeploymentName!, apiUrl!, apiKey!);
});

builder.Services.AddKeyedTransient<Kernel>("ExamEvaluatorKernel", (sp, key) =>
{
    // Create a collection of plugins that the kernel will use
    KernelPluginCollection pluginCollection = new();
    return new Kernel(sp, pluginCollection);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
