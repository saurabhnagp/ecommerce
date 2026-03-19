#!/usr/bin/env bash
# Run on EC2: logs into ECR and restarts the stack.
# Usage: from repo copy, bash scripts/pull-and-up.sh
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
ENV_FILE="${1:-.env}"
if [[ ! -f "$ENV_FILE" ]]; then
  echo "Missing $ENV_FILE — copy from .env.example and fill secrets."
  exit 1
fi

# Parse ECR login vars without sourcing the whole file (passwords may contain special chars)
AWS_REGION="$(grep -E '^[[:space:]]*AWS_REGION=' "$ENV_FILE" | tail -1 | cut -d= -f2- | tr -d '\r' | sed 's/^["'\'']//;s/["'\'']$//')"
ECR_REGISTRY="$(grep -E '^[[:space:]]*ECR_REGISTRY=' "$ENV_FILE" | tail -1 | cut -d= -f2- | tr -d '\r' | sed 's/^["'\'']//;s/["'\'']$//')"
if [[ -z "$AWS_REGION" || -z "$ECR_REGISTRY" ]]; then
  echo "AWS_REGION and ECR_REGISTRY must be set in $ENV_FILE"
  exit 1
fi

aws ecr get-login-password --region "${AWS_REGION}" | docker login --username AWS --password-stdin "${ECR_REGISTRY}"
docker compose --env-file "$ENV_FILE" pull
docker compose --env-file "$ENV_FILE" up -d
