output "static_web_app_name" {
  description = "Name of the Static Web App"
  value       = azurerm_static_web_app.main.name
}

output "static_web_app_id" {
  description = "Resource ID of the Static Web App"
  value       = azurerm_static_web_app.main.id
}

output "default_host_name" {
  description = "Default hostname assigned by Azure"
  value       = azurerm_static_web_app.main.default_host_name
}

output "api_key" {
  description = "Deployment API key — used by GitHub Actions to publish the frontend"
  value       = azurerm_static_web_app.main.api_key
  sensitive   = true
}
