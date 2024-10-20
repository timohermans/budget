#!/bin/bash

current_date=$(date '+%Y-%m-%d %H:%M:%S')

last_tag_file=".last_deployed_tag"

log_message() {
  echo "[${current_date}] $1"
}

get_latest_tag() {
  git fetch --tags > /dev/null 2>&1
  git tag --sort=-creatordate | head -n 1
}

# Check if we're in a git repository
if ! git rev-parse --is-inside-work-tree > /dev/null 2>&1; then
    log_message "Error: Not a git repository"
    exit 1
fi

latest_tag=$(get_latest_tag)

if [ -z "$latest_tag" ]; then
    log_message "No tags found in the repository"
    exit 0
fi

# Check if the last known tag file exists
if [ ! -f "$last_tag_file" ]; then
    echo "-1" > $last_tag_file
    log_message "Initialized with tag: $latest_tag"
fi

# Read the last known tag
last_known_tag=$(cat "$last_tag_file")

# Compare tags
if [ "$latest_tag" != "$last_known_tag" ]; then
    log_message "New tag detected: $latest_tag (previous: $last_known_tag)"
    log_message "Deploying..."
    docker compose pull
    docker compose up -d
    source .env
    docker run --network postgres --env "ConnectionStrings__BudgetContext=$ConnectionStrings__BudgetContext" budget-migrations
    log_message "Deployed!"
    echo "$latest_tag" > "$last_tag_file"
    log_message "Saved latest tag"
    exit 0
else
    log_message "No new tags found. Latest tag: $latest_tag"
    log_message "No need to deploy anything..."
    exit 0
fi

