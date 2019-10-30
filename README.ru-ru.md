
# Что такое Cron-lux?

Это .NET-клиент для JSON-RPC- ноды блокчейна [CRYPTOCEAN / cronfoundation.org](http://cronfoundation.org) (см. также [tracker.cron.global](http://tracker.cron.global) и [explorer.cron.global](http://explorer.cron.global) ), который позволяет работать с блокчейном CRON без установки ноды и без передачи приватных ключей по JSON-RPC, как это сделано ранее в [плагине](https://github.com/cronfoundation/neo-plugins/tree/master/RpcSystemAssetTracker).

Cron-lux подписывает транзакцию на своей стороне.
Чтобы подписать транзакцию, ему необходима информация об UTxO - Unspent Tx (Transactions) Output
Он получает её из API explorer.cron.global, но у меня в планах получать её из [плагина](https://github.com/cronfoundation/neo-plugins/tree/master/RpcSystemAssetTracker), благо, что там это доступно из коробки без моих доработок. 
Подписанную транзакцию плагин передаёт через JSON-RPC ноде методом sendrawtransaction.


