# Template WPF Blank App

Reusable WPF shell template for internal business applications that need common navigation, settings, editing, logging, and connection management before wiring in real SQL, SharePoint, Microsoft Graph, or Power BI integrations.

## Included template areas

- Shell-style main window with left navigation and shared page host
- Home, Data View, Edit, Bulk Update, Connections, Settings, Saved Profiles, Logs, and About pages
- Mock connection manager with test actions for SSO-oriented endpoints
- Shared data workspace with sample records, detail view, edit form, and bulk update preview
- JSON-based settings/profile persistence under local application data
- Activity logging with export support

## Project structure

- `/home/runner/work/template_wpf_blank_app/template_wpf_blank_app/src/TemplateWpfBlankApp.App` - WPF application
- `ViewModels` - MVVM presentation logic for shell pages
- `Services` - profile persistence, mock connection testing, activity log, and sample workspace services
- `Models` - reusable data contracts for records, profiles, connections, and logs
- `Views/Pages` - page-level user controls consumed by the shell

## How to extend

1. Replace `MockConnectionService` with real adapters for SQL, SharePoint, Graph, or Power BI.
2. Replace `WorkspaceService` sample data loading/saving with real repository or API logic.
3. Add authentication abstractions for your company SSO model and token acquisition flow.
4. Expand bulk update import/export and validation pipelines for your target systems.
5. Add role-based visibility or feature flags per internal app.
