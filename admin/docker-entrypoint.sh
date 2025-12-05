#!/bin/sh

# Replace __API_URL__ with actual API URL from environment variable
# Empty API_URL means use relative path (proxy through nginx)
sed -i "s|__API_URL__|${API_URL}|g" /usr/share/nginx/html/config.js

# Start nginx
exec nginx -g 'daemon off;'
