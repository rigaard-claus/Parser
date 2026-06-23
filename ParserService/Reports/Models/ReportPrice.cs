using ParserService.Reports.Attributes;

namespace ParserService.Reports.Models
{
    public record ReportPrice(
        [property: ReportColumn("Оператор")] string OperatorName,
        [property: ReportColumn("Страна отправления")] string DepartureCountry,
        [property: ReportColumn("Регион отправления")] string DepartureRegion,
        [property: ReportColumn("Страна прибытия")] string ArrivalCountry,
        [property: ReportColumn("CountryID")] string ArrivalCountryFrontendId,
        [property: ReportColumn("Тур прибытия")] string ArrivalTour,
        [property: ReportColumn("TourID")] string ArrivalTourFrontendId,
        [property: ReportColumn("Регион прибытия")] string ArrivalRegion,
        [property: ReportColumn("RegionID")] string ArrivalRegionFrontendId,
        [property: ReportColumn("Отель")] string HotelName,
        [property: ReportColumn("HotelID")] string HotelFrontendId,
        [property: ReportColumn("Дата")] DateTime Date,
        [property: ReportColumn("Количество ночей")] int Nights,
        [property: ReportColumn("Размещение")] string PlacementName,
        [property: ReportColumn("Цена")] decimal Price,
        [property: ReportColumn("Валюта")] string CurrencyCode
    );
}
