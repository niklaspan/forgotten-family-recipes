# Storage account for recipe images uploaded by users
resource "azurerm_storage_account" "recipes" {
  # Storage account names must be globally unique, lowercase alphanumeric, max 24 chars
  name                     = "${substr(lower(replace(var.prefix, "-", "")), 0, 16)}recipes"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

  # Images are served via SAS tokens or managed identity — no public access needed
  allow_nested_items_to_be_public = false

  tags = var.tags
}

resource "azurerm_storage_container" "recipe_images" {
  name                  = "recipe-images"
  storage_account_id    = azurerm_storage_account.recipes.id
  container_access_type = "private"
}
