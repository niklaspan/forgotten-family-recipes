# Access policies are managed in the root module so all grants are visible in one place
resource "azurerm_key_vault" "main" {
  name                = "${var.prefix}-kv"
  resource_group_name = var.resource_group_name
  location            = var.location
  tenant_id           = var.tenant_id
  sku_name            = "standard"

  # 7-day soft-delete window — short enough to not block re-creation during active development
  soft_delete_retention_days = 7

  # Purge protection disabled in dev so the vault can be fully destroyed and recreated
  purge_protection_enabled = false

  tags = var.tags
}
