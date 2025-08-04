using AutoMapper;
using K8Intel.Dtos;
using K8Intel.Models;

namespace K8Intel.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Mappings
            CreateMap<User, UserDto>();

            // Cluster Mappings
            CreateMap<Cluster, ClusterDto>();
            CreateMap<CreateClusterDto, Cluster>();

            // Alert Mappings
            CreateMap<Alert, AlertDto>();
            CreateMap<CreateAlertDto, Alert>();

            // ClusterMetric Mappings
            CreateMap<ClusterMetric, MetricDto>();
            CreateMap<CreateMetricDto, ClusterMetric>();
        }
    }
}