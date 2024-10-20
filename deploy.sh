#!/bin/bash

image_name="registry.timo-hermans.nl/budget"
current_date=$(date '+%Y-%m-%d %H:%M:%S')
app_name="Budget"

last_tag_file=".last_deployed_tag"

log_message() {
  local level=$1
  local message=$2
  logger -s -t "${app_name}" -p "user.${level,,}" "${message}"
}

# Check if we're in a git repository
if ! git rev-parse --is-inside-work-tree > /dev/null 2>&1; then
    log_message "ERROR" "Error: Not a git repository"
    exit 1
fi

get_image_id() {
  docker images -q "${image_name}"
}

log_message "INFO" "Fetching current ${app_name} image"
current_image_id=$(get_image_id)

log_message "INFO" "Pulling docker compose file"
docker compose pull

log_message "INFO" "Fetching new ${app_name} image"
new_image_id=$(get_image_id)

# Compare tags
if [ "$current_image_id" != "$new_image_id" ]; then
    log_message "INFO" "New $app_name image found: $new_image_id (previous: $current_image_id)"
    log_message "INFO" "Deploying..."
    docker compose up -d
    source .env
    docker run --network postgres --env "ConnectionStrings__BudgetContext=$ConnectionStrings__BudgetContext" budget-migrations
    log_message "INFO" "Deployed!"
    exit 0
else
    log_message "INFO" "No new image found. Latest image: $current_image_id"
    log_message "INFO" "No need to deploy anything..."
    exit 0
fi

