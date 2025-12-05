#!/bin/sh

# Replace __API_URL__ with actual API URL from environment variable
sed -i "s|__API_URL__|${API_URL:-http://localhost:5001}|g" /usr/share/nginx/html/config.js

# Start nginx
exec nginx -g 'daemon off;'
