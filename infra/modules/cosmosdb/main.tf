# Cosmos DB account — serverless means we pay per request only, nothing when idle,
# which is correct for development and low-traffic workloads
resource "azurerm_cosmosdb_account" "main" {
  name                = "${var.prefix}-cosmos"
  resource_group_name = var.resource_group_name
  location            = var.location
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  # EnableServerless switches the account to consumption billing — charged per RU consumed,
  # nothing when idle. This is the correct provider 4.x way; the old capacity.throughput_mode
  # argument was removed. The capacity block (total_throughput_limit) is unrelated — it caps
  # RU/s on provisioned accounts and has no effect here.
  capabilities {
    name = "EnableServerless"
  }

  # Eventual consistency is sufficient for a recipe app; stronger levels cost more RUs
  consistency_policy {
    consistency_level = "Session"
  }

  # Single-region write; geo-replication added here if needed later
  geo_location {
    location          = var.location
    failover_priority = 0
  }

  # Disable public network access in production; kept open here to allow dev access
  # without requiring a private endpoint or VNet peering
  public_network_access_enabled = true

  tags = var.tags
}

resource "azurerm_cosmosdb_sql_database" "recipes" {
  name                = "recipes-db"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.main.name
}

# Partition key is /id — each recipe is its own logical partition, which makes
# point reads (ReadItemAsync with id + partition key) single-partition and therefore
# the cheapest possible Cosmos DB operation. Cross-partition queries (e.g. by chapter)
# are acceptable at this data volume.
resource "azurerm_cosmosdb_sql_container" "recipes" {
  name                = "recipes"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.main.name
  database_name       = azurerm_cosmosdb_sql_database.recipes.name
  partition_key_paths = ["/id"]

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/*"
    }

    excluded_path {
      # Raw OCR text can be large; exclude from index since it is never queried directly
      path = "/rawOcrText/?"
    }
  }
}
