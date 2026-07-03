namespace PartnerIntegrationBFFService.Application.Common.Constants
{
    public static class CurrencyConstants
    {
        // Dùng HashSet để tra cứu với tốc độ O(1)
        private static readonly HashSet<string> ValidCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "EUR", "VND", "JPY", "GBP", "AUD", "CAD", "CHF", "CNY", "HKD", "SGD"
        // Thêm các mã khác nếu cần tại đây
    };

        public static bool IsValid(string currencyCode) =>
            !string.IsNullOrWhiteSpace(currencyCode) && ValidCurrencies.Contains(currencyCode);
    }
}
