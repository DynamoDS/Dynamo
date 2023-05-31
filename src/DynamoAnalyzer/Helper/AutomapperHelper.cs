using AutoMapper;
using DynamoAnalyzer.Models;
using Greg.Responses;

namespace DynamoAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to configure Automapper
    /// </summary>
    public static class AutomapperHelper
    {
        /// <summary>
        /// Returns an instance of automapper
        /// </summary>
        /// <returns></returns>
        public static IMapper GetMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<PackageHeader, PackageHeaderCustom>());
            return config.CreateMapper();
        }
    }
}
