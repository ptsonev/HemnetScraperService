using Microsoft.Net.Http.Headers;

namespace HemnetScraperService.Scraper
{
    public static class HemnetScraperConstants
    {
        public const string INCLUDE = "INCLUDE";
        public const string SORT_NEWEST = "NEWEST";

        public const string SEARCH_OPERATION = "SearchForSaleListings";
        public const string CALCULATOR_OPERATION = "SellerMarketingProductPrices";
        public const string TOTAL_LISTINGS_OPERATION = "MetaSearchForSale";

        public const string SEARCH_QUERY = "query SearchForSaleListings($searchInput: ListingSearchForSaleInput!, $limit: Int!, $offset: Int!, $sort: ListingSearchForSaleSorting, $withBrokerTips: Boolean!, $withTopListing: Boolean!) { searchForSaleListings(search: $searchInput, limit: $limit, offset: $offset, sort: $sort) { total limit offset adTargeting articles { __typename ...GraphArticle } gaCustomParameters { __typename ...GraphCustomParameter } gaPageViewCustomDimensions { __typename ...GraphCustomDimension } listings { __typename ...GraphPropertyListingItemLarge } brokerTips @include(if: $withBrokerTips) { __typename ...GraphBrokerTip } toplistingAd @include(if: $withTopListing) { id listing { __typename ...GraphPropertyListingItemLarge } } recommendations { title trackingIdentifier trackingName listings { __typename ...GraphPropertyListingItemLarge } } exposedSales { brokerAgency { id name brokersCount brokerCustomization { logoCompact: logoUrl(format: BROKER_CUSTOMIZATION_COMPACT) } brokerProfileImages(limit: 3) { url } } ctaText sales { imageUrls sale { streetAddress formattedSoldAt(formatType: SHORT_MONTH_YEAR) sellingPrice { __typename ...GraphMoney } staticMapImageUrl(options: { width: 300 height: 764 } ) } } subTitle title trackingData { eligibleCount locationId myHomeMatch } } } }  fragment GraphArticle on Article { path categoryName title articleImageUrl sponsored }  fragment GraphCustomParameter on GACustomParameter { name value }  fragment GraphCustomDimension on CustomDimension { identifier value }  fragment GraphImageLarge on ListingImage { url(format: ITEMGALLERY_L) }  fragment GraphMoney on Money { amountInCents formatted }  fragment GraphHousingForm on HousingForm { name primaryGroup symbol }  fragment GraphLabel on Label { text identifier category }  fragment GraphGeometryPoint on GeometryPoint { lat long }  fragment GraphImageWidthHd on ListingImage { url(format: WIDTH1024) }  fragment GraphOpenHouse on OpenHouse { description end id isOnlyDate start }  fragment GraphPropertyListingItemLarge on PropertyListing { __typename id activeProducts { __typename code ... on ThumbnailsProduct { images { __typename ...GraphImageLarge } } } askingPrice { __typename ...GraphMoney } area broker { name profileImage { url } } brokerAgency { brokerCustomization { resultCardLogo showLogoInSerp } } fee { __typename ...GraphMoney } formattedLandArea housingForm { __typename ...GraphHousingForm } isSaved isUpcoming labels(categories: [STATE,PRODUCT,FEATURE,TYPE,MEDIA_ATTACHMENT]) { __typename ...GraphLabel } municipality { id fullName } numberOfRooms coordinates { __typename ...GraphGeometryPoint } activePackage streetAddress title isUpcoming livingArea squareMeterPrice { __typename ...GraphMoney } supplementalArea ... on ActivePropertyListing { daysOnHemnet gaClickCustomDimensions { __typename ...GraphCustomDimension } gaPageViewCustomDimensions { __typename ...GraphCustomDimension } publishedAt thumbnail { __typename ...GraphImageWidthHd } upcomingOpenHouses { __typename ...GraphOpenHouse } photoAttribution { photoAgencyName } } ... on ProjectUnit { daysOnHemnet gaClickCustomDimensions { __typename ...GraphCustomDimension } gaPageViewCustomDimensions { __typename ...GraphCustomDimension } publishedAt thumbnail { __typename ...GraphImageWidthHd } upcomingOpenHouses { __typename ...GraphOpenHouse } photoAttribution { photoAgencyName } } ... on Project { daysOnHemnet formattedLandAreaRange formattedLivingAreaRange formattedLowestFee formattedLowestPrice formattedLowestSquareMeterPrice formattedRoomsRange publishedAt thumbnail { __typename ...GraphImageWidthHd } upcomingOpenHouses { __typename ...GraphOpenHouse } projectUnits(limit: 0) { total } photoAttribution { photoAgencyName } } ... on DeactivatedBeforeOpenHousePropertyListing { daysOnHemnet gaClickCustomDimensions { __typename ...GraphCustomDimension } gaPageViewCustomDimensions { __typename ...GraphCustomDimension } publishedAt soldListing { id } thumbnail { __typename ...GraphImageLarge } } ... on DeactivatedPropertyListing { deactivatedAt soldListing { id formattedSoldAt sellingPrice { formatted } formattedPriceChangePercentageWithSign } } }  fragment GraphBrokerTipPropertyListing on PropertyListing { __typename id area broker { name profileImage { url(format: BROKER_PROFILE_LARGE) } } housingForm { __typename ...GraphHousingForm } municipality { fullName } streetAddress title activePackage ... on ActivePropertyListing { askingPrice { __typename ...GraphMoney } livingArea photoAttribution { photoAgencyName } } ... on ProjectUnit { askingPrice { __typename ...GraphMoney } livingArea photoAttribution { photoAgencyName } } ... on Project { formattedLivingAreaRange formattedLowestPrice photoAttribution { photoAgencyName } } }  fragment GraphBrokerTip on BrokerTip { id brokerLogo { url } imageUrls(format: WIDTH1024) propertyListing { __typename ...GraphBrokerTipPropertyListing } title }";
        public const string CALCULATOR_QUERY = "query SellerMarketingProductPrices($locationId: ID!, $askingPrice: Int, $housingFormGroup: HousingFormGroup, $livingAreaInSqm: Float, $productCodes: [PackagePurchase!]!) {\n  sellerMarketingProductPrices(\n    locationId: $locationId\n    askingPrice: $askingPrice\n    productCodes: $productCodes\n    housingFormGroup: $housingFormGroup\n    livingAreaInSqm: $livingAreaInSqm\n  ) {\n    formattedValidThrough\n    prices {\n      code\n      price {\n        amount\n        __typename\n      }\n      __typename\n    }\n       __typename\n  }\n}\n";
        public const string TOTAL_LISTINGS_QUERY = "query MetaSearchForSale($searchInput: ListingSearchForSaleInput!, $limit: Int!, $offset: Int, $sort: ListingSearchForSaleSorting) { searchForSaleListings(search: $searchInput, limit: $limit, offset: $offset, sort: $sort) { total limit offset gaCustomParameters { __typename ...GraphCustomParameter } viewport { __typename ...GraphBoundingBox } tiles { __typename ...GraphTiles } adTargeting } }  fragment GraphCustomParameter on GACustomParameter { name value }  fragment GraphGeometryPoint on GeometryPoint { lat long }  fragment GraphBoundingBox on BoundingBox { northEast { __typename ...GraphGeometryPoint } southWest { __typename ...GraphGeometryPoint } }  fragment GraphTiles on TilesResult { locationsURL locationsHighDPIURL resultsHighDPIURL resultsURL locationsTemplateUrl markersTemplateUrl }";

        public const string GRAPH_URL = "https://www.hemnet.se/graphql";

        public static readonly Dictionary<string, string> DESKTOP_HEADERS = new()
        {
            {HeaderNames.Accept, "*/*"},
            {HeaderNames.AcceptLanguage, "en-US,en;q=0.9"},
            {HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36"},
            {HeaderNames.Origin, "https://www.hemnet.se"},
            {"hemnet-application-version", "www-0.0.1"},
            {"sec-ch-ua", "\"Not.A/Brand\";v=\"8\", \"Chromium\";v=\"114\", \"Google Chrome\";v=\"114\""},
            {"sec-ch-ua-mobile", "?0"},
            {"sec-ch-ua-platform", "\"Windows\""},
            {"Sec-Fetch-Dest", "empty"},
            {"Sec-Fetch-Mode", "cors"},
            {"Sec-Fetch-Site", "same-origin"},
        };

        public static readonly Dictionary<string, string> ANDROID_HEADERS = new()
        {
            {HeaderNames.Accept, "multipart/mixed; deferSpec=20220824, application/json"},
            {HeaderNames.UserAgent, "Hemnet-Android/4.79.0"},
            {"Hemnet-Application-Version", "android-4.79.0"},
        };
    }
}
