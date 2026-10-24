using CYHDocBrandModel.DTO;

namespace CYHDocBrandUI.Services;

public interface IBannerAiService
{
    Task<BannerGenerationResult> GenerateBannerAsync(BannerGenerateRequest request);
    List<string> GetSupportedAspectRatios();
    List<string> GetTemplatePresets();
}
