
var builder = WebApplication.CreateBuilder(args);
// ���Yarp�м��
IReverseProxyBuilder reverseProxyBuilder = builder.Services.AddReverseProxy();
// ��������ļ�
reverseProxyBuilder.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
// ��Ӵ���·�ɵ��ܵ���·�ɱ���
app.MapReverseProxy();

app.Run();
