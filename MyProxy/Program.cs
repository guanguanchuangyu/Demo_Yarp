
var builder = WebApplication.CreateBuilder(args);
// ���Yarp�м��
builder.Services.AddReverseProxy();

var app = builder.Build();

app.Run();
