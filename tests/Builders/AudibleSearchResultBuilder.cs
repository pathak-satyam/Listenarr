using Listenarr.Application.Metadata;

namespace Listenarr.Tests.Builders
{
    public class AudibleSearchResultBuilder
    {
        private readonly AudibleSearchResult _audibleSearchResult = new()
        {
            Asin = "B0TESTASIN",
            Title = "Test Audiobook",
            Authors = new List<AudibleAuthor> { new() { Name = "Test Author" } },
            Language = "english",
            BookFormat = "unabridged"
        };

        public AudibleSearchResultBuilder WithAsin(string value)
        {
            _audibleSearchResult.Asin = value;
            return this;
        }

        public AudibleSearchResultBuilder WithTitle(string value)
        {
            _audibleSearchResult.Title = value;
            return this;
        }

        public AudibleSearchResultBuilder WithAuthor(string value)
        {
            _audibleSearchResult.Authors = new List<AudibleAuthor> { new() { Name = value } };
            return this;
        }

        public AudibleSearchResultBuilder WithLanguage(string? value)
        {
            _audibleSearchResult.Language = value;
            return this;
        }

        public AudibleSearchResult Build()
        {
            return _audibleSearchResult;
        }
    }
}
