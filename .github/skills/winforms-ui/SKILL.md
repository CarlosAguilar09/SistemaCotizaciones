---
name: winforms-ui
description: WinForms UI creation patterns, layout rules, and theming conventions for SistemaCotizaciones. Use this when creating or modifying views, fixing layout issues, styling controls, or working with the AppTheme system.
---

# WinForms UI Patterns

## Golden Rules

1. **All controls are built programmatically** — no Designer files for Views. Only `MainForm` uses the Designer.
2. **Always call `AppTheme.ApplyTo(this)` first** in every view's `InitializeControls()`.
3. **Use `AppTheme.Style*` methods** — never set colors, fonts, or sizes directly on controls.
4. **All user-facing text is in Spanish.**

## Layout System

### Docking (Primary Layout Strategy)

UserControls are hosted in `MainForm.pnlContent` with `Dock = Fill`. Build layouts using nested panels with docking.

**Docking order matters.** Controls are docked in reverse `Controls` collection order (last added docks first). Add controls in this order:

```csharp
Controls.Add(fillContent);     // Fill — added first, processed last
Controls.Add(bottomBar);       // Bottom — processed second
Controls.Add(topToolbar);      // Top — processed first
```

### Critical: No Anchor with Absolute Position

**Never** do this in a UserControl:
```csharp
// BAD — control flies off-screen because UserControl starts at default size
button.Location = new Point(500, 10);
button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
```

**Instead**, use dock-based sub-panels:
```csharp
// GOOD — use AppTheme layout helpers
var (toolbar, flow) = AppTheme.CreateToolbar();
flow.Controls.Add(button);
Controls.Add(toolbar); // Dock = Top
```

### Do Not Call BringToFront() on Docked Panels

`BringToFront()` changes the control's index in `Controls`, which changes docking resolution order and causes overlaps.

## AppTheme Layout Helpers

Use these instead of manual panel creation:

| Helper | Returns | Dock | Use For |
|--------|---------|------|---------|
| `CreateToolbar()` | `(Panel, FlowLayoutPanel)` | Top | Search bars, filter controls, action buttons |
| `CreateButtonBar()` | `(Panel, FlowLeft, FlowRight)` | Bottom | Save/Cancel, navigation buttons |
| `CreateFormLayout(rows)` | `TableLayoutPanel` | Fill | Label-input form grids |
| `AddFormRow(table, row, label, control)` | void | — | Add a labeled row to a form layout |
| `CreateSeparator()` | `Panel` | Top | Thin horizontal divider line |
| `CreateButton(text, width)` | `Button` | — | Standard button with consistent sizing |

## Styling Methods

Always use the appropriate method:

| Control | Styling Method |
|---------|---------------|
| Primary action button | `StylePrimaryButton(btn)` |
| Secondary/cancel button | `StyleSecondaryButton(btn)` |
| Delete/destructive button | `StyleDangerButton(btn)` |
| DataGridView | `StyleDataGridView(dgv)` |
| TextBox | `StyleTextBox(txt)` |
| ComboBox | `StyleComboBox(cmb)` |
| NumericUpDown | `StyleNumericUpDown(nud)` |
| GroupBox | `StyleGroupBox(grp)` |
| Currency column (DGV) | `StyleCurrencyColumn(col)` |
| Date column (DGV) | `StyleDateColumn(col)` |

## Spacing and Dimensions

Use `AppTheme` constants — never hardcode pixel values for standard spacings:

```
SpaceXS  = 4    tight spacing
SpaceSM  = 8    small gaps
SpaceMD  = 12   medium gaps
SpaceLG  = 16   standard padding
SpaceXL  = 24   section spacing
SpaceXXL = 32   large padding

ButtonHeight    = 34
ButtonWidthSM   = 90
ButtonWidthMD   = 110
ButtonWidthLG   = 140
ToolbarHeight   = 48
ButtonBarHeight = 56
FormRowHeight   = 36
InputHeight     = 28
```

## View Structure Template

Every view follows this skeleton:

```csharp
namespace SistemaCotizaciones.Views;

public class MyView : UserControl
{
    private readonly Navigator _navigator;

    public MyView(Navigator navigator)
    {
        _navigator = navigator;
        InitializeControls();
    }

    private void InitializeControls()
    {
        AppTheme.ApplyTo(this);

        // 1. Create layout containers (toolbar, form, button bar)
        // 2. Create and style controls
        // 3. Wire event handlers
        // 4. Add to Controls collection (Fill first, Bottom, then Top)
    }
}
```

## IRefreshable Pattern

Views that load data (typically list views) implement `IRefreshable` so the Navigator can refresh them on back-navigation:

```csharp
public class MyListView : UserControl, IRefreshable
{
    public void RefreshData() => LoadData();
}
```

## Navigation

```csharp
// Navigate forward
_navigator.NavigateTo(new TargetView(_navigator, args), "Título en Español");

// Navigate back
_navigator.GoBack();
```

## Error Handling in Views

Use `ErrorHelper` for all user-facing errors:

```csharp
try { /* operation */ }
catch (Exception ex) { ErrorHelper.ShowError("No se pudo guardar el registro.", ex); }
```

## Confirmation Dialogs

Use standard `MessageBox` with Spanish text:

```csharp
var result = MessageBox.Show(
    "¿Está seguro de eliminar este registro?",
    "Confirmar eliminación",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Warning);
if (result != DialogResult.Yes) return;
```
