private const string ConnectionString =
    "Server=sql-prod-01.contoso.com,1433;Database=OrdersDb;" +
    "User Id=app_user;Password=StrongP@ssw0rd!2025;" +
    "Encrypt=True;Trust Server Certificate=True;" +  
    "Connection Timeout=30;" +                        // таймаут подключения
    "Command Timeout=60;" +                           // таймаут команд 
    "Pooling=true;" +
    "Min Pool Size=5;" +                              // быстро стартуем при рестарте
    "Max Pool Size=500;" +                            // под высокую нагрузку 
    "Connection Lifetime=300;" +                      // убиваем старые соединения 
    "Connection Reset=true;" +                        // очищаем состояние сессии
    "Load Balance Timeout=300;" +                     // TTL для DNS 
    "Packet Size=4096;" +                             
    "Application Name=OrderService-v2025.03;" +       
    "ConnectRetryCount=3;" +                     
    "ConnectRetryInterval=10;";                      // 10 сек между попытками