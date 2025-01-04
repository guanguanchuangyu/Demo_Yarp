namespace HttpYarnProxy.Entities
{
    /// <summary>
    /// Url代理映射
    /// </summary>
    public class UrlProxyMap
    {
        /// <summary>
        /// 请求方法
        /// </summary>
        public string RequestType { get; set; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 请求Body参数
        /// </summary>

        public string ParamsJson { get; set; }
    }
}
