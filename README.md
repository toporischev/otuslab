1. Установить сервер базы данных
```Shell
helm install otuslab-pg bitnami/postgresql -f k8s/values.postgres.yaml
```

2. Запустить userservice (миграции будут выполнены при запуске сервиса)
```Shell
helm install userservice k8s/charts/appservice -f src/userservice/values.yaml
```
