# Azure Functions requires its own dedicated storage account for runtime state
resource "azurerm_storage_account" "functions_runtime" {
  name                     = "${substr(lower(replace(var.prefix, "-", "")), 0, 20)}func"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

  tags = var.tags
}

# Consumption plan (Y1) — scales to zero when idle, 1M free executions per month
resource "azurerm_service_plan" "functions" {
  name                = "${var.prefix}-plan"
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Linux"
  sku_name            = "Y1"

  tags = var.tags
}

resource "azurerm_linux_function_app" "main" {
  name                       = "${var.prefix}-func"
  resource_group_name        = var.resource_group_name
  location                   = var.location
  service_plan_id            = azurerm_service_plan.functions.id
  storage_account_name       = azurerm_storage_account.functions_runtime.name
  storage_account_access_key = azurerm_storage_account.functions_runtime.primary_access_key

  # System-assigned managed identity lets the app authenticate to Key Vault without credentials
  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"

    # Key Vault references — values are resolved at runtime by the managed identity
    ClaudeApiKey             = "@Microsoft.KeyVault(VaultName=${var.key_vault_name};SecretName=ClaudeApiKey)"
    StorageConnectionString  = "@Microsoft.KeyVault(VaultName=${var.key_vault_name};SecretName=StorageConnectionString)"
    CosmosDbConnectionString = "@Microsoft.KeyVault(VaultName=${var.key_vault_name};SecretName=CosmosDbConnectionString)"
  }

  tags = var.tags
}
