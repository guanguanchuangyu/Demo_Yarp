
var builder = WebApplication.CreateBuilder(args);
// 添加Yarp中间件
builder.Services.AddReverseProxy();

var app = builder.Build();

app.Run();
