
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);
// 添加Yarp中间件
IReverseProxyBuilder reverseProxyBuilder = builder.Services.AddReverseProxy();
// 多配置源
reverseProxyBuilder.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
reverseProxyBuilder.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy1"));
var app = builder.Build();
// 添加代理路由到管道内路由表中
app.MapReverseProxy();

app.Run();
