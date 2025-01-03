
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);
// ���Yarp�м��
IReverseProxyBuilder reverseProxyBuilder = builder.Services.AddReverseProxy();
// ������Դ
reverseProxyBuilder.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
reverseProxyBuilder.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy1"));
var app = builder.Build();
// ��Ӵ���·�ɵ��ܵ���·�ɱ���
app.MapReverseProxy();

app.Run();
