
var builder = WebApplication.CreateBuilder(args);
// 添加Yarp中间件
IReverseProxyBuilder reverseProxyBuilder = builder.Services.AddReverseProxy();
// 添加配置文件
reverseProxyBuilder.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
// 添加代理路由到管道内路由表中
app.MapReverseProxy();

app.Run();
