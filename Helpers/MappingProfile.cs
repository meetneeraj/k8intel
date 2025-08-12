using AutoMapper;
using K8Intel.Dtos;
using K8Intel.Dtos.Common;
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
            CreateMap<Cluster, ClusterDto>()
                 .ForMember(dest => dest.HealthStatus, opt => opt.MapFrom(src =>
                    !src.LastAgentContactAt.HasValue
                        ? "Unknown"
                        : (src.LastAgentContactAt.Value < DateTime.UtcNow.AddMinutes(-10) ? "Offline" : "Healthy")));
            CreateMap<CreateClusterDto, Cluster>();

            // Alert Mappings
            CreateMap<Alert, AlertDto>();
            CreateMap<CreateAlertDto, Alert>();

            // ClusterMetric Mappings
            CreateMap<ClusterMetric, MetricDto>();
            CreateMap<CreateMetricDto, ClusterMetric>();

            CreateMap<Node, NodeDto>();
            CreateMap<Pod, PodDto>()
                .ForMember(dest => dest.NodeName, opt => opt.MapFrom(src => src.Node.Name));

            CreateMap<NodeDto, Node>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Don't map Id, let DB generate it
                .ForMember(dest => dest.Cluster, opt => opt.Ignore()); // Don't map navigation property

            CreateMap<PodDto, Pod>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Node, opt => opt.Ignore())
                .ForMember(dest => dest.NodeId, opt => opt.Ignore());
        }
    }
}