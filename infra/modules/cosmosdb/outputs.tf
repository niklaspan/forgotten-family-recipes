output "account_name" {
  description = "Name of the Cosmos DB account"
  value       = azurerm_cosmosdb_account.main.name
}

output "account_id" {
  description = "Resource ID of the Cosmos DB account"
  value       = azurerm_cosmosdb_account.main.id
}

output "endpoint" {
  description = "Document endpoint URL — used by the Functions app to connect"
  value       = azurerm_cosmosdb_account.main.endpoint
}

output "primary_key" {
  description = "Primary master key — passed to Key Vault, never exposed directly"
  value       = azurerm_cosmosdb_account.main.primary_key
  sensitive   = true
}

output "connection_strings" {
  description = "Full connection strings — passed to Key Vault, never exposed directly"
  value       = azurerm_cosmosdb_account.main.connection_strings
  sensitive   = true
}

output "database_name" {
  description = "Name of the SQL database inside the account"
  value       = azurerm_cosmosdb_sql_database.recipes.name
}

output "recipes_container_name" {
  description = "Name of the recipes container"
  value       = azurerm_cosmosdb_sql_container.recipes.name
}
