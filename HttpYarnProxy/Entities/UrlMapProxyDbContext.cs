namespace HttpYarnProxy.Entities
{
    public class UrlMapProxyDbContext : IUrlMapProxyDbContext
    {
        public Task<UrlProxyMap?> SelectUrlMapById(Guid mapId)
        {
            return Task.FromResult(new UrlProxyMap { RequestType = "Get", Url = "http://127.0.0.1:6014" });
        }
    }
}
