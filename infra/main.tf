terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
  required_version = ">= 1.5.0"
}

provider "azurerm" {
  features {
    key_vault {
      # Allow vault to be fully deleted in dev — purge protection would block re-creation
      purge_soft_delete_on_destroy    = true
      recover_soft_deleted_key_vaults = true
    }
  }
  subscription_id = var.subscription_id
}

# Used to get the current deployer's object_id for Key Vault access
data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "main" {
  name     = "${var.prefix}-rg"
  location = var.location
  tags     = var.tags
}

module "storage" {
  source              = "./modules/storage"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  prefix              = var.prefix
  tags                = var.tags
}

module "keyvault" {
  source              = "./modules/keyvault"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  prefix              = var.prefix
  tenant_id           = var.tenant_id
  tags                = var.tags
}

module "functions" {
  source              = "./modules/functions"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  prefix              = var.prefix
  key_vault_name      = module.keyvault.key_vault_name
  tags                = var.tags
}

module "static_web" {
  source              = "./modules/static-web"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  prefix              = var.prefix
  tags                = var.tags
}

# --- Key Vault access policies ---
# Kept here in the root rather than the module so all access grants are visible in one place

# Terraform deployer gets full access to manage secrets during provisioning
resource "azurerm_key_vault_access_policy" "deployer" {
  key_vault_id = module.keyvault.key_vault_id
  tenant_id    = var.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = ["Get", "List", "Set", "Delete", "Recover", "Backup", "Restore", "Purge"]
}

# Function App managed identity gets read-only access at runtime — no credentials needed in code
resource "azurerm_key_vault_access_policy" "function_app" {
  key_vault_id = module.keyvault.key_vault_id
  tenant_id    = var.tenant_id
  object_id    = module.functions.principal_id

  secret_permissions = ["Get", "List"]
}

# --- Key Vault secrets ---
# Values come from terraform.tfvars, which is gitignored — never hardcoded here

resource "azurerm_key_vault_secret" "claude_api_key" {
  name         = "ClaudeApiKey"
  value        = var.claude_api_key
  key_vault_id = module.keyvault.key_vault_id

  # Deployer access policy must exist before secrets can be written
  depends_on = [azurerm_key_vault_access_policy.deployer]
}

resource "azurerm_key_vault_secret" "storage_connection_string" {
  name         = "StorageConnectionString"
  value        = module.storage.primary_connection_string
  key_vault_id = module.keyvault.key_vault_id

  depends_on = [azurerm_key_vault_access_policy.deployer]
}
