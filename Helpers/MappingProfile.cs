using AutoMapper;
using K8Intel.Dtos;
using K8Intel.Models;
using System;

namespace K8Intel.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- User, Alert, Metric Mappings ---
            CreateMap<User, UserDto>();
            CreateMap<Alert, AlertDto>();
            CreateMap<CreateAlertDto, Alert>();
            CreateMap<ClusterMetric, MetricDto>();
            CreateMap<CreateMetricDto, ClusterMetric>();
            
            // --- Cluster Mappings ---
            CreateMap<CreateClusterDto, Cluster>();
            CreateMap<Cluster, ClusterDto>()
                .ForMember(
                    dest => dest.HealthStatus,
                    opt => opt.MapFrom(src =>
                        !src.LastAgentContactAt.HasValue
                            ? "Unknown"
                            : (src.LastAgentContactAt.Value < DateTime.UtcNow.AddMinutes(-10)
                                ? "Offline"
                                : "Healthy")));
                                
            // --- K8s Object Mappings ---
            CreateMap<Node, NodeDto>();
            CreateMap<Pod, PodDto>().ForMember(dest => dest.NodeName, opt => opt.MapFrom(src => src.Node.Name));
            CreateMap<NodeDto, Node>().ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<PodDto, Pod>().ForMember(dest => dest.Id, opt => opt.Ignore());

            // --- Insight Mappings ---
            CreateMap<Incident, IncidentDto>();
            CreateMap<Recommendation, RecommendationDto>().ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}