using LearnQuestV1.Api.DTOs.Contents;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace LearnQuestV1.Api.Services.Implementations
{
    /// <summary>
    /// Service for validating content quality, completeness, and identifying issues
    /// </summary>
    
    public class ContentValidationService : IContentValidationService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ContentValidationService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly HttpClient _httpClient;

        // Validation rules configuration
        private readonly int _minTitleLength = 5;
        private readonly int _maxTitleLength = 200;
        private readonly int _minDescriptionLength = 10;
        private readonly int _maxDescriptionLength = 1000;
        private readonly int _minTextContentLength = 50;
        private readonly int _maxContentDuration = 300; // 5 hours in minutes
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".webm" };
        private readonly string[] _allowedDocExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".txt" };

        public ContentValidationService(
            IUnitOfWork uow,
            ILogger<ContentValidationService> logger,
            IWebHostEnvironment webHostEnvironment,
            HttpClient httpClient)
        {
            _uow = uow;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _httpClient = httpClient;
        }

        // =====================================================
        // Main Validation Methods
        // =====================================================

        public async Task<ContentValidationResultDto> ValidateContentAsync(int contentId)
        {
            try
            {
                var content = await _uow.Contents.Query()
                    .Include(c => c.Section)
                        .ThenInclude(s => s.Level)
                            .ThenInclude(l => l.Course)
                    .FirstOrDefaultAsync(c => c.ContentId == contentId && !c.IsDeleted);

                if (content == null)
                {
                    return new ContentValidationResultDto
                    {
                        ContentId = contentId,
                        Title = "Unknown Content",
                        IsValid = false,
                        Issues = new[] { "Content not found or has been deleted" },
                        Severity = ContentValidationSeverity.Critical
                    };
                }

                var issues = new List<string>();
                var warnings = new List<string>();

                // Validate basic properties
                ValidateBasicProperties(content, issues, warnings);

                // Validate content type specific requirements
                await ValidateContentTypeSpecificAsync(content, issues, warnings);

                // Validate file existence and accessibility
                await ValidateFileExistenceAsync(content, issues, warnings);

                // Validate content structure and quality
                ValidateContentQuality(content, issues, warnings);

                // Validate accessibility requirements
                ValidateAccessibility(content, issues, warnings);

                // Determine overall severity
                var severity = DetermineSeverity(issues, warnings);

                return new ContentValidationResultDto
                {
                    ContentId = contentId,
                    Title = content.Title,
                    IsValid = !issues.Any(),
                    Issues = issues,
                    Warnings = warnings,
                    Severity = severity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating content {ContentId}", contentId);
                return new ContentValidationResultDto
                {
                    ContentId = contentId,
                    Title = "Validation Error",
                    IsValid = false,
                    Issues = new[] { "An error occurred during validation" },
                    Severity = ContentValidationSeverity.Error
                };
            }
        }

        public async Task<IEnumerable<ContentValidationResultDto>> ValidateBulkContentAsync(IEnumerable<int> contentIds)
        {
            var results = new List<ContentValidationResultDto>();

            foreach (var contentId in contentIds)
            {
                try
                {
                    var result = await ValidateContentAsync(contentId);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating content {ContentId} in bulk operation", contentId);
                    results.Add(new ContentValidationResultDto
                    {
                        ContentId = contentId,
                        Title = "Validation Error",
                        IsValid = false,
                        Issues = new[] { "An error occurred during validation" },
                        Severity = ContentValidationSeverity.Error
                    });
                }
            }

            return results;
        }

        public async Task<IEnumerable<ContentIssueDto>> GetContentIssuesAsync(int? instructorId = null)
        {
            try
            {
                var query = _uow.Contents.Query()
                    .Include(c => c.Section)
                        .ThenInclude(s => s.Level)
                            .ThenInclude(l => l.Course)
                    .Where(c => !c.IsDeleted);

                if (instructorId.HasValue)
                {
                    query = query.Where(c => c.Section.Level.Course.InstructorId == instructorId.Value);
                }

                var contents = await query.ToListAsync();
                var issues = new List<ContentIssueDto>();

                foreach (var content in contents)
                {
                    var validationResult = await ValidateContentAsync(content.ContentId);

                    if (!validationResult.IsValid || validationResult.Warnings.Any())
                    {
                        // Add issues
                        foreach (var issue in validationResult.Issues)
                        {
                            issues.Add(new ContentIssueDto
                            {
                                ContentId = content.ContentId,
                                Title = content.Title,
                                IssueType = "Validation Error",
                                Description = issue,
                                Severity = validationResult.Severity,
                                DetectedAt = DateTime.UtcNow,
                                IsResolved = false
                            });
                        }

                        // Add warnings
                        foreach (var warning in validationResult.Warnings)
                        {
                            issues.Add(new ContentIssueDto
                            {
                                ContentId = content.ContentId,
                                Title = content.Title,
                                IssueType = "Warning",
                                Description = warning,
                                Severity = ContentValidationSeverity.Warning,
                                DetectedAt = DateTime.UtcNow,
                                IsResolved = false
                            });
                        }
                    }
                }

                return issues.OrderByDescending(i => i.Severity).ThenBy(i => i.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content issues for instructor {InstructorId}", instructorId);
                return Enumerable.Empty<ContentIssueDto>();
            }
        }

        // =====================================================
        // Specific Validation Methods
        // =====================================================

        private void ValidateBasicProperties(Content content, List<string> issues, List<string> warnings)
        {
            // Title validation
            if (string.IsNullOrWhiteSpace(content.Title))
            {
                issues.Add("Content title is required");
            }
            else if (content.Title.Length < _minTitleLength)
            {
                warnings.Add($"Content title is too short (minimum {_minTitleLength} characters)");
            }
            else if (content.Title.Length > _maxTitleLength)
            {
                issues.Add($"Content title is too long (maximum {_maxTitleLength} characters)");
            }

            // Description validation
            if (!string.IsNullOrWhiteSpace(content.ContentDescription))
            {
                if (content.ContentDescription.Length < _minDescriptionLength)
                {
                    warnings.Add($"Content description is too short (minimum {_minDescriptionLength} characters)");
                }
                else if (content.ContentDescription.Length > _maxDescriptionLength)
                {
                    issues.Add($"Content description is too long (maximum {_maxDescriptionLength} characters)");
                }
            }
            else
            {
                warnings.Add("Content description is missing - descriptions help users understand the content");
            }

            // Duration validation
            if (content.DurationInMinutes <= 0)
            {
                warnings.Add("Content duration is not specified or is zero");
            }
            else if (content.DurationInMinutes > _maxContentDuration)
            {
                warnings.Add($"Content duration is very long ({content.DurationInMinutes} minutes) - consider breaking into smaller parts");
            }

            // Order validation
            if (content.ContentOrder <= 0)
            {
                issues.Add("Content order must be greater than zero");
            }
        }

        private async Task ValidateContentTypeSpecificAsync(Content content, List<string> issues, List<string> warnings)
        {
            switch (content.ContentType)
            {
                case ContentType.Video:
                    await ValidateVideoContentAsync(content, issues, warnings);
                    break;
                case ContentType.Doc:
                    await ValidateDocumentContentAsync(content, issues, warnings);
                    break;
                case ContentType.Text:
                    ValidateTextContent(content, issues, warnings);
                    break;
                default:
                    issues.Add($"Unknown content type: {content.ContentType}");
                    break;
            }
        }

        private async Task ValidateVideoContentAsync(Content content, List<string> issues, List<string> warnings)
        {
            if (string.IsNullOrWhiteSpace(content.ContentUrl))
            {
                issues.Add("Video content must have a valid URL");
                return;
            }

            // Validate URL format
            if (!Uri.TryCreate(content.ContentUrl, UriKind.RelativeOrAbsolute, out Uri? uri))
            {
                issues.Add("Video URL is not in a valid format");
                return;
            }

            // Check if it's a local file or external URL
            if (uri.IsAbsoluteUri)
            {
                // External URL - validate if accessible
                await ValidateExternalUrlAsync(content.ContentUrl, "video", issues, warnings);
            }
            else
            {
                // Local file - validate extension and existence
                var extension = Path.GetExtension(content.ContentUrl).ToLowerInvariant();
                if (!_allowedVideoExtensions.Contains(extension))
                {
                    issues.Add($"Video file extension '{extension}' is not supported");
                }
            }
        }

        private async Task ValidateDocumentContentAsync(Content content, List<string> issues, List<string> warnings)
        {
            if (string.IsNullOrWhiteSpace(content.ContentDoc))
            {
                issues.Add("Document content must have a valid file path");
                return;
            }

            // Validate file extension
            var extension = Path.GetExtension(content.ContentDoc).ToLowerInvariant();
            if (!_allowedDocExtensions.Contains(extension))
            {
                issues.Add($"Document file extension '{extension}' is not supported");
            }

            // Check file existence if it's a local file
            if (!content.ContentDoc.StartsWith("http"))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, content.ContentDoc.TrimStart('/'));
                if (!File.Exists(fullPath))
                {
                    issues.Add("Document file does not exist on the server");
                }
            }
        }

        private void ValidateTextContent(Content content, List<string> issues, List<string> warnings)
        {
            if (string.IsNullOrWhiteSpace(content.ContentText))
            {
                issues.Add("Text content cannot be empty");
                return;
            }

            if (content.ContentText.Length < _minTextContentLength)
            {
                warnings.Add($"Text content is too short (minimum {_minTextContentLength} characters recommended)");
            }

            // Check for potential formatting issues
            if (content.ContentText.Contains("<script>") || content.ContentText.Contains("javascript:"))
            {
                issues.Add("Text content contains potentially unsafe script elements");
            }

            // Check for broken internal links
            var internalLinkPattern = @"href=[""'](/[^""']*)[""']";
            var matches = Regex.Matches(content.ContentText, internalLinkPattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var link = match.Groups[1].Value;
                if (link.StartsWith("/uploads/"))
                {
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, link.TrimStart('/'));
                    if (!File.Exists(fullPath))
                    {
                        warnings.Add($"Referenced file in text content does not exist: {link}");
                    }
                }
            }
        }

        private async Task ValidateFileExistenceAsync(Content content, List<string> issues, List<string> warnings)
        {
            try
            {
                switch (content.ContentType)
                {
                    case ContentType.Video when !string.IsNullOrWhiteSpace(content.ContentUrl):
                        if (!content.ContentUrl.StartsWith("http") && !content.ContentUrl.StartsWith("//"))
                        {
                            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, content.ContentUrl.TrimStart('/'));
                            if (!File.Exists(fullPath))
                            {
                                issues.Add("Video file does not exist on the server");
                            }
                        }
                        break;

                    case ContentType.Doc when !string.IsNullOrWhiteSpace(content.ContentDoc):
                        if (!content.ContentDoc.StartsWith("http"))
                        {
                            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, content.ContentDoc.TrimStart('/'));
                            if (!File.Exists(fullPath))
                            {
                                issues.Add("Document file does not exist on the server");
                            }
                            else
                            {
                                // Check file size and properties
                                var fileInfo = new FileInfo(fullPath);
                                if (fileInfo.Length == 0)
                                {
                                    warnings.Add("Document file is empty");
                                }
                                else if (fileInfo.Length > 50 * 1024 * 1024) // 50MB
                                {
                                    warnings.Add("Document file is very large (over 50MB) - consider compressing");
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating file existence for content {ContentId}", content.ContentId);
                warnings.Add("Could not verify file existence");
            }
        }

        private void ValidateContentQuality(Content content, List<string> issues, List<string> warnings)
        {
            // Check for placeholder content
            var commonPlaceholders = new[] { "lorem ipsum", "placeholder", "todo", "tbd", "coming soon", "under construction" };
            var titleLower = content.Title.ToLowerInvariant();
            var descriptionLower = content.ContentDescription?.ToLowerInvariant() ?? "";
            var textLower = content.ContentText?.ToLowerInvariant() ?? "";

            foreach (var placeholder in commonPlaceholders)
            {
                if (titleLower.Contains(placeholder) || descriptionLower.Contains(placeholder) || textLower.Contains(placeholder))
                {
                    warnings.Add($"Content appears to contain placeholder text: '{placeholder}'");
                }
            }

            // Check for repetitive content
            if (!string.IsNullOrWhiteSpace(content.ContentText))
            {
                var sentences = content.ContentText.Split('.', '!', '?')
                    .Where(s => s.Trim().Length > 10)
                    .Select(s => s.Trim().ToLowerInvariant())
                    .ToList();

                var duplicateSentences = sentences.GroupBy(s => s)
                    .Where(g => g.Count() > 3)
                    .Select(g => g.Key);

                if (duplicateSentences.Any())
                {
                    warnings.Add("Content contains repetitive text that may indicate copy-paste errors");
                }
            }

            // Check for appropriate content length vs duration
            if (content.ContentType == ContentType.Text && !string.IsNullOrWhiteSpace(content.ContentText))
            {
                var estimatedReadingTime = content.ContentText.Split(' ').Length / 200; // Average reading speed
                if (content.DurationInMinutes > 0 && Math.Abs(estimatedReadingTime - content.DurationInMinutes) > 5)
                {
                    warnings.Add("Estimated reading time doesn't match specified duration");
                }
            }
        }

        private void ValidateAccessibility(Content content, List<string> issues, List<string> warnings)
        {
            if (content.ContentType == ContentType.Text && !string.IsNullOrWhiteSpace(content.ContentText))
            {
                // Check for alt text in images
                var imgTagPattern = @"<img[^>]*>";
                var imgMatches = Regex.Matches(content.ContentText, imgTagPattern, RegexOptions.IgnoreCase);

                foreach (Match match in imgMatches)
                {
                    if (!match.Value.Contains("alt=", StringComparison.OrdinalIgnoreCase))
                    {
                        warnings.Add("Images in text content should include alt text for accessibility");
                        break;
                    }
                }

                // Check for proper heading structure
                var headingPattern = @"<h[1-6][^>]*>";
                var headingMatches = Regex.Matches(content.ContentText, headingPattern, RegexOptions.IgnoreCase);

                if (content.ContentText.Length > 1000 && !headingMatches.Any())
                {
                    warnings.Add("Long text content should use headings for better accessibility and readability");
                }
            }

            // Check for video accessibility
            if (content.ContentType == ContentType.Video)
            {
                if (string.IsNullOrWhiteSpace(content.ContentDescription))
                {
                    warnings.Add("Video content should include a description for accessibility");
                }
            }
        }

        private async Task ValidateExternalUrlAsync(string url, string expectedType, List<string> issues, List<string> warnings)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    issues.Add($"External {expectedType} URL is not accessible (HTTP {(int)response.StatusCode})");
                    return;
                }

                // Check content type
                var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(contentType))
                {
                    var isValidType = expectedType switch
                    {
                        "video" => contentType.StartsWith("video/"),
                        "document" => contentType.StartsWith("application/") || contentType.StartsWith("text/"),
                        _ => true
                    };

                    if (!isValidType)
                    {
                        warnings.Add($"External URL content type '{contentType}' may not be appropriate for {expectedType} content");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not validate external URL: {Url}", url);
                warnings.Add($"Could not verify external {expectedType} URL accessibility");
            }
        }

        private ContentValidationSeverity DetermineSeverity(List<string> issues, List<string> warnings)
        {
            if (issues.Any(i => i.Contains("not found") || i.Contains("does not exist") || i.Contains("Critical")))
            {
                return ContentValidationSeverity.Critical;
            }

            if (issues.Any())
            {
                return ContentValidationSeverity.Error;
            }

            if (warnings.Any())
            {
                return ContentValidationSeverity.Warning;
            }

            return ContentValidationSeverity.None;
        }

        // =====================================================
        // Additional Validation Methods
        // =====================================================

        public async Task<ContentQualityScoreDto> CalculateContentQualityScoreAsync(int contentId)
        {
            var validationResult = await ValidateContentAsync(contentId);
            var content = await _uow.Contents.GetByIdAsync(contentId);

            if (content == null)
            {
                return new ContentQualityScoreDto
                {
                    ContentId = contentId,
                    QualityScore = 0,
                    MaxScore = 100,
                    ScoreBreakdown = new Dictionary<string, int>()
                };
            }

            var scoreBreakdown = new Dictionary<string, int>();
            var totalScore = 0;
            var maxScore = 100;

            // Basic completeness (40 points)
            var completenessScore = CalculateCompletenessScore(content, validationResult);
            scoreBreakdown["Completeness"] = completenessScore;
            totalScore += completenessScore;

            // Quality factors (30 points)
            var qualityScore = CalculateQualityScore(content, validationResult);
            scoreBreakdown["Quality"] = qualityScore;
            totalScore += qualityScore;

            // Accessibility (20 points)
            var accessibilityScore = CalculateAccessibilityScore(content, validationResult);
            scoreBreakdown["Accessibility"] = accessibilityScore;
            totalScore += accessibilityScore;

            // Technical correctness (10 points)
            var technicalScore = CalculateTechnicalScore(validationResult);
            scoreBreakdown["Technical"] = technicalScore;
            totalScore += technicalScore;

            return new ContentQualityScoreDto
            {
                ContentId = contentId,
                Title = content.Title,
                QualityScore = totalScore,
                MaxScore = maxScore,
                ScoreBreakdown = scoreBreakdown,
                QualityLevel = DetermineQualityLevel(totalScore),
                Recommendations = GenerateRecommendations(validationResult, scoreBreakdown)
            };
        }

        public async Task<IEnumerable<ContentIssueDto>> ScanForBrokenLinksAsync(int? instructorId = null)
        {
            var issues = new List<ContentIssueDto>();

            var query = _uow.Contents.Query()
                .Include(c => c.Section.Level.Course)
                .Where(c => !c.IsDeleted &&
                           (c.ContentType == ContentType.Video && !string.IsNullOrEmpty(c.ContentUrl) ||
                            c.ContentType == ContentType.Doc && !string.IsNullOrEmpty(c.ContentDoc)));

            if (instructorId.HasValue)
            {
                query = query.Where(c => c.Section.Level.Course.InstructorId == instructorId.Value);
            }

            var contents = await query.ToListAsync();

            foreach (var content in contents)
            {
                await CheckContentLinks(content, issues);
            }

            return issues;
        }

        public async Task<IEnumerable<ContentIssueDto>> ScanForMissingFilesAsync(int? instructorId = null)
        {
            var issues = new List<ContentIssueDto>();

            var query = _uow.Contents.Query()
                .Include(c => c.Section.Level.Course)
                .Where(c => !c.IsDeleted);

            if (instructorId.HasValue)
            {
                query = query.Where(c => c.Section.Level.Course.InstructorId == instructorId.Value);
            }

            var contents = await query.ToListAsync();

            foreach (var content in contents)
            {
                await CheckMissingFiles(content, issues);
            }

            return issues;
        }

        public async Task<ContentAccessibilityReportDto> ValidateContentAccessibilityAsync(int contentId)
        {
            var content = await _uow.Contents.GetByIdAsync(contentId);
            if (content == null)
            {
                return new ContentAccessibilityReportDto
                {
                    ContentId = contentId,
                    AccessibilityScore = 0,
                    Issues = new[] { "Content not found" }
                };
            }

            var issues = new List<string>();
            var recommendations = new List<string>();
            var score = 100;

            // Check various accessibility criteria
            if (content.ContentType == ContentType.Video)
            {
                if (string.IsNullOrWhiteSpace(content.ContentDescription))
                {
                    issues.Add("Video lacks descriptive text");
                    recommendations.Add("Add a detailed description of the video content");
                    score -= 20;
                }
            }

            if (content.ContentType == ContentType.Text && !string.IsNullOrWhiteSpace(content.ContentText))
            {
                // Check for proper heading structure
                if (!Regex.IsMatch(content.ContentText, @"<h[1-6]", RegexOptions.IgnoreCase) && content.ContentText.Length > 500)
                {
                    issues.Add("Long text content lacks proper heading structure");
                    recommendations.Add("Use headings (H1, H2, etc.) to structure content");
                    score -= 15;
                }

                // Check for alt text in images
                var imgMatches = Regex.Matches(content.ContentText, @"<img[^>]*>", RegexOptions.IgnoreCase);
                var imagesWithoutAlt = imgMatches.Cast<Match>()
                    .Count(m => !m.Value.Contains("alt=", StringComparison.OrdinalIgnoreCase));

                if (imagesWithoutAlt > 0)
                {
                    issues.Add($"{imagesWithoutAlt} images lack alt text");
                    recommendations.Add("Add descriptive alt text to all images");
                    score -= imagesWithoutAlt * 10;
                }
            }

            return new ContentAccessibilityReportDto
            {
                ContentId = contentId,
                Title = content.Title,
                AccessibilityScore = Math.Max(0, score),
                Issues = issues,
                Recommendations = recommendations,
                ComplianceLevel = score >= 80 ? "Good" : score >= 60 ? "Fair" : "Poor"
            };
        }

        // =====================================================
        // Helper Methods
        // =====================================================

        private int CalculateCompletenessScore(Content content, ContentValidationResultDto validation)
        {
            var score = 40;

            if (string.IsNullOrWhiteSpace(content.Title) || content.Title.Length < _minTitleLength)
                score -= 10;

            if (string.IsNullOrWhiteSpace(content.ContentDescription) || content.ContentDescription.Length < _minDescriptionLength)
                score -= 10;

            if (content.DurationInMinutes <= 0)
                score -= 10;

            if (validation.Issues.Any(i => i.Contains("required") || i.Contains("empty")))
                score -= 10;

            return Math.Max(0, score);
        }

        private int CalculateQualityScore(Content content, ContentValidationResultDto validation)
        {
            var score = 30;

            if (validation.Warnings.Any(w => w.Contains("placeholder") || w.Contains("repetitive")))
                score -= 10;

            if (validation.Warnings.Any(w => w.Contains("too short")))
                score -= 5;

            if (validation.Issues.Any())
                score -= 15;

            return Math.Max(0, score);
        }

        private int CalculateAccessibilityScore(Content content, ContentValidationResultDto validation)
        {
            var score = 20;

            if (validation.Warnings.Any(w => w.Contains("accessibility") || w.Contains("alt text") || w.Contains("headings")))
                score -= 10;

            return Math.Max(0, score);
        }

        private int CalculateTechnicalScore(ContentValidationResultDto validation)
        {
            var score = 10;

            if (validation.Issues.Any(i => i.Contains("does not exist") || i.Contains("not accessible")))
                score -= 10;

            return Math.Max(0, score);
        }

        private string DetermineQualityLevel(int score)
        {
            return score switch
            {
                >= 90 => "Excellent",
                >= 80 => "Good",
                >= 70 => "Fair",
                >= 60 => "Poor",
                _ => "Very Poor"
            };
        }

        private IEnumerable<string> GenerateRecommendations(ContentValidationResultDto validation, Dictionary<string, int> scores)
        {
            var recommendations = new List<string>();

            if (scores["Completeness"] < 30)
                recommendations.Add("Complete all required fields including title, description, and duration");

            if (scores["Quality"] < 20)
                recommendations.Add("Review content for quality - remove placeholder text and ensure uniqueness");

            if (scores["Accessibility"] < 15)
                recommendations.Add("Improve accessibility by adding alt text, descriptions, and proper structure");

            if (scores["Technical"] < 8)
                recommendations.Add("Fix technical issues such as broken links or missing files");

            return recommendations;
        }

        private async Task CheckContentLinks(Content content, List<ContentIssueDto> issues)
        {
            var urlToCheck = content.ContentType == ContentType.Video
                ? content.ContentUrl
                : content.ContentDoc;

            if (string.IsNullOrEmpty(urlToCheck))
                return;

            if (!Uri.TryCreate(urlToCheck, UriKind.Absolute, out var uri))
                return;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, uri);
                using var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead
                );

                if (!response.IsSuccessStatusCode)
                {
                    issues.Add(new ContentIssueDto
                    {
                        ContentId = content.ContentId,
                        Title = content.Title,
                        IssueType = "Broken Link",
                        Description = $"External URL returns HTTP {(int)response.StatusCode}: {urlToCheck}",
                        Severity = ContentValidationSeverity.Error,
                        DetectedAt = DateTime.UtcNow,
                        IsResolved = false
                    });
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ContentIssueDto
                {
                    ContentId = content.ContentId,
                    Title = content.Title,
                    IssueType = "Broken Link",
                    Description = $"Cannot access external URL: {urlToCheck} – {ex.Message}",
                    Severity = ContentValidationSeverity.Error,
                    DetectedAt = DateTime.UtcNow,
                    IsResolved = false
                });
            }
        }

        private async Task CheckMissingFiles(Content content, List<ContentIssueDto> issues)
        {
            var filePath = content.ContentType switch
            {
                ContentType.Video when !string.IsNullOrEmpty(content.ContentUrl) && !content.ContentUrl.StartsWith("http") => content.ContentUrl,
                ContentType.Doc when !string.IsNullOrEmpty(content.ContentDoc) && !content.ContentDoc.StartsWith("http") => content.ContentDoc,
                _ => null
            };

            if (filePath != null)
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
                if (!File.Exists(fullPath))
                {
                    issues.Add(new ContentIssueDto
                    {
                        ContentId = content.ContentId,
                        Title = content.Title,
                        IssueType = "Missing File",
                        Description = $"File does not exist on server: {filePath}",
                        Severity = ContentValidationSeverity.Critical,
                        DetectedAt = DateTime.UtcNow,
                        IsResolved = false
                    });
                }
            }
        }
    }

    
}