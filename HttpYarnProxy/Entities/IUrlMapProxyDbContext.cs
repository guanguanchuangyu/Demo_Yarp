namespace HttpYarnProxy.Entities
{
    public interface IUrlMapProxyDbContext
    {
        Task<UrlProxyMap?> SelectUrlMapById(Guid mapId);
    }
}
