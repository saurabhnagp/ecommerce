#!/usr/bin/env bash
# Run from your LAPTOP (not EC2) to start/stop the AmCart EC2 instance.
#
# Prerequisites:
#   - AWS CLI configured (aws configure) with your access key
#   - gh CLI installed and authenticated (gh auth login) — for updating EC2_HOST secret
#
# Usage:
#   bash infra/ec2/scripts/ec2-manage.sh start
#   bash infra/ec2/scripts/ec2-manage.sh stop
#   bash infra/ec2/scripts/ec2-manage.sh status
#   bash infra/ec2/scripts/ec2-manage.sh ip          # just print current public IP
#   bash infra/ec2/scripts/ec2-manage.sh ssh          # SSH into the instance
#
# Configuration — edit these or export before running:
INSTANCE_ID="${AMCART_INSTANCE_ID:-i-031a9ebc275946957}"
REGION="${AWS_REGION:-ap-south-1}"
GITHUB_REPO="${AMCART_GITHUB_REPO:-saurabhnagp/ecommerce}"
SSH_KEY="${AMCART_SSH_KEY:-$HOME/.ssh/amcart-ec2.pem}"
SSH_USER="${AMCART_SSH_USER:-ec2-user}"

set -euo pipefail

get_state() {
  aws ec2 describe-instances \
    --instance-ids "$INSTANCE_ID" \
    --region "$REGION" \
    --query 'Reservations[0].Instances[0].State.Name' \
    --output text
}

get_public_ip() {
  aws ec2 describe-instances \
    --instance-ids "$INSTANCE_ID" \
    --region "$REGION" \
    --query 'Reservations[0].Instances[0].PublicIpAddress' \
    --output text
}

wait_for_state() {
  local target="$1"
  echo "Waiting for instance to be $target..."
  while true; do
    local state
    state="$(get_state)"
    echo "  current state: $state"
    if [[ "$state" == "$target" ]]; then break; fi
    sleep 5
  done
}

update_github_secret() {
  local ip="$1"
  if command -v gh &>/dev/null; then
    echo "Updating GitHub secret EC2_HOST to $ip ..."
    echo -n "$ip" | gh secret set EC2_HOST --repo "$GITHUB_REPO"
    echo "Done. GitHub Actions will now deploy to $ip."
  else
    echo ""
    echo "  gh CLI not found. Update EC2_HOST manually in GitHub:"
    echo "  Repo: $GITHUB_REPO -> Settings -> Secrets -> EC2_HOST = $ip"
  fi
}

cmd="${1:-status}"

case "$cmd" in
  start)
    echo "Starting instance $INSTANCE_ID in $REGION ..."
    aws ec2 start-instances --instance-ids "$INSTANCE_ID" --region "$REGION" --output text
    wait_for_state "running"
    sleep 5
    IP="$(get_public_ip)"
    echo ""
    echo "Instance running. Public IP: $IP"
    echo "App URL:  http://$IP/"
    echo ""
    update_github_secret "$IP"
    echo ""
    echo "Docker containers auto-start (restart: unless-stopped)."
    echo "If this is the first start after a fresh launch, SSH in and run:"
    echo "  cd /opt/amcart && bash scripts/pull-and-up.sh"
    ;;

  stop)
    echo "Stopping instance $INSTANCE_ID in $REGION ..."
    echo "(EBS volume is preserved — no data loss. ~\$1.60/mo for 20 GiB gp3.)"
    aws ec2 stop-instances --instance-ids "$INSTANCE_ID" --region "$REGION" --output text
    wait_for_state "stopped"
    echo "Instance stopped. No compute charges while stopped."
    ;;

  status)
    STATE="$(get_state)"
    echo "Instance: $INSTANCE_ID ($REGION)"
    echo "State:    $STATE"
    if [[ "$STATE" == "running" ]]; then
      IP="$(get_public_ip)"
      echo "IP:       $IP"
      echo "App:      http://$IP/"
    fi
    ;;

  ip)
    get_public_ip
    ;;

  ssh)
    IP="$(get_public_ip)"
    if [[ "$IP" == "None" || -z "$IP" ]]; then
      echo "Instance is not running. Start it first: $0 start"
      exit 1
    fi
    echo "Connecting to $SSH_USER@$IP ..."
    ssh -i "$SSH_KEY" "$SSH_USER@$IP"
    ;;

  *)
    echo "Usage: $0 {start|stop|status|ip|ssh}"
    exit 1
    ;;
esac
