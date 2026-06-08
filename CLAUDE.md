# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Forgotten Family Recipes - A digital family cookbook built to preserve handwritten recipes using AI interpretation. Built to demonstrate DevOps, cloud architecture, and system development skills.

## Tech Stack

- Backend: C# Azure Functions
- IaC: Terraform with modules
- Hosting: Azure Static Web Apps
- Database: Azure Cosmos DB
- Image Storage: Azure Blob Storage
- CI/CD: GitHub Actions
- Security: Azure Key Vault
- AI: Claude API for handwritten text interpretation

## Folder Structure

```
/infra                  <- Terraform
  main.tf
  variables.tf
  terraform.tfvars      <- never commit this
  /modules
    storage/
    functions/
    static-web/
    keyvault/

/backend                <- C# Azure Functions
  /Functions            <- HTTP endpoints
  /Services             <- business logic
  /Models               <- data models
  /Helpers              <- reusable code

/frontend               <- Static Web App

CLAUDE.md
.gitignore
README.md
```

## Code Rules - System Development

- Main shall be short and clean - only calls to methods
- Logic goes in Services, not in Functions
- Use Models to define data structures
- Write comments that explain WHY, not just what - code should be easy to understand when reviewed later or shown in an interview
- Use try/catch and handle errors properly
- Name variables and methods clearly in English

## Code Rules - DevOps/IaC

- Use modules in Terraform
- No hardcoded values - use variables.tf
- Sensitive values handled via Azure Key Vault
- Never commit passwords or keys to Git

## Mockup Data

- Real recipes are not needed during development
- Use fake test data while building and testing
- Replace with real recipes when app is complete

## Git

- Every commit message shall be clear and descriptive in English
- Never commit: `terraform.tfvars`, `.terraform/`, `*.tfstate`, `*.tfstate.backup`, `bin/`, `obj/`, `.env`, `local.settings.json`
