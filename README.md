# Template WPF Blank App

Reusable WPF shell template for internal business applications that need a standard enterprise menu structure before wiring in real SQL, SharePoint, Microsoft Graph, or Power BI integrations.

## Included template areas

- Shell-style main window with left navigation and shared page host
- Home, Data View, Edit, Bulk Update, Sync / Refresh, Connection Manager, Settings, Saved Profiles, Logs / History, and About pages
- Mock connection manager with test actions for SSO-oriented endpoints
- Shared data workspace with sample records, search/filter/sort/paging, detail view, edit form, bulk update preview, and sync placeholders
- JSON-based settings/profile persistence under local application data with favorite/recent profile metadata
- Activity logging with severity filtering and export support
- Primary vs advanced navigation toggles plus administrator-aware actions for bulk execution, log clearing, and connection parameter save flows

## Project structure

- `/home/runner/work/template_wpf_blank_app/template_wpf_blank_app/src/TemplateWpfBlankApp.App` - WPF application
- `ViewModels` - MVVM presentation logic for shell pages
- `Services` - profile persistence, mock connection testing, activity log, and sample workspace services
- `Models` - reusable data contracts for records, profiles, connections, and logs
- `Views/Pages` - page-level user controls consumed by the shell

## How to extend

1. Replace `MockConnectionService` with real adapters for SQL, SharePoint, Graph, or Power BI.
2. Replace `WorkspaceService` sample data loading/saving with real repository or API logic for list/detail/edit, bulk execution, and sync retry queues.
3. Add authentication abstractions for your company SSO model and token acquisition flow.
4. Swap the template CSV/preview placeholders for real CSV/Excel upload, mapping, validation, and execution handlers.
5. Adjust primary/advanced navigation defaults and administrator gating for each internal app.
