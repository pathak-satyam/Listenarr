using Listenarr.Application.Metadata;

namespace Listenarr.Tests.Builders
{
    public class SeriesLookupItemBuilder
    {
        private readonly SeriesLookupItem _seriesLookupItem = new()
        {
            Asin = "B0SERIES",
            Name = "Test Series",
            Region = "us"
        };

        public SeriesLookupItemBuilder WithAsin(string value)
        {
            _seriesLookupItem.Asin = value;
            return this;
        }

        public SeriesLookupItemBuilder WithName(string value)
        {
            _seriesLookupItem.Name = value;
            return this;
        }

        public SeriesLookupItemBuilder WithRegion(string value)
        {
            _seriesLookupItem.Region = value;
            return this;
        }

        public SeriesLookupItem Build()
        {
            return _seriesLookupItem;
        }
    }
}
