output "storage_account_name" {
  description = "Name of the storage account"
  value       = azurerm_storage_account.recipes.name
}

output "storage_account_id" {
  description = "Resource ID of the storage account"
  value       = azurerm_storage_account.recipes.id
}

output "primary_connection_string" {
  description = "Primary connection string — passed to Key Vault, never exposed directly"
  value       = azurerm_storage_account.recipes.primary_connection_string
  sensitive   = true
}

output "recipe_images_container_name" {
  description = "Name of the blob container for recipe images"
  value       = azurerm_storage_container.recipe_images.name
}
