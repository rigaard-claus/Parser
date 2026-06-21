namespace ParserService.Reports.Models
{
    public record ReportPrice(
        string OperatorName,
        string DepartureCountry,
        string DepartureRegion,
        string ArrivalCountry,
        string ArrivalCountryFrontendId,
        string ArrivalTour,
        string ArrivalTourFrontendId,
        string ArrivalRegion,
        string ArrivalRegionFrontendId,
        string HotelName,
        string HotelFrontendId,
        DateTime Date,
        int Nights,
        string PlacementName,
        decimal Price,
        string CurrencyCode
    );
}
