using Microsoft.AspNetCore.Mvc.Routing;

namespace HttpYarnProxy.Routes
{
    /// <summary>
    /// 动态路由
    /// </summary>
    public class UrlMapRouterTransformer : DynamicRouteValueTransformer
    {
        private IServiceProvider serviceProvider;
        public UrlMapRouterTransformer(IServiceProvider provider)
        {
            serviceProvider = provider;
        }
        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (values == null)
                values = new RouteValueDictionary();

            values["controller"] = "urlmaproute";
            values["action"] = "getrealurldataresult";
            //values["id"] = values["id"];
            // 匹配 api 前缀 + 后续部分
            //api/xxxx     +  /bb?id= 123

            return values;
        }
    }
}
