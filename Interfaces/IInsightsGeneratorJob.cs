namespace K8Intel.Jobs;
public interface IInsightsGeneratorJob
{
    Task GenerateStabilityRecommendationsAsync();
}