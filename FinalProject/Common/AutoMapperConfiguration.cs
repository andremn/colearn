using AutoMapper;

namespace FinalProject.Common
{
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(mapperConfig =>
            {
            });
        }
    }
}