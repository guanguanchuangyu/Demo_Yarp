using HttpYarnProxy.Caches;
using HttpYarnProxy.Entities;
using HttpYarnProxy.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.ComponentModel;

namespace HttpYarnProxy.Controllers
{
    /// <summary>
    /// 映射地址数据管理模块
    /// </summary>
    [DisplayName("网站")]
    [ApiExplorerSettings(GroupName = "buss")]
    [Description("映射地址数据管理模块")]
    [Route("api/[controller]/[action]")]
    public class UrlMapRouteController : ControllerBase
    {
        private IUrlMapProxyDbContext _serverUrlMap;
        private ILogger _logger;
        private readonly IDistributedCache _cache;
        private readonly UrlProxyService _proxy;
        public UrlMapRouteController(IUrlMapProxyDbContext serverUrlMap, UrlProxyService urlProxyService, ILogger<UrlMapRouteController> logger, IDistributedCache cache)
        {
            _serverUrlMap = serverUrlMap;
            _logger = logger;
            _cache = cache;
            _proxy = urlProxyService;
        }
        /// <summary>
        /// 获取真实地址返回数据
        /// </summary>
        /// <param name="pname"></param>
        /// <param name="mapUrl"></param>
        /// <returns></returns>
        [Description("获取真实地址返回数据")]
        [HttpGet("{pname}")]
        public async Task GetRealUrlDataResult(string pname, string mapUrl)
        {
            if (string.IsNullOrEmpty(mapUrl))
            {
                throw new ArgumentNullException("代理地址不能为空");
            }
            mapUrl = System.Web.HttpUtility.UrlDecode(mapUrl);
            var param = mapUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            // 考虑以host为缓存key
            var mapId = Guid.Parse(param[2]);//获取映射id
            //获取真实地址数据
            UrlProxyMap entity = await _cache.GetAsync<UrlProxyMap>(mapId.ToString());
            if (entity == null)
            {
                entity = await _serverUrlMap.SelectUrlMapById(mapId);
                _logger.LogInformation($"读库加载:{mapId}");
                if (entity != null)
                {
                    // 缓存时间100秒(如果100秒内没访问就过期)
                    await _cache.SetAsync(mapId.ToString(), entity, options: new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(100) });
                }
            }

            //从路径中去除getrealurldataresult
            pname = pname.Contains("getrealurldataresult/") ? pname.Replace("getrealurldataresult/", "") : pname;
            pname = pname.Contains("getrealurldataresult") ? pname.Replace("getrealurldataresult", "") : pname;
            //路径拼接
            var url = entity.Url + (string.IsNullOrWhiteSpace(pname) ? "" : "/" + pname);
            //// 判定真实链接是否异常
            //if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri tmpuri)) {
            //    return;
            //}

            var sourceType = param[0]?.ToString();//获取服务类型  瓦片  影像 基础底图 矢量数据
            //如果是瓦片类型  最后的字符不是tileset.json   则去掉tileset.json
            if (sourceType == "Tile" && !url.EndsWith("tileset.json") && url.Contains("tileset.json"))
            {
                url = url.Replace("/tileset.json", "");
            }
            var mapType = param[1]?.ToString();//底图类型
            // 判定请求中是否包含x,y,z
            if (mapType == "xyz")
            {
                var x = HttpContext.Request.Query["x"];
                var y = HttpContext.Request.Query["y"];
                var z = HttpContext.Request.Query["z"];
                url = url.Replace("{x}", x).Replace("{y}", y).Replace("{z}", z);
            }
            // 判定url中是否包含?
            if (!url.Contains("?"))
            {
                url += "?";
            }
            //
            foreach (var query in HttpContext.Request.Query)
            {
                string key = query.Key.ToLower();
                if (key == "mapurl")
                {
                    // 去除多余参数
                    continue;
                }
                // 只要请求中包含x,y,z则进行替换
                if (key == "x" || key == "y" || key == "z")
                {
                    var value = HttpContext.Request.Query[key];
                    url = url.Replace($"{{{key}}}", value);
                    continue;
                }
                if (url.EndsWith("?"))
                {
                    url += $"{key}={query.Value}";
                }
                else
                {
                    url += $"&{key}={query.Value}";
                }
            }
            if (url.EndsWith("?"))
                url = url.Remove(url.LastIndexOf("?"));
            _logger.LogInformation($"代理地址：{url}");
            entity.Url = url;
            //请求真实地址 取到服务数据
            await _proxy.HttpClientRequestAsync(entity, HttpContext);
        }
    }
}
