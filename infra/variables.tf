variable "subscription_id" {
  description = "Azure subscription ID"
  type        = string
}

variable "tenant_id" {
  description = "Azure Active Directory tenant ID"
  type        = string
}

variable "prefix" {
  description = "Short prefix applied to all resource names — keep it unique and under 10 characters (e.g. ffr-dev)"
  type        = string
}

variable "location" {
  description = "Azure region for all resources"
  type        = string
  default     = "swedencentral"
}

variable "claude_api_key" {
  description = "Anthropic Claude API key — stored in Key Vault, never logged or outputted"
  type        = string
  sensitive   = true
}

variable "tags" {
  description = "Tags applied to all resources for cost tracking and organisation"
  type        = map(string)
  default = {
    project    = "forgotten-family-recipes"
    managed_by = "terraform"
  }
}
