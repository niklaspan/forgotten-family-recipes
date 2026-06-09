output "function_app_name" {
  description = "Name of the Function App"
  value       = azurerm_linux_function_app.main.name
}

output "function_app_id" {
  description = "Resource ID of the Function App"
  value       = azurerm_linux_function_app.main.id
}

output "default_hostname" {
  description = "Default hostname of the Function App"
  value       = azurerm_linux_function_app.main.default_hostname
}

output "principal_id" {
  description = "Managed identity principal ID — used by root module to grant Key Vault access"
  value       = azurerm_linux_function_app.main.identity[0].principal_id
}
