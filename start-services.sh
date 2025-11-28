#!/bin/bash

# Evently Servislerini Başlatma Scripti
# Terminal pencereleri açar ve her servisi başlatır

echo "🚀 Evently Servislerini Başlatıyorum..."

# macOS için Terminal pencereleri aç
osascript <<EOF
tell application "Terminal"
    -- Identity Service
    do script "cd /Users/kerem/Documents/GitHub/Evently/src/IdentityService && dotnet run"
    delay 3
    
    -- Event Service
    do script "cd /Users/kerem/Documents/GitHub/Evently/src/EventService && dotnet run"
    delay 2
    
    -- Ticket Service
    do script "cd /Users/kerem/Documents/GitHub/Evently/src/TicketService && dotnet run"
    delay 2
    
    -- Payment Service
    do script "cd /Users/kerem/Documents/GitHub/Evently/src/PaymentService && dotnet run"
    delay 2
    
    -- Notification Service
    do script "cd /Users/kerem/Documents/GitHub/Evently/src/NotificationService && dotnet run"
    delay 2
    
    -- API Gateway
    do script "cd /Users/kerem/Documents/GitHub/Evently/src/ApiGateway && dotnet run"
    delay 2
    
    -- Angular Client
    do script "cd /Users/kerem/Documents/GitHub/Evently/src/client && npm start"
end tell
EOF

echo "✅ Tüm servisler için terminal pencereleri açıldı!"
echo ""
echo "📊 Erişim Adresleri:"
echo "  - API Gateway: http://localhost:5000"
echo "  - Angular: http://localhost:4200"
echo "  - Seq Logs: http://localhost:5341"

