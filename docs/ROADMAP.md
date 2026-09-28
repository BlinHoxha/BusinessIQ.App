# BusinessIQ functional roadmap

Based on the supplied *AI Business Intelligence & Advisory Platform — Functional Requirements & Development Roadmap*, version 1.0, September 2026. This summary records product scope; it does not mark the source document's requirements as implemented.

## Current milestone

Foundation in progress: BusinessIQ naming and layered API/Application/Domain/Contracts/Infrastructure projects; a tenant-isolated Business CRUD/archive slice using Framework base controllers, services, repositories, DTOs and AutoMapper; plus shared document ingestion and grounded Q&A. Full organization/location models, memberships, audit conventions, migrations and financial entities remain ahead.

## Roadmap phases

| Phase | Capability |
| --- | --- |
| 0 | Foundation/domain: users, organizations, businesses, locations, industries, currencies, fiscal settings, audit and lifecycle conventions |
| 1 | Authentication, onboarding, organization/business/location management, invitations and permissions |
| 2 | Customers, suppliers, products/services, categories, units, employees |
| 3 | Sales/orders, line items, discounts/taxes, payment methods and imports |
| 4 | Invoices, deterministic totals, numbering, lifecycle, PDFs and multiple payments |
| 5 | Expenses, purchases, recurring costs and supporting evidence |
| 6 | Employees and period-based labor costs |
| 7 | Tax records, obligations, evidence and deterministic jurisdiction rules |
| 8 | Receivables/payables, aging, outstanding and overdue balances |
| 9 | Financial dashboard and verified period comparisons |
| 10 | Consolidated multi-business dashboard with business/location drill-down |
| 11 | Optional industry-specific operational intelligence |
| 12 | AI adviser using authorized records and deterministic financial metrics |
| 13 | Automatic evidence-backed insights |
| 14 | Anomaly detection and investigation |
| 15 | Forecasts with assumptions and uncertainty |
| 16 | Deterministic what-if scenarios with AI explanation |
| 17 | Prioritized improvement plans and actions |
| 18 | Daily/weekly AI business briefs |
| 19 | Inventory, movements, costs and industry extensions |
| 20 | Assets, loans, leases and liabilities |
| 21 | Document classification/extraction with review before posting |
| 22 | Bank, accounting, POS, commerce, payment, payroll/CRM/ERP integrations |
| 23 | Notifications and alerts |
| 24 | Advanced financial/management reports and PDF/Excel/CSV exports |

## Delivery sequence

1. **Foundation:** authentication, organizations, businesses, locations, users and permissions.
2. **Business Data:** customers, suppliers, products/services, categories and employees.
3. **Money In:** sales, invoices, payments and receivables.
4. **Money Out:** expenses, purchases, payables, labor costs and taxes.
5. **Financial Intelligence:** deterministic metrics and single-business dashboard.
6. **Multi-Business Intelligence:** consolidation and business/location comparisons.
7. **AI Adviser:** authorized structured-data context, metrics, evidence and recommendations.
8. **AI Insights:** proactive findings, anomalies, briefs and alerts.
9. **Forecasting & Planning:** forecasts, scenarios, plans, budgets and targets.
10. **Automation & Integrations:** extraction and external systems.
11. **Industry Intelligence:** optional restaurant, construction, salon, retail, commerce and professional-services modules.

The commercially useful MVP proceeds through trustworthy business/financial data and dashboards before the structured-data adviser. Existing document Q&A does not bypass those prerequisites.

## Cross-cutting acceptance criteria

Implement and test organization isolation and role/business/location permissions server-side; financial decimal/rounding/currency accuracy; audits; transactional consistency; data quality; observability; backup/recovery; retention/export/deletion; localization; accessible responsive UI; and explainable AI results. Performance and availability figures in the source roadmap are targets to validate, not current guarantees.

Next specification: technical architecture and database design, including entities, relationships, API boundaries, EF mappings/migrations, caching, jobs, document storage, deployment and implementation sprints.

