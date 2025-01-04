using HttpYarnProxy.Entities;
using HttpYarnProxy.Routes;
using HttpYarnProxy.Services;
using System.Net;

namespace HttpYarnProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var services = builder.Services;

            services.AddControllers();
            // 服务注册
            services.AddScoped<IUrlMapProxyDbContext, UrlMapProxyDbContext>();

            services.AddScoped<UrlMapRouterTransformer>();

            // 指定特定类型依赖注入时，由httpclientfactory 构建指定httpclient实例
            services.AddHttpClient<UrlProxyService>(client =>
            {
                //client.BaseAddress = new Uri("http://10.50.18.1:6040");
                // 设置请求超时时间
                client.Timeout = TimeSpan.FromSeconds(30);
                //增加保活机制，表明连接为长连接(不随意修改请求是否长连接)
                //client.DefaultRequestHeaders.Connection.Add("keep-alive");
                ////启用保活机制（保持活动超时设置为 2 分钟，并将保持活动间隔设置为 1 秒。）
                //ServicePointManager.SetTcpKeepAlive(true, 60000, 1000);
                //默认连接数限制为2，增加连接数限制（考虑通过配置文件进行变更连接请求数）
                ServicePointManager.DefaultConnectionLimit = 1000;
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            // 设置请求处理使用SocketsHttpHandler
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                MaxConnectionsPerServer = 100,//高性能服务器+高性能客户端
                PooledConnectionLifetime = TimeSpan.FromMinutes(5) // 设置连接的最大存活时间为 5 分钟
            })
            ;

            // 添加分布式缓存内存缓存
            services.AddDistributedMemoryCache();

            // 构建Webapplication实例
            var app = builder.Build();

            // Configure the HTTP request pipeline.

            //app.UseAuthorization();

            app.UseRouting();

            // 动态路由配置
            app.MapDynamicControllerRoute<UrlMapRouterTransformer>("api/urlmaproute/{*pname}");

            app.MapControllers();

            app.Run();
        }
    }
}
