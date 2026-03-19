#!/bin/bash
set -e
set -u

# Create amcart_products (amcart_users is created by POSTGRES_DB).
# Runs only on first container init when data dir is empty.

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" <<-EOSQL
  CREATE DATABASE amcart_products OWNER $POSTGRES_USER;
EOSQL
