# Hashtrion Platform API — Agentic Payment Authorization

## Overview

The Platform API manages Agent Accounts, Spending Policies, and payment authorization for Hashtrion Agentic Commerce.

It validates AI-initiated payments before delegating XRPL execution to Transaction API.

## Key Features

- Agent Account registration, ownership verification, and platform connections.
- Signing Authority provisioning and activation management.
- Spending Policy management with asset, transaction limit, and expiration controls.
- Checkout-bound UCP payment credential issuance and validation.
- Merchant payment authorization, InfoFeed pricing, and duplicate-payment prevention.
- Payment record creation and execution through Transaction API.

## How It Works

**Agent Setup**

1. Wallet connects through Transaction API to register an Agent Account.
2. Platform API provisions an authority key through Signing API.
3. Wallet signs `SetRegularKey` locally. Transaction API submits the transaction and verifies activation on XRPL.
4. Wallet manages Spending Policies through the same integration.

**Agent Payment**

1. AI Agent requests a payment credential through `POST /api/UcpPayment/issue-credential`.
2. Platform API verifies the signed request, Agent Account, platform connection, and business.
3. Merchant API requests payment through `POST /api/UcpPayment/process-payment`.
4. Platform API validates the credential, recipient, amount, Spending Policy, and Signing Authority.
5. Transaction API coordinates KMS signing, XRPL submission, and payment confirmation. Merchant API handles the order.

Spending Policies are enforced **off-chain by Hashtrion**, not directly by XRPL.

## Authentication

The API uses App Client JWTs, internal API keys, signed Agent HTTP requests, and Merchant API keys for different integration paths.

Payment credentials are short-lived and bound to the Agent Account, business, checkout, and platform.

The UCP payment integration uses Hashtrion's `com.hashtrion.pay` handler.

## Build & Configuration

Requires **.NET 10**, SQL Server, Azure Key Vault, and the Hashtrion Development services.

```bash
dotnet restore PLATFORM.API.sln
dotnet build PLATFORM.API.sln -c Release
ASPNETCORE_ENVIRONMENT=Development dotnet run --project API/API.csproj -c Release --no-build --no-launch-profile
```

Required configuration includes database connection, Azure authentication, Key Vault, service URLs, JWT settings, and UCP security settings.

Configure credentials through Azure App Settings or secure local configuration. Valid Azure credentials and Key Vault access are required at startup.
