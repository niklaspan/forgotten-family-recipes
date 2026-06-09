# Free tier — suitable for portfolio/dev; upgrade to Standard for custom auth providers
resource "azurerm_static_web_app" "main" {
  name                = "${var.prefix}-swa"
  resource_group_name = var.resource_group_name
  # Static Web Apps is not available in swedencentral — westeurope is the closest supported region
  location            = "westeurope"
  sku_tier            = "Free"
  sku_size            = "Free"

  tags = var.tags
}
