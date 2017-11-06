using AutoMapper.Configuration;
using LiveBolt.Models;
using LiveBolt.Models.HomeViewModels;

namespace LiveBolt
{
    public class MappingProfile : MapperConfigurationExpression
    {
        public MappingProfile()
        {
            // Register mappings between persistence models and view models
            CreateMap<ApplicationUser, HomeUserViewModel>().ReverseMap();
            CreateMap<Home, HomeStatusViewModel>().ReverseMap();
            CreateMap<DLM, HomeDLMViewModel>().ReverseMap();
            CreateMap<IDM, HomeIDMViewModel>().ReverseMap();
        }

        /*[Fact]
        public void MappingProfile_VerifyMappings()
        {
            var mappingProfile = new MappingProfile();

            var config = new MapperConfiguration(mappingProfile);
            var mapper = new Mapper(config);

            (mapper as IMapper).ConfigurationProvider.AssertConfigurationIsValid();
        }*/
    }
}
