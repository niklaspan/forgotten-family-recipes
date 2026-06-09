# Free tier — suitable for portfolio/dev; upgrade to Standard for custom auth providers
resource "azurerm_static_web_app" "main" {
  name                = "${var.prefix}-swa"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku_tier            = "Free"
  sku_size            = "Free"

  tags = var.tags
}
