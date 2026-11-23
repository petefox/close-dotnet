# Changelog

## 1.1.0

- Add typed filter options for documented query parameters while retaining `Query` for advanced filters.
- Add organization-defined field helpers to custom object and activity models.
- Hide example-only custom object properties from IntelliSense while preserving their 1.0 signatures and wire names.

## 1.0.0

- Add named methods for all 308 documented Close REST operations.
- Add typed models for documented payloads and JSON access for dynamic fields and variant shapes.
- Add async streams for documented offset-paginated endpoints outside the original beta scope.
- Add `MapCloseWebhook` through the optional ASP.NET Core package.
- Add streamed attachment uploads and export downloads using a dedicated storage connection.

## 1.0.0-beta.1

- Add named async APIs for core CRM records, advanced search, event history, webhook subscriptions, and the current user.
- Support API keys, OAuth code exchange, refresh and revocation, and ASP.NET Core dependency injection.
- Add offset and cursor paging, field projection, custom fields, raw JSON requests, and bounded retries for reads.
- Include a webhook signature verifier, runnable tutorials, and a coverage inventory for all documented REST operations.
