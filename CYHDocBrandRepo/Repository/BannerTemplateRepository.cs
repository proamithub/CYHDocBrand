using Microsoft.Extensions.Caching.Memory;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandRepo.Repository;

public class BannerTemplateRepository : IBannerTemplateRepository
{
    private readonly string _conn;
    private readonly ILogger<BannerTemplateRepository>? _logger;
    private readonly IMemoryCache? _cache;

    public BannerTemplateRepository(IConfiguration configuration, IMemoryCache? cache = null, ILogger<BannerTemplateRepository>? logger = null)
    {
        _conn = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=AMITLAP;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<BannerCategoryDTO>> GetCategoriesAsync(bool onlyActive = true)
    {
        var cacheKey = $"cyh_banner_categories_{onlyActive}";
        if (_cache != null && _cache.TryGetValue(cacheKey, out List<BannerCategoryDTO>? cached) && cached != null)
        {
            return cached;
        }

        var list = new List<BannerCategoryDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerCategories_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", onlyActive ? "GET_ACTIVE_WITH_COUNTS" : "GET_ALL");

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new BannerCategoryDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    Slug = reader.GetString(reader.GetOrdinal("Slug")),
                    IconClass = reader.IsDBNull(reader.GetOrdinal("IconClass")) ? "bi-folder-fill" : reader.GetString(reader.GetOrdinal("IconClass")),
                    BadgeColor = reader.IsDBNull(reader.GetOrdinal("BadgeColor")) ? "#00f2fe" : reader.GetString(reader.GetOrdinal("BadgeColor")),
                    DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    TemplateCount = reader.IsDBNull(reader.GetOrdinal("TemplateCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("TemplateCount")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }

            if (_cache != null && list.Count > 0)
            {
                _cache.Set(cacheKey, list, TimeSpan.FromMinutes(10));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting banner categories via ManageBannerCategories_CYH");
        }
        return list;
    }

    public async Task<List<BannerTemplateDTO>> GetAllTemplatesAsync(int? categoryId = null, string? style = null, string? search = null)
    {
        var isMasterQuery = categoryId == null && string.IsNullOrEmpty(style) && string.IsNullOrEmpty(search);
        var cacheKey = "cyh_banner_templates_master_all";
        if (isMasterQuery && _cache != null && _cache.TryGetValue(cacheKey, out List<BannerTemplateDTO>? cached) && cached != null)
        {
            return cached;
        }

        var list = new List<BannerTemplateDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerTemplates_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_ALL");
            cmd.Parameters.AddWithValue("@CategoryId", (object?)categoryId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StyleTheme", (object?)style?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Search", (object?)search?.Trim() ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToTemplate(reader));
            }

            if (isMasterQuery && _cache != null && list.Count > 0)
            {
                _cache.Set(cacheKey, list, TimeSpan.FromMinutes(5));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting banner templates via ManageBannerTemplates_CYH");
        }
        return list;
    }

    public async Task<BannerTemplateDTO?> GetTemplateByIdAsync(int id)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerTemplates_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_BY_ID");
            cmd.Parameters.AddWithValue("@Id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToTemplate(reader);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting banner template ID {Id} via ManageBannerTemplates_CYH", id);
        }
        return null;
    }

    public async Task<int> SaveTemplateAsync(BannerTemplateDTO template)
    {
        _cache?.Remove("cyh_banner_templates_master_all");
        _cache?.Remove("cyh_banner_categories_True");
        _cache?.Remove("cyh_banner_categories_False");
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerTemplates_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "SAVE_TEMPLATE");
            cmd.Parameters.AddWithValue("@Id", template.Id > 0 ? template.Id : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId", template.CategoryId);
            cmd.Parameters.AddWithValue("@Title", template.Title);
            cmd.Parameters.AddWithValue("@StyleTheme", template.StyleTheme ?? "Modern Clean");
            cmd.Parameters.AddWithValue("@AspectRatio", template.AspectRatio ?? "1:1");
            cmd.Parameters.AddWithValue("@BackgroundImageUrl", template.BackgroundImageUrl);
            cmd.Parameters.AddWithValue("@ThumbnailUrl", (object?)template.ThumbnailUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrimaryColor", template.PrimaryColor ?? "#00f2fe");
            cmd.Parameters.AddWithValue("@SecondaryColor", template.SecondaryColor ?? "#0a192f");
            cmd.Parameters.AddWithValue("@TextColor", template.TextColor ?? "#ffffff");
            cmd.Parameters.AddWithValue("@DefaultHeading", template.DefaultHeading);
            cmd.Parameters.AddWithValue("@DefaultSubtitle", (object?)template.DefaultSubtitle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BadgeText", (object?)template.BadgeText ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ConfigJson", template.ConfigJson ?? string.Empty);
            cmd.Parameters.AddWithValue("@DisplayOrder", template.DisplayOrder);
            cmd.Parameters.AddWithValue("@IsActive", template.IsActive);

            var res = await cmd.ExecuteScalarAsync();
            if (res != null && int.TryParse(res.ToString(), out int savedId))
            {
                return savedId;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving banner template via ManageBannerTemplates_CYH");
        }
        return 0;
    }

    public async Task<bool> DeleteTemplateAsync(int id)
    {
        _cache?.Remove("cyh_banner_templates_master_all");
        _cache?.Remove("cyh_banner_categories_True");
        _cache?.Remove("cyh_banner_categories_False");
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerTemplates_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "DELETE");
            cmd.Parameters.AddWithValue("@Id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting banner template ID {Id} via ManageBannerTemplates_CYH", id);
            return false;
        }
    }

    private static BannerTemplateDTO MapReaderToTemplate(SqlDataReader r)
    {
        return new BannerTemplateDTO
        {
            Id = r.GetInt32(r.GetOrdinal("Id")),
            CategoryId = r.GetInt32(r.GetOrdinal("CategoryId")),
            CategoryName = r.IsDBNull(r.GetOrdinal("CategoryName")) ? "General" : r.GetString(r.GetOrdinal("CategoryName")),
            CategoryIcon = r.IsDBNull(r.GetOrdinal("CategoryIcon")) ? "bi-folder-fill" : r.GetString(r.GetOrdinal("CategoryIcon")),
            Title = r.GetString(r.GetOrdinal("Title")),
            StyleTheme = r.GetString(r.GetOrdinal("StyleTheme")),
            AspectRatio = r.GetString(r.GetOrdinal("AspectRatio")),
            BackgroundImageUrl = r.GetString(r.GetOrdinal("BackgroundImageUrl")),
            ThumbnailUrl = r.IsDBNull(r.GetOrdinal("ThumbnailUrl")) ? null : r.GetString(r.GetOrdinal("ThumbnailUrl")),
            PrimaryColor = r.GetString(r.GetOrdinal("PrimaryColor")),
            SecondaryColor = r.GetString(r.GetOrdinal("SecondaryColor")),
            TextColor = r.GetString(r.GetOrdinal("TextColor")),
            DefaultHeading = r.GetString(r.GetOrdinal("DefaultHeading")),
            DefaultSubtitle = r.IsDBNull(r.GetOrdinal("DefaultSubtitle")) ? null : r.GetString(r.GetOrdinal("DefaultSubtitle")),
            BadgeText = r.IsDBNull(r.GetOrdinal("BadgeText")) ? null : r.GetString(r.GetOrdinal("BadgeText")),
            ConfigJson = r.GetString(r.GetOrdinal("ConfigJson")),
            DisplayOrder = r.GetInt32(r.GetOrdinal("DisplayOrder")),
            IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
            CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
        };
    }
}
