#!/usr/bin/env bash
# EC2 User Data script — paste into "Advanced details → User data" when launching a NEW instance.
# This installs Docker, creates /opt/amcart, and pulls/starts the stack.
#
# IMPORTANT: You still need to SCP your .env and infra/ec2 files, OR bake them into an AMI.
# This script only handles Docker + Compose installation so you don't have to do it manually.
#
# After launch, SSH in and:
#   1. scp infra/ec2 files to /opt/amcart (from laptop)
#   2. create .env
#   3. bash /opt/amcart/scripts/pull-and-up.sh

set -euxo pipefail

dnf update -y
dnf install -y docker
systemctl enable --now docker
usermod -aG docker ec2-user

mkdir -p /usr/local/lib/docker/cli-plugins
curl -SL "https://github.com/docker/compose/releases/latest/download/docker-compose-linux-x86_64" \
  -o /usr/local/lib/docker/cli-plugins/docker-compose
chmod +x /usr/local/lib/docker/cli-plugins/docker-compose

mkdir -p /opt/amcart
chown ec2-user:ec2-user /opt/amcart

echo "Bootstrap complete. Docker $(docker --version) and Compose $(docker compose version) installed."
